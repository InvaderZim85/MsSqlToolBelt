﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.TableType;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.TableTypesControl"/>
/// </summary>
internal partial class TableTypesControlViewModel : ViewModelBase
{
    /// <summary>
    /// The instance of the table type manager
    /// </summary>
    private TableTypeManager? _manager;

    /// <summary>
    /// Contains the value which indicates if the data already loaded
    /// </summary>
    private bool _dataLoaded;

    /// <summary>
    /// The action to open the selected entry in the search
    /// </summary>
    private Action<string>? _openInSearch;

    #region View Properties

    /// <summary>
    /// The list with the table types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TableTypeEntry> _tableTypes = [];

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
            SetProperty(ref _selectedTableType, value);

            if (_manager == null)
                return;
            
            _manager.SelectedTableType = value;

            if (value is { Columns.Count: > 0 })
                SetColumns();
            else
                EnrichData();
        }
    }

    /// <summary>
    /// The list with the columns
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColumnEntry> _columns = [];

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
    private string _headerList = "Table types";

    /// <summary>
    /// The column header
    /// </summary>
    [ObservableProperty]
    private string _headerColumns = "Columns";

    #endregion

    /// <summary>
    /// The command to reload the table types
    /// </summary>
    public ICommand ReloadCommand => new RelayCommand(() =>
    {
        _dataLoaded = false;
        LoadData();
    });

    /// <summary>
    /// Sets the connection
    /// </summary>
    /// <param name="dataSource">The data source</param>
    /// <param name="database">The database</param>
    public void SetConnection(string dataSource, string database)
    {
        // Clear the current result
        TableTypes = [];
        Columns = [];

        // Reset the manager
        _manager?.Dispose();
        _manager = new TableTypeManager(dataSource, database);

        // Reset the data loaded value
        _dataLoaded = false;
    }

    /// <summary>
    /// Closes the current connection
    /// </summary>
    public void CloseConnection()
    {
        _manager?.Dispose();
    }

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="openInSearch">The action to open the selected entry in the search</param>
    public void InitViewModel(Action<string> openInSearch)
    {
        _openInSearch = openInSearch;
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    /// <param name="showProgress"><see langword="true"/> to show the progress, <see langword="false"/> to hide the progress information (optional)</param>
    public async void LoadData(bool showProgress = true)
    {
        if (_dataLoaded || _manager == null)
            return;

        ProgressDialogController? controller = null;
        if (showProgress)
            controller = await ShowProgressAsync("Loading", "Please wait while loading the table types...");

        try
        {
            await _manager.LoadTableTypesAsync();

            _dataLoaded = true;

            FilterList();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            if (controller != null)
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
            ? _manager.TableTypes
            : _manager.TableTypes.Where(w => w.Name.ContainsIgnoreCase(Filter)).ToList();

        TableTypes = result.ToObservableCollection();
        HeaderList = TableTypes.Count switch
        {
            0 => "No table types available",
            1 => "1 table type",
            _ => $"{TableTypes.Count} table types"
        };
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
            await ShowErrorAsync(ex, ErrorMessageType.Load);
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

        Columns = _manager.SelectedTableType.Columns.ToObservableCollection();

        HeaderColumns = Columns.Count > 1 ? $"{Columns.Count} columns" : "1 column";
    }

    /// <summary>
    /// Opens the selected table in the search
    /// </summary>
    [RelayCommand]
    private void OpenInSearch()
    {
        if (SelectedTableType == null)
            return;

        _openInSearch?.Invoke(SelectedTableType.Name);
    }
}