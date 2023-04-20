using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.DefinitionExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the logic for the interaction with the types
/// </summary>
internal class DefinitionExportManager : IDisposable
{
    /// <summary>
    /// Occurs when the export makes progress
    /// </summary>
    public event EventHandler<string>? Progress;
    
    /// <summary>
    /// Contains the value which indicates if the class was already disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The instance for the interaction with the database
    /// </summary>
    private readonly DefinitionExportRepo _repo;

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager;

    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private readonly SettingsManager _settingsManager;

    /// <summary>
    /// Gets the list with the objects
    /// </summary>
    public List<DefinitionExportObject> Objects { get; private set; } = new();
    
    /// <summary>
    /// Gets the list with the different objects types
    /// </summary>
    public List<string> Types { get; private set; } = new();

    /// <summary>
    /// Gets the list with the tables
    /// </summary>
    public List<DefinitionExportObject> Tables { get; private set; } = new();

    /// <summary>
    /// Creates a new instance of the <see cref="DefinitionExportManager"/>
    /// </summary>
    /// <param name="settingsManager">The instance of the settings manager</param>
    /// <param name="dataSource">The name / path of the MSSQL server</param>
    /// <param name="database">The name of the database</param>
    public DefinitionExportManager(SettingsManager settingsManager, string dataSource, string database)
    {
        _repo = new DefinitionExportRepo(dataSource, database);
        _tableManager = new TableManager(dataSource, database);
        _settingsManager = settingsManager;
    }

    #region Objects
    /// <summary>
    /// Loads all available objects and stores them into <see cref="Objects"/>. The list with the types (<see cref="Types"/> will also be filled)
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadObjectsAsync()
    {
        var result = await _repo.LoadObjectsAsync();

        await _settingsManager.LoadFilterAsync();
        Objects = _settingsManager.FilterList.Any()
            ? result.Where(w => w.Name.IsValid(_settingsManager.FilterList)).Select(s => (DefinitionExportObject) s)
                .OrderBy(o => o.Type).ThenBy(t => t.Name).ToList()
            : result.Select(s => (DefinitionExportObject) s).OrderBy(o => o.Type).ThenBy(t => t.Name).ToList();

        Types.Add("All");
        Types.AddRange(result.Select(s => s.TypeName).Distinct());
    }

    /// <summary>
    /// Exports the selected entries
    /// </summary>
    /// <param name="objects">The list with the export entries</param>
    /// <param name="objectList">The list with the custom objects</param>
    /// <param name="exportDir">The export directory</param>
    /// <param name="createTypeDir"><see langword="true"/> to create a sub directory for each type, otherwise <see langword="false"/></param>
    /// <returns>The awaitable task</returns>
    public async Task ExportObjectsAsync(List<DefinitionExportObject> objects, string objectList, string exportDir, bool createTypeDir)
    {
        if (!string.IsNullOrEmpty(objectList))
            SetExportFlag(objects, objectList);

        var count = 1;
        var totalCount = objects.Count(c => c.Export);
        foreach (var entry in objects.OrderBy(o => o.Name).Where(w => w.Export))
        {
            Progress?.Invoke(this, $"{count++} of {totalCount} > Export '{entry.Name}' definition...");
            await ExportObjectsAsync(entry, exportDir, createTypeDir);
        }
    }

    /// <summary>
    /// Exports the specified object
    /// </summary>
    /// <param name="obj">The object which should be exported</param>
    /// <param name="exportDir">The export directory</param>
    /// <param name="createTypeDir"><see langword="true"/> to create a sub directory for each type, otherwise <see langword="false"/></param>
    /// <returns>The awaitable task</returns>
    private static async Task ExportObjectsAsync(DefinitionExportObject obj, string exportDir, bool createTypeDir)
    {
        var name = $"{obj.Name.Trim()}.sql";
        var path = Path.Combine(exportDir, name);

        if (createTypeDir)
        {
            var subDir = Path.Combine(exportDir, obj.Type);
            Directory.CreateDirectory(subDir);
            path = Path.Combine(subDir, name);
        }

        await File.WriteAllTextAsync(path, obj.OriginalObject.Definition, Encoding.UTF8);
    }

    /// <summary>
    /// Sets the export flag
    /// </summary>
    /// <param name="objects">The list with the objects</param>
    /// <param name="objectList">The object list (custom entries)</param>
    private static void SetExportFlag(IEnumerable<DefinitionExportObject> objects, string objectList)
    {
        var tmpList = objectList.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Distinct()
            .ToList();

        foreach (var entry in objects.Where(w => !w.Export))
        {
            entry.Export = tmpList.Any(a => a.EqualsIgnoreCase(entry.Name));
        }
    }

    #endregion

    #region Tables

    /// <summary>
    /// Loads all available tables and stores them into <see cref="Tables"/>
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadTablesAsync()
    {
        var tables = await _tableManager.LoadTablesAsync();

        Tables = tables.Select(s => (DefinitionExportObject) s).ToList();
    }

    /// <summary>
    /// Exportes the tables
    /// </summary>
    /// <param name="tables">The list with the export entries</param>
    /// <param name="exportDir">The export directory</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The awaitable task</returns>
    public async Task ExportTablesAsync(List<DefinitionExportObject> tables, string exportDir, CancellationToken ct)
    {
        _repo.Progress += Progress;

        // Load the definitions
        var definitions = await _repo.LoadTableDefinitionAsync(tables.Where(w => w.Export).ToList(), ct);

        // export the definitions
        var count = 1;
        var totalCount = definitions.Count;
        foreach (var definition in definitions)
        {
            Progress?.Invoke(this, $"{count++} of {totalCount} > Export '{definition.Name}' definition...");

            var path = Path.Combine(exportDir, $"{definition.Name}.sql");

            await File.WriteAllTextAsync(path, definition.Definition, ct);
        }

        _repo.Progress -= Progress;
    }
    #endregion

    /// <summary>
    /// Releases all resources used by the <see cref="DefinitionExportManager"/>
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _repo.Dispose();

        _disposed = true;
    }
}