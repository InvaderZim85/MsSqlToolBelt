using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MsSqlToolBelt.Ui.Common;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the functions for <see cref="View.Windows.LoadTableWindow"/>
/// </summary>
internal partial class LoadTableWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private TableQueryManager? _manager;

    /// <summary>
    /// The action to set the SQL text
    /// </summary>
    private Action<string>? _setSqlText;

    /// <summary>
    /// Contains the selected table
    /// </summary>
    private TableEntry? _selectedTable;

    /// <summary>
    /// Contains the generated SQL
    /// </summary>
    private string _sql = string.Empty;

    #region Properties
    /// <summary>
    /// Gets or sets the list with the limit values
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IdTextEntry> _limitValues = new();

    /// <summary>
    /// Gets or sets the selected limit value
    /// </summary>
    [ObservableProperty]
    private IdTextEntry? _selectedLimit;

    /// <summary>
    /// Gets or sets the duration
    /// </summary>
    [ObservableProperty]
    private string _duration = string.Empty;

    /// <summary>
    /// Gets or sets the row count
    /// </summary>
    [ObservableProperty]
    private string _rowCount = "Rows";

    /// <summary>
    /// Gets or sets the load info
    /// </summary>
    [ObservableProperty]
    private string _loadInfo = string.Empty;

    /// <summary>
    /// Gets or sets the data view
    /// </summary>
    [ObservableProperty]
    private DataView _view = new();

    /// <summary>
    /// Gets or sets the name of the selected table
    /// </summary>
    [ObservableProperty]
    private string _tableName = string.Empty;
    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="dataSource">The name / path of the MS SQL database</param>
    /// <param name="database">The name of the database</param>
    /// <param name="selectedTable">The selected table</param>
    /// <param name="setSqlText">The action to set the SQL text</param>
    public void InitViewModel(string dataSource, string database, TableEntry selectedTable, Action<string> setSqlText)
    {
        _manager = new TableQueryManager(dataSource, database);

        _setSqlText = setSqlText;

        _selectedTable = selectedTable;

        TableName = $"{selectedTable.Schema}.{selectedTable.Name}";

        // Set the limit values
        LimitValues = _manager.LimitList.ToObservableCollection();
        SelectedLimit = LimitValues.FirstOrDefault(f => f.Id == 1000);

        _sql = _manager.CreateQuery(selectedTable, SelectedLimit ?? new IdTextEntry(1000, string.Empty));
        _setSqlText?.Invoke(_sql);
    }

    /// <summary>
    /// Executes the generated query
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ExecuteQueryAsync()
    {
        if (_manager == null || _selectedTable == null)
            return;

        var controller = await ShowProgressAsync("Executing", "Please wait while executing the query...");

        try
        {
            var result =
                await _manager.LoadTableDataAsync(_selectedTable, SelectedLimit ?? new IdTextEntry(1000, string.Empty));

            LoadInfo = $"{result.Rows:N0} row(s) in {result.Duration}";

            View = _manager.ResultTable.DefaultView;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Copies the generated SQL to the clipboard
    /// </summary>
    [RelayCommand]
    private void CopyContent()
    {
        CopyToClipboard(_sql);
    }

    /// <summary>
    /// Occurs when the user selects another limit
    /// </summary>
    /// <param name="value">The selected limit</param>
    partial void OnSelectedLimitChanged(IdTextEntry? value)
    {
        if (_manager == null || _selectedTable == null)
            return;

        _sql = _manager.CreateQuery(_selectedTable, value ?? new IdTextEntry(1000, string.Empty));
        _setSqlText?.Invoke(_sql);
    }
}