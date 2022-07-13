using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.TableType;
using MsSqlToolBelt.Ui.View.Common;
using ZimLabs.CoreLib;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.TableTypesControl"/>
/// </summary>
internal class TableTypesControlViewModel : ViewModelBase, IConnection
{
    /// <summary>
    /// The instance of the table type manager
    /// </summary>
    private TableTypeManager? _manager;

    /// <summary>
    /// Contains the value which indicates if the data already loaded
    /// </summary>
    private bool _dataLoaded;

    #region View Properties

    /// <summary>
    /// Backing field for <see cref="TableTypes"/>
    /// </summary>
    private ObservableCollection<TableTypeEntry> _tableTypes = new();

    /// <summary>
    /// Gets or sets the list with the table types
    /// </summary>
    public ObservableCollection<TableTypeEntry> TableTypes
    {
        get => _tableTypes;
        private set => SetField(ref _tableTypes, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedTableType"/>
    /// </summary>
    private TableTypeEntry? _selectedTableType;

    /// <summary>
    /// Gets or sets the selected table type
    /// </summary>
    public TableTypeEntry? SelectedTableType
    {
        get => _selectedTableType;
        set
        {
            SetField(ref _selectedTableType, value);

            if (_manager == null)
                return;
            
            _manager.SelectedTableType = value;

            if (value != null && value.Columns.Any())
                SetColumns();
            else
                EnrichData();
        }
    }

    /// <summary>
    /// Backing field for <see cref="Columns"/>
    /// </summary>
    private ObservableCollection<ColumnEntry> _columns = new();

    /// <summary>
    /// Gets or sets the list with the columns
    /// </summary>
    public ObservableCollection<ColumnEntry> Columns
    {
        get => _columns;
        private set => SetField(ref _columns, value);
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
        set => SetField(ref _filter, value);
    }

    /// <summary>
    /// Backing field for <see cref="HeaderList"/>
    /// </summary>
    private string _headerList = "Table types";

    /// <summary>
    /// Gets or sets the list header
    /// </summary>
    public string HeaderList
    {
        get => _headerList;
        set => SetField(ref _headerList, value);
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
        set => SetField(ref _headerColumns, value);
    }

    #endregion

    /// <summary>
    /// The command to filter the table types
    /// </summary>
    public ICommand FilterCommand => new DelegateCommand(FilterResult);

    /// <summary>
    /// The command to reload the table types
    /// </summary>
    public ICommand ReloadCommand => new DelegateCommand(() =>
    {
        _dataLoaded = false;
        LoadData();
    });

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        // Clear the current result
        TableTypes = new ObservableCollection<TableTypeEntry>();
        Columns = new ObservableCollection<ColumnEntry>();

        _manager?.Dispose();
        _dataLoaded = false;

        _manager = new TableTypeManager(dataSource, database);
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    public async void LoadData()
    {
        if (_dataLoaded || _manager == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the table types...");

        try
        {
            await _manager.LoadTableTypesAsync();

            _dataLoaded = true;

            FilterResult();
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
    private void FilterResult()
    {
        if (_manager == null)
            return;

        var result = string.IsNullOrEmpty(Filter)
            ? _manager.TableTypes
            : _manager.TableTypes.Where(w => w.Name.ContainsIgnoreCase(Filter)).ToList();

        TableTypes = new ObservableCollection<TableTypeEntry>(result);
        HeaderList = TableTypes.Count > 1 ? $"{TableTypes.Count} table types" : "1 table type";
    }

    /// <summary>
    /// Enriches the selected table type
    /// </summary>
    private async void EnrichData()
    {
        if (_manager?.SelectedTableType == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the columns...");

        try
        {
            await _manager.EnrichTableTypeAsync();

            // Set the columns
            SetColumns();
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
        if (_manager?.SelectedTableType == null)
            return;

        Columns = new ObservableCollection<ColumnEntry>(_manager.SelectedTableType.Columns);

        HeaderColumns = Columns.Count > 1 ? $"{Columns.Count} columns" : "1 column";
    }
}