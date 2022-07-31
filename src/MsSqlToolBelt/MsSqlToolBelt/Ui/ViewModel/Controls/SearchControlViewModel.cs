using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.View.Windows;
using Serilog;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.SettingsControl"/>
/// </summary>
internal class SearchControlViewModel : ViewModelBase, IConnection
{
    /// <summary>
    /// The instance for the search
    /// </summary>
    private SearchManager? _manager;

    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private SettingsManager? _settingsManager;

    /// <summary>
    /// The instance for the interaction with the search history
    /// </summary>
    private SearchHistoryManager? _searchHistoryManager;

    /// <summary>
    /// The action to set the SQL text
    /// </summary>
    private Action<string>? _setSqlText;

    /// <summary>
    /// The action to set the CMD text
    /// </summary>
    private Action<string>? _setCmdText;

    /// <summary>
    /// Contains the sql text
    /// </summary>
    private string _sqlText = string.Empty;

    #region View properties

    #region Search
    /// <summary>
    /// Backing field for <see cref="SearchString"/>
    /// </summary>
    private string _searchString = string.Empty;

    /// <summary>
    /// Gets or sets the search string
    /// </summary>
    public string SearchString
    {
        get => _searchString;
        set => SetField(ref _searchString, value);
    }

    /// <summary>
    /// Backing field for <see cref="SearchResults"/>
    /// </summary>
    private ObservableCollection<SearchResult> _searchResults = new();

    /// <summary>
    /// Gets or sets the search result
    /// </summary>
    public ObservableCollection<SearchResult> SearchResults
    {
        get => _searchResults;
        private set => SetField(ref _searchResults, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedResult"/>
    /// </summary>
    private SearchResult? _selectedResult;

    /// <summary>
    /// Gets or sets the selected result
    /// </summary>
    public SearchResult? SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (!SetField(ref _selectedResult, value) || _manager == null) 
                return;

            _manager.SelectedResult = value;

            if (value == null)
                return;

            switch (value.Type)
            {
                case "Table":
                    SetVisibility(SearchViewType.Table);
                    EnhanceData();
                    break;
                case "Job":
                    SetVisibility(SearchViewType.Job);
                    EnhanceData();
                    break;
                default:
                    SetVisibility(SearchViewType.Sql);
                    _sqlText = value.BoundItem is ObjectEntry entry ? entry.Definition : string.Empty;
                    _setSqlText?.Invoke(_sqlText);
                    break;
            }
        }
    }

    /// <summary>
    /// Backing field for <see cref="ObjectTypes"/>
    /// </summary>
    private ObservableCollection<string> _objectTypes = new();

    /// <summary>
    /// Gets or sets the list with the result types
    /// </summary>
    public ObservableCollection<string> ObjectTypes
    {
        get => _objectTypes;
        private set => SetField(ref _objectTypes, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedObjectType"/>
    /// </summary>
    private string _selectedObjectType = "All";

    /// <summary>
    /// Gets or sets the selected result type
    /// </summary>
    public string SelectedObjectType
    {
        get => _selectedObjectType;
        set
        {
            if (SetField(ref _selectedObjectType, value))
                FilterResult();
        }
    }

    /// <summary>
    /// Backing field for <see cref="AddWildcardAutomatically"/>
    /// </summary>
    private bool _addWildcardAutomatically = true;

    /// <summary>
    /// Gets or sets the value which indicates if a wildcard ("*") should be added to the search string automatically
    /// </summary>
    public bool AddWildcardAutomatically
    {
        get => _addWildcardAutomatically;
        set
        {
            if (SetField(ref _addWildcardAutomatically, value))
                SaveWildcardValue();
        }
    }

    /// <summary>
    /// Backing field for <see cref="HeaderResult"/>
    /// </summary>
    private string _headerResult = "Result";

    /// <summary>
    /// Gets or sets the header of the result
    /// </summary>
    public string HeaderResult
    {
        get => _headerResult;
        private set => SetField(ref _headerResult, value);
    }

    #endregion

    #region Table grid

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
        private set => SetField(ref _columns, value);
    }
    #endregion

    #region Job grid

    /// <summary>
    /// Backing field for <see cref="JobSteps"/>
    /// </summary>
    private ObservableCollection<JobStepEntry> _jobSteps = new();

    /// <summary>
    /// Gets or sets the list with the job steps
    /// </summary>
    public ObservableCollection<JobStepEntry> JobSteps
    {
        get => _jobSteps;
        private set => SetField(ref _jobSteps, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedJobStep"/>
    /// </summary>
    private JobStepEntry? _selectedJobStep = new();

    /// <summary>
    /// Gets or sets the selected job step
    /// </summary>
    public JobStepEntry? SelectedJobStep
    {
        get => _selectedJobStep;
        set
        {
            if (SetField(ref _selectedJobStep, value))
                _setCmdText?.Invoke(value?.Command ?? "");
        }
    }

    #endregion

    #region Buttons

    /// <summary>
    /// Backing field for <see cref="ButtonShowIndexEnabled"/>
    /// </summary>
    private bool _buttonShowIndexEnabled;

    /// <summary>
    /// Gets or sets the value which indicates if the show indexes button is enabled
    /// </summary>
    public bool ButtonShowIndexEnabled
    {
        get => _buttonShowIndexEnabled;
        set => SetField(ref _buttonShowIndexEnabled, value);
    }
    #endregion

    #endregion

    #region View Properties - Bottom view

    /// <summary>
    /// Backing field for <see cref="ShowSql"/>
    /// </summary>
    private bool _showSql = true;

    /// <summary>
    /// Gets or sets the value which indicates if the sql editor should be shown
    /// </summary>
    public bool ShowSql
    {
        get => _showSql;
        set => SetField(ref _showSql, value);
    }

    /// <summary>
    /// Backing field for <see cref="ShowTableGrid"/>
    /// </summary>
    private bool _showTableGrid;

    /// <summary>
    /// Gets or sets the value which indicates if the table grid should be shown
    /// </summary>
    public bool ShowTableGrid
    {
        get => _showTableGrid;
        set => SetField(ref _showTableGrid, value);
    }

    /// <summary>
    /// Backing field for <see cref="ShowJobGrid"/>
    /// </summary>
    private bool _showJobGrid;

    /// <summary>
    /// Gets or sets the value which indicates if the job grid should be shown
    /// </summary>
    public bool ShowJobGrid
    {
        get => _showJobGrid;
        set => SetField(ref _showJobGrid, value);
    }

    /// <summary>
    /// Sets the visibility of the specified type
    /// </summary>
    /// <param name="type">The type</param>
    private void SetVisibility(SearchViewType type)
    {
        // Grids
        ShowJobGrid = false;
        ShowSql = false;
        ShowTableGrid = false;
        
        // Buttons
        ButtonShowIndexEnabled = false;

        switch (type)
        {
            case SearchViewType.Job:
                ShowJobGrid = true;
                break;
            case SearchViewType.Sql:
                ShowSql = true;
                break;
            case SearchViewType.Table:
                ShowTableGrid = true;
                break;
        }
    }
    #endregion

    #region Commands
    /// <summary>
    /// The command to 
    /// </summary>
    public ICommand SearchCommand => new DelegateCommand(ExecuteSearch);

    /// <summary>
    /// The command to copy the sql text
    /// </summary>
    public ICommand CopySqlCommand => new DelegateCommand(() => CopyToClipboard(_sqlText));

    /// <summary>
    /// The command to copy / export the table data
    /// </summary>
    public ICommand CopyExportTableCommand => new DelegateCommand(CopyExportTable);

    /// <summary>
    /// The command to copy / export the job
    /// </summary>
    public ICommand CopyExportJobCommand => new DelegateCommand(CopyExportJob);

    /// <summary>
    /// The command to show the index window
    /// </summary>
    public ICommand ShowIndexesCommand => new DelegateCommand(ShowTableIndexes);

    /// <summary>
    /// The command to show the history
    /// </summary>
    public ICommand ShowHistoryCommand => new DelegateCommand(ShowHistory);
    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="settingsManager">The instance of the settings manager</param>
    /// <param name="setSqlText">The action to set the text of the sql control</param>
    /// <param name="setCmdText">The action to set the text of the cmd control</param>
    public async void InitViewModel(SettingsManager settingsManager, Action<string> setSqlText, Action<string> setCmdText)
    {
        _settingsManager = settingsManager;
        _setSqlText = setSqlText;
        _setCmdText = setCmdText;

        _searchHistoryManager = new SearchHistoryManager(settingsManager);

        try
        {
            AddWildcardAutomatically = await _settingsManager.LoadSettingsValueAsync(SettingsKey.AutoWildcard, true);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Can't load settings.");
        }
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        // Clear the old search result
        ClearResult();

        // Dispose the old instance
        _manager?.Dispose();

        // Create a new instance
        _manager = new SearchManager(dataSource, database);
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        _manager?.Dispose();
    }

    /// <summary>
    /// Clears the last search result
    /// </summary>
    private void ClearResult()
    {
        SearchResults.Clear();
        SelectedResult = null;
    }

    /// <summary>
    /// Executes the search
    /// </summary>
    private async void ExecuteSearch()
    {
        if (_manager == null)
            return;

        ClearResult();

        if (string.IsNullOrEmpty(SearchString))
            return;

        if (AddWildcardAutomatically && !SearchString.Contains('*') && !SearchString.Contains('%'))
            SearchString = $"*{SearchString}*";

        await ShowProgressAsync("Search", $"Please wait while searching for \"{SearchString}\"...");

        try
        {
            // Add the search entry to the history
            if (_searchHistoryManager != null)
                await _searchHistoryManager.AddSearchEntryAsync(SearchString);

            // Load the ignore list
            if (_settingsManager != null)
                await _settingsManager.LoadFilterAsync();

            // Execute the search
            await _manager.SearchAsync(SearchString, _settingsManager?.FilterList ?? new List<FilterEntry>());
            ObjectTypes = _manager.ResultTypes.ToObservableCollection();
            SelectedObjectType = ObjectTypes.FirstOrDefault() ?? "All";

            // Filter and show the result
            FilterResult();
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
    /// Filters and shows the result
    /// </summary>
    private void FilterResult()
    {
        ClearResult();

        var result = SelectedObjectType.Equals("All")
            ? _manager!.SearchResults
            : _manager!.SearchResults.Where(w => w.Type.Equals(SelectedObjectType)).ToList();

        SearchResults = result.OrderBy(o => o.Type).ThenBy(t => t.Name).ToObservableCollection();

        // Create the header result
        var info = result.GroupBy(g => g.Type).Select(s => new
        {
            Type = s.Key,
            Count = s.Count()
        }).Select(s => $"{s.Type}: {s.Count}");

        HeaderResult = $"Result - Total: {result.Count} // {string.Join(" // ", info)}";
    }

    /// <summary>
    /// Enhances the selected search result
    /// </summary>
    private async void EnhanceData()
    {
        if (_manager?.SelectedResult == null)
            return;

        await ShowProgressAsync("Loading", "Please wait while loading the entry data...");

        try
        {
            await _manager.EnrichDataAsync();
            ButtonShowIndexEnabled = false;

            // Reset the job / table list
            Columns = new ObservableCollection<ColumnEntry>();
            JobSteps = new ObservableCollection<JobStepEntry>();

            switch (_manager.SelectedResult.BoundItem)
            {
                case TableEntry table:
                    Columns = table.Columns.ToObservableCollection();
                    ButtonShowIndexEnabled = table.Indexes.Any();
                    break;
                case JobEntry job:
                    JobSteps = job.JobSteps.ToObservableCollection();
                    break;
            }
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
    /// Saves the wildcard value
    /// </summary>
    private async void SaveWildcardValue()
    {
        if (_settingsManager == null)
            return;

        try
        {
            await _settingsManager.SaveSettingsValueAsync(SettingsKey.AutoWildcard, AddWildcardAutomatically);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Can't save settings entry.");
        }
    }

    /// <summary>
    /// Copies / Exports the table information
    /// </summary>
    private void CopyExportTable()
    {
        if (SelectedResult == null)
            return;

        ExportListData(Columns, $"{SelectedResult.Name}");
    }

    /// <summary>
    /// Copies / Exports the job information
    /// </summary>
    private void CopyExportJob()
    {
        if (SelectedJobStep == null)
            return;

        ExportObjectData(SelectedJobStep, $"{SelectedJobStep.Name}");
    }

    /// <summary>
    /// Shows the window with the table indexes
    /// </summary>
    private void ShowTableIndexes()
    {
        if (SelectedResult is not {BoundItem: TableEntry table})
            return;

        var window = new TableIndexWindow(table)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    /// <summary>
    /// Shows the search history
    /// </summary>
    private void ShowHistory()
    {
        _settingsManager ??= new SettingsManager();
        _searchHistoryManager ??= new SearchHistoryManager(_settingsManager);
        var window = new SearchHistoryWindow(_searchHistoryManager) {Owner = Application.Current.MainWindow};

        if (window.ShowDialog() == false || string.IsNullOrEmpty(window.SelectedEntry))
            return;

        SearchString = window.SelectedEntry;

        ExecuteSearch();
    }
}