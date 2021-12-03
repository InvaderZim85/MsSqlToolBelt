﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.DefinitionExport;
using Serilog;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.Business
{
    /// <summary>
    /// Provides the functions for the interaction with the object definitions
    /// </summary>
    internal sealed class DefinitionExportManager
    {
        /// <summary>
        /// The instance for the interaction with the database
        /// </summary>
        private readonly DefinitionRepo _repo;

        /// <summary>
        /// Occurs when progress was made
        /// </summary>
        public event EventHandler<string> Progress;

        /// <summary>
        /// Occurs when a progress was made (short info message)
        /// </summary>
        public event EventHandler<string> ProgressShort; 

        /// <summary>
        /// Creates a new instance of the <see cref="DefinitionExportManager"/>
        /// </summary>
        /// <param name="connector">The instance for the database connection</param>
        public DefinitionExportManager(Connector connector)
        {
            _repo = new DefinitionRepo(connector);
        }

        /// <summary>
        /// Loads all available procedures
        /// </summary>
        /// <returns>The list with the procedures</returns>
        public async Task<List<DefinitionEntry>> LoadProceduresAsync()
        {
            return await _repo.LoadProceduresAsync();
        }

        /// <summary>
        /// Exports the definition of the specified objects into the specified directory
        /// </summary>
        /// <param name="exportDirectory">The path of the export directory</param>
        /// <param name="objectList">The object list</param>
        /// <param name="defList">The list with the definition entries</param>
        /// <returns>The awaitable task</returns>
        public async Task ExportDefinitions(string exportDirectory, string objectList, List<DefinitionEntry> defList)
        {
            if (string.IsNullOrWhiteSpace(exportDirectory))
                throw new ArgumentNullException(nameof(exportDirectory));

            if (!Directory.Exists(exportDirectory))
                throw new DirectoryNotFoundException("The specified directory doesn't exist.");

            // If the object list is empty, skip any further action
            if (string.IsNullOrWhiteSpace(objectList) && !defList.Any(a => a.Export))
                return;

            // Combine the two lists
            var objectNames = GetObjectNames(objectList);
            var tmpList = new List<string>(objectNames.Count + defList.Count(c => c.Export));
            tmpList.AddRange(objectNames);
            tmpList.AddRange(defList.Where(w => w.Export).Select(s => s.Name));
            // Remove duplicated entries
            tmpList = tmpList.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var loadCount = 0;
            var errorCount = 0;
            foreach (var entry in tmpList)
            {
                WriteMessage($"Load definition of '{entry}'");
                try
                {
                    var content = await _repo.LoadDefinitionAsync(entry);
                    ExportContent(exportDirectory, entry, content);
                    WriteMessage("Definition exported.");
                    loadCount++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Can't gather definition of object '{objectName}'", entry);
                    WriteMessage($"Can't load definition of '{entry}'");
                    errorCount++;
                }
            }

            WriteMessage($"{loadCount:N0} definition loaded. {errorCount:N0} failed to load.");
        }

        /// <summary>
        /// Gets the list with the object names
        /// </summary>
        /// <param name="objectList">The user input</param>
        /// <returns>The list with the object names</returns>
        private static IReadOnlyCollection<string> GetObjectNames(string objectList)
        {
            var content = objectList.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            return content.Distinct(StringComparer.OrdinalIgnoreCase).Select(s => s.Trim()).ToList(); // Remove duplicates
        }

        /// <summary>
        /// Exports the definition content
        /// </summary>
        /// <param name="exportDirectory">The export directory</param>
        /// <param name="objectName">The name of the object</param>
        /// <param name="definition">The definition of the object</param>
        private static void ExportContent(string exportDirectory, string objectName, List<string> definition)
        {
            if (string.IsNullOrWhiteSpace(exportDirectory))
                throw new ArgumentNullException(nameof(exportDirectory));

            if (!Directory.Exists(exportDirectory))
                throw new DirectoryNotFoundException("The specified directory doesn't exist.");

            if (string.IsNullOrWhiteSpace(objectName))
                throw new ArgumentNullException(nameof(objectName));

            if (definition == null || !definition.Any())
                return;

            // Beautify the definition list > remove all new line arguments
            definition = definition.Select(s => s.Replace(Environment.NewLine, "")).ToList();

            var filePath = Path.Combine(exportDirectory, $"{objectName}.sql");

            File.WriteAllLines(filePath, definition, Encoding.UTF8);
        }

        /// <summary>
        /// Writes a message
        /// </summary>
        /// <param name="msg">The message which should be written</param>
        /// <param name="info">true if it's an info message, false if it's an error message</param>
        private void WriteMessage(string msg, bool info = true)
        {
            ProgressShort?.Invoke(this, msg);
            Progress?.Invoke(this, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {(info ? "INFO" : "ERROR")} | {msg}");
        }
    }
}
