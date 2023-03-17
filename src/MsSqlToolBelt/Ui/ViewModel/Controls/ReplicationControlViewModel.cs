using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

internal class ReplicationControlViewModel : ViewModelBase, IConnection
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
    /// Backing field for <see cref="Tables"/>
    /// </summary>
    private ObservableCollection<TableEntry> _tables = new();

    /// <summary>
    /// Gets or sets the list with the tables
    /// </summary>
    public ObservableCollection<TableEntry> Tables
    {
        get => _tables;
        private set => SetProperty(ref _tables, value);
    }

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
    /// Backing field for <see cref="Columns"/>
    /// </summary>
    private ObservableCollection<ColumnEntry> _columns = new();

    /// <summary>
    /// Gets or sets the list with the table columns
    /// </summary>
    public ObservableCollection<ColumnEntry> Columns
    {
        get => _columns;
        private set => SetProperty(ref _columns, value);
    }

    /// <summary>
    /// Backing field for <see cref="Indexes"/>
    /// </summary>
    private ObservableCollection<IndexEntry> _indexes = new();

    /// <summary>
    /// Gets or sets the list with the indexes
    /// </summary>
    public ObservableCollection<IndexEntry> Indexes
    {
        get => _indexes;
        private set => SetProperty(ref _indexes, value);
    }

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
    /// Backing field for <see cref="HeaderList"/>
    /// </summary>
    private string _headerList = "Tables";

    /// <summary>
    /// Gets or sets the list header
    /// </summary>
    public string HeaderList
    {
        get => _headerList;
        private set => SetProperty(ref _headerList, value);
    }

    /// <summary>
    /// Backing field for <see cref="HeaderColumns"/>
    /// </summary>
    private string _headerColumns = "Columns";

    /// <summary>
    /// Gets or sets the column header
    /// </summary>
    public string HeaderColumns
    {
        get => _headerColumns;
        private set => SetProperty(ref _headerColumns, value);
    }

    /// <summary>
    /// Backing field for <see cref="HeaderIndex"/>
    /// </summary>
    private string _headerIndex = "Indexes";

    /// <summary>
    /// Gets or sets the index header
    /// </summary>
    public string HeaderIndex
    {
        get => _headerIndex;
        private set => SetProperty(ref _headerIndex, value);
    }

    /// <summary>
    /// Backing field for <see cref="ShowInfo"/>
    /// </summary>
    private bool _showInfo = true;

    /// <summary>
    /// Gets or sets the value which indicates if the info panel should be shown
    /// </summary>
    public bool ShowInfo
    {
        get => _showInfo;
        set => SetProperty(ref _showInfo, value);
    }

    /// <summary>
    /// Backing field for <see cref="ControlEnabled"/>
    /// </summary>
    private bool _controlEnabled;

    /// <summary>
    /// Gets or sets the value which indicates if the control is enabled
    /// </summary>
    public bool ControlEnabled
    {
        get => _controlEnabled;
        set => SetProperty(ref _controlEnabled, value);
    }
    #endregion

    /// <summary>
    /// The command to filter the table types
    /// </summary>
    public ICommand FilterCommand => new RelayCommand(FilterList);

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

        await ShowProgressAsync("Loading", "Please wait while loading the replication information...");

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
            await CloseProgressAsync();
        }
    }

    /// <summary>
    /// Filters the result
    /// </summary>
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

        await ShowProgressAsync("Loading", "Please wait while loading the columns...");

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
            await CloseProgressAsync();
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