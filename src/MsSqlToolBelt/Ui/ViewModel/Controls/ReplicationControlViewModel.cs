using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

internal partial class ReplicationControlViewModel : ViewModelBase, IConnection
{
    /// <summary>
    /// The instance for the interaction with the replication data
    /// </summary>
    private ReplicationManager? _manager;

    /// <summary>
    /// Contains the value which indicates if the data already loaded
    /// </summary>
    private bool _dataLoaded;

    #region View Properties

    /// <summary>
    /// The list with the tables
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TableEntry> _tables = new();
    
    /// <summary>
    /// Backing field for <see cref="SelectedTable"/>
    /// </summary>
    private TableEntry? _selectedTable;

    /// <summary>
    /// Gets or sets the selected table
    /// </summary>
    public TableEntry? SelectedTable
    {
        get => _selectedTable;
        set
        {
            SetProperty(ref _selectedTable, value);

            if (_manager == null)
                return;

            _manager.SelectedTable = value;

            if (value != null && value.Columns.Any())
            {
                SetColumns();
                SetIndexes();
            }
            else
                EnrichData();
        }
    }

    /// <summary>
    /// The list with the table columns
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColumnEntry> _columns = new();

    /// <summary>
    /// The list with the indices
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IndexEntry> _indexes = new();

    /// <summary>
    /// Backing field for <see cref="Filter"/>
    /// </summary>
    private string _filter = string.Empty;

    /// <summary>
    /// Gets or sets the filter
    /// </summary>
    public string Filter
    {
        get => _filter;
        set
        {
            SetProperty(ref _filter, value);
            if (string.IsNullOrEmpty(value))
                FilterList();
        }
    }

    /// <summary>
    /// The list header
    /// </summary>
    [ObservableProperty]
    private string _headerList = "Tables";

    /// <summary>
    /// The column header
    /// </summary>
    [ObservableProperty]
    private string _headerColumns = "Columns";

    /// <summary>
    /// The index header
    /// </summary>
    [ObservableProperty]
    private string _headerIndex = "Indexes";

    /// <summary>
    /// The value which indicates if the info panel should be shown
    /// </summary>
    [ObservableProperty]
    private bool _showInfo = true;

    /// <summary>
    /// The value which indicates if the control is enabled
    /// </summary>
    [ObservableProperty]
    private bool _controlEnabled;
    #endregion

    /// <summary>
    /// The command to reload the table types
    /// </summary>
    public ICommand ReloadCommand => new RelayCommand(() =>
    {
        _dataLoaded = false;
        LoadData();
    });

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        // Clear the current result
        Tables = new ObservableCollection<TableEntry>();
        Columns = new ObservableCollection<ColumnEntry>();

        // Reset the manager
        _manager?.Dispose();
        _manager = new ReplicationManager(dataSource, database);

        // Reset the info values
        ShowInfo = false;
        ControlEnabled = true;

        // Reset the data loaded value
        _dataLoaded = false;
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        _manager?.Dispose();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    public async void LoadData()
    {
        if (_dataLoaded || _manager == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the replication information...");

        try
        {
            await _manager.LoadTablesAsync();

            _dataLoaded = true;

            FilterList();
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
    /// Filters the result
    /// </summary>
    [RelayCommand]
    private void FilterList()
    {
        if (_manager == null)
            return;

        var result = string.IsNullOrEmpty(Filter)
            ? _manager.Tables
            : _manager.Tables.Where(w => w.Name.ContainsIgnoreCase(Filter)).ToList();

        Tables = result.ToObservableCollection();
        HeaderList = Tables.Count > 1
            ? $"{Tables.Count} tables"
            : Tables.Count == 0
                ? "Tables"
                : "1 table";

        // Show the info if there are no tables available
        ShowInfo = Tables.Count == 0;
        ControlEnabled = Tables.Any();
    }

    /// <summary>
    /// Enriches the selected table type
    /// </summary>
    private async void EnrichData()
    {
        if (_manager?.SelectedTable == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the columns...");

        try
        {
            await _manager.EnrichTableAsync();

            // Set the columns
            SetColumns();
            SetIndexes();
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
    /// Sets the columns
    /// </summary>
    private void SetColumns()
    {
        if (_manager?.SelectedTable == null)
            return;

        Columns = _manager.SelectedTable.Columns.ToObservableCollection();

        HeaderColumns = Columns.Count > 1 ? $"{Columns.Count} columns" : "1 column";
    }

    /// <summary>
    /// Sets the indexes
    /// </summary>
    private void SetIndexes()
    {
        if (_manager?.SelectedTable == null)
            return;

        Indexes = _manager.SelectedTable.Indexes.ToObservableCollection();
        HeaderIndex = Indexes.Count > 1 ? $"{Indexes.Count} indexes" : "1 index";
    }
}