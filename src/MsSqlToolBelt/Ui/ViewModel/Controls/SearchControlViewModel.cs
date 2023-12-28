using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.DataObjects.TableType;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.View.Windows;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.SettingsControl"/>
/// </summary>
internal partial class SearchControlViewModel : ViewModelBase, IConnection
{
    /// <summary>
    /// The instance for the search
    /// </summary>
    private SearchManager? _manager;

    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private TableManager? _tableManager;

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
    /// The action to set the table definition
    /// </summary>
    private Action<string>? _setTableDefinitionText;

    /// <summary>
    /// The action to set the CMD text
    /// </summary>
    private Action<string>? _setCmdText;

    /// <summary>
    /// Contains the sql text
    /// </summary>
    private string _sqlText = string.Empty;

    /// <summary>
    /// Contains the table definition text
    /// </summary>
    private string _tableDefinitionText = string.Empty;

    /// <summary>
    /// The name / path of the MS SQL database
    /// </summary>
    private string _dataSource = string.Empty;

    /// <summary>
    /// The name of the database
    /// </summary>
    private string _database = string.Empty;

    /// <summary>
    /// The value which indicates if the connection is setting / resetting
    /// </summary>
    private bool _resettingConnection;

    #region View properties

    #region Search
    /// <summary>
    /// The search string
    /// </summary>
    [ObservableProperty]
    private string _searchString = string.Empty;

    /// <summary>
    /// The search result
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SearchResult> _searchResults = [];

    /// <summary>
    /// Gets or sets the selected result
    /// </summary>
    [ObservableProperty]
    private SearchResult? _selectedResult;

    /// <summary>
    /// The list with the result types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _objectTypes = [];

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
            if (SetProperty(ref _selectedObjectType, value) && !_resettingConnection)
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
            if (SetProperty(ref _addWildcardAutomatically, value))
                SaveWildcardValue();
        }
    }

    /// <summary>
    /// The result header
    /// </summary>
    [ObservableProperty]
    private string _headerResult = "Result";

    /// <summary>
    /// Gets or sets the value which indicates if the query window button is enabled
    /// </summary>
    [ObservableProperty]
    private bool _buttonQueryWindowEnabled;

    /// <summary>
    /// The value which indicates if the msdb violation message should be shown or not
    /// </summary>
    [ObservableProperty]
    private bool _hideMsdbViolationMessage;

    #endregion

    #region Table grid

    /// <summary>
    /// The list with the table columns
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColumnEntry> _columns = [];

    /// <summary>
    /// Gets or sets the tab index of the table tab control
    /// </summary>
    [ObservableProperty]
    private int _tableTabIndex;
    #endregion

    #region Job grid

    /// <summary>
    /// The list with the job steps
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<JobStepEntry> _jobSteps = [];

    /// <summary>
    /// Gets or sets the selected job step
    /// </summary>
    [ObservableProperty]
    private JobStepEntry? _selectedJobStep = new();

    /// <summary>
    /// Gets or sets the job filter
    /// </summary>
    [ObservableProperty]
    private string _jobFilter = string.Empty;

    #endregion

    #region Buttons

    /// <summary>
    /// The value which indicates if the show indices button is enabled
    /// </summary>
    [ObservableProperty]
    private bool _buttonShowIndexEnabled;
    #endregion

    #endregion

    #region View Properties - Bottom view

    /// <summary>
    /// Gets or sets the value which indicates if the main "border" / panel should be shown
    /// </summary>
    [ObservableProperty]
    private bool _showMain = true;

    /// <summary>
    /// The value which indicates if the sql editor should be shown
    /// </summary>
    [ObservableProperty]
    private bool _showSql;

    /// <summary>
    /// The value which indicates if the table grid should be shown
    /// </summary>
    [ObservableProperty]
    private bool _showTableGrid;

    /// <summary>
    /// The value which indicates if the job grid should be shown
    /// </summary>
    [ObservableProperty]
    private bool _showJobGrid;

    #endregion

    #region Change methods

    /// <summary>
    /// Occurs when the user selects another entry
    /// </summary>
    /// <param name="value">The selected value</param>
    partial void OnSelectedResultChanged(SearchResult? value)
    {
        if (_manager == null)
            return;

        _manager.SelectedResult = value;

        ButtonQueryWindowEnabled = false;

        if (value == null)
            return;

        switch (value.EntryType)
        {
            case EntryType.Table:
            case EntryType.TableType:
                SetVisibility(SearchViewType.Table);
                EnhanceData(false);
                break;
            case EntryType.Job:
                SetVisibility(SearchViewType.Job);
                EnhanceData(false);
                break;
            case EntryType.Object:
                SetVisibility(SearchViewType.Sql); // Default
                _sqlText = value.BoundItem is ObjectEntry entry ? entry.Definition : string.Empty;
                _setSqlText?.Invoke(_sqlText);
                break;
            default:
                SetVisibility(SearchViewType.None);
                break;
        }
    }

    /// <summary>
    /// Occurs when the user selects another tab (table tab control)
    /// </summary>
    /// <param name="value">The selected index</param>
    partial void OnTableTabIndexChanged(int value)
    {
        // Index:
        // - 0: Column preview
        // - 1: Definition preview
        if (value == 0)
            return;

        LoadTableDefinition();
    }

    /// <summary>
    /// Occurs when the user selects another job step
    /// </summary>
    /// <param name="value">The job step</param>
    partial void OnSelectedJobStepChanged(JobStepEntry? value)
    {
        _setCmdText?.Invoke(value?.Command ?? string.Empty);
    }

    /// <summary>
    /// Occurs when the user changes the job filter
    /// </summary>
    /// <param name="value">The new value</param>
    partial void OnJobFilterChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            FilterJob();
    }

    #endregion

    #region Commands
    /// <summary>
    /// Copies the sql text
    /// </summary>
    [RelayCommand]
    private void CopySql()
    {
        CopyToClipboard(_sqlText);
    }

    /// <summary>
    /// Copies the table definition to the clip board
    /// </summary>
    [RelayCommand]
    private void CopyTableDefinition()
    {
        CopyToClipboard(_tableDefinitionText);
    }

    /// <summary>
    /// Opens the table query window
    /// </summary>
    [RelayCommand]
    private void OpenQueryWindow()
    {
        if (SelectedResult?.BoundItem is not TableEntry table)
            return;

        var window = new TableQueryWindow(_dataSource, _database, table) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    /// <summary>
    /// Shows the search history
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private Task ShowHistoryAsync()
    {
        _searchHistoryManager ??= new SearchHistoryManager();
        var window = new SearchHistoryWindow(_searchHistoryManager) {Owner = Application.Current.MainWindow};

        if (window.ShowDialog() == false || string.IsNullOrEmpty(window.SelectedEntry))
            return Task.CompletedTask;

        SearchString = window.SelectedEntry;

        return ExecuteSearchAsync();
    }

    /// <summary>
    /// Shows the window with the table indexes
    /// </summary>
    [RelayCommand]
    private void ShowTableIndices()
    {
        if (_tableManager == null || SelectedResult is not {BoundItem: TableEntry table})
            return;

        var window = new TableIndexWindow(_tableManager, table)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    /// <summary>
    /// Copies / Exports the job information
    /// </summary>
    [RelayCommand]
    private void CopyExportJob()
    {
        if (SelectedJobStep == null)
            return;

        ExportObjectData(SelectedJobStep, $"{SelectedJobStep.Name}");
    }

    /// <summary>
    /// Copies / Exports the table information
    /// </summary>
    [RelayCommand]
    private void CopyExportTable()
    {
        if (SelectedResult == null)
            return;

        ExportListData(Columns, $"{SelectedResult.Name}");
    }

    /// <summary>
    /// Executes the search
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ExecuteSearchAsync()
    {
        if (_manager == null)
            return;

        ResetSearch();

        if (string.IsNullOrEmpty(SearchString))
            return;

        if (AddWildcardAutomatically && !SearchString.Contains('*') && !SearchString.Contains('%'))
            SearchString = $"*{SearchString}*";

        var controller = await ShowProgressAsync("Search", $"Please wait while searching for \"{SearchString}\"...");

        try
        {
            // Add the search entry to the history
            if (_searchHistoryManager != null)
                await SearchHistoryManager.AddSearchEntryAsync(SearchString);

            // Load the ignore list
            if (_settingsManager != null)
                await _settingsManager.LoadFilterAsync();

            // Add the event
            _manager.ProgressEvent += ProgressEvent;

            // Execute the search
            await _manager.SearchAsync(SearchString, _settingsManager?.FilterList ?? []);
            ObjectTypes = _manager.ResultTypes.ToObservableCollection();
            SelectedObjectType = ObjectTypes.FirstOrDefault() ?? "All";

            FilterResult();

            if (_manager.HasJobSearchError && !HideMsdbViolationMessage)
            {
                var result = await ShowMessageWithOptionAsync(MessageHelper.SearchMsdbAccessViolation);
                HideMsdbViolationMessage = result == MessageDialogResult.Negative;
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            // Remove the event
            _manager.ProgressEvent -= ProgressEvent;
            await controller.CloseAsync();
        }

        return;

        void ProgressEvent(object? sender, string message)
        {
            controller.SetMessage(message);
        }
    }

    /// <summary>
    /// Reloads the data of the selected entry
    /// </summary>
    [RelayCommand]
    private void ReloadData()
    {
        EnhanceData(true);
    }

    /// <summary>
    /// Filters the job list
    /// </summary>
    [RelayCommand]
    private void FilterJob()
    {
        FilterJobSteps();
    }

    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="settingsManager">The instance of the settings manager</param>
    /// <param name="setSqlText">The action to set the text of the sql control</param>
    /// <param name="setCmdText">The action to set the text of the cmd control</param>
    /// <param name="setTableDefinition">The action to set the text of the table definition control</param>
    public async void InitViewModel(SettingsManager settingsManager, Action<string> setSqlText, Action<string> setCmdText, Action<string> setTableDefinition)
    {
        _settingsManager = settingsManager;
        _setSqlText = setSqlText;
        _setCmdText = setCmdText;
        _setTableDefinitionText = setTableDefinition;

        _searchHistoryManager = new SearchHistoryManager();

        try
        {
            AddWildcardAutomatically = await SettingsManager.LoadSettingsValueAsync(SettingsKey.AutoWildcard, DefaultEntries.AutoWildcard);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Can't load settings.");
        }
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        _dataSource = dataSource;
        _database = database;

        _resettingConnection = true;

        // Clear the old search result
        ResetSearch(true);

        // Dispose the old instance
        _manager?.Dispose();

        // Create a new instance
        _tableManager = new TableManager(dataSource, database);
        _manager = new SearchManager(_settingsManager!, _tableManager!, dataSource, database);

        _resettingConnection = false;
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        _manager?.Dispose();
    }

    /// <inheritdoc />
    public void LoadData(bool showProgress)
    {
        // Ignore
    }

    /// <summary>
    /// Resets the last search result
    /// </summary>
    /// <param name="completeReset"><see langword="true"/> to perform a complete reset, otherwise <see langword="false"/></param>
    private void ResetSearch(bool completeReset = false)
    {
        // Remove the search result
        SearchResults.Clear();
        SelectedResult = null;

        // Remove the texts
        _setCmdText?.Invoke(string.Empty);
        _setSqlText?.Invoke(string.Empty);
        _setTableDefinitionText?.Invoke(string.Empty);

        // Only continue when a complete reset is wanted
        if (!completeReset)
            return;

        SearchString = string.Empty;

        // Remove the columns
        Columns.Clear();

        // Reset the header
        HeaderResult = "Result";

        // Reset the buttons
        ButtonShowIndexEnabled = false;

        // Reset the job steps
        JobSteps.Clear();
        SelectedJobStep = null;

        // Reset the object types
        ObjectTypes.Clear();
        SelectedObjectType = string.Empty;

        SetVisibility(SearchViewType.None);
    }

    /// <summary>
    /// Filters and shows the result
    /// </summary>
    private void FilterResult()
    {
        ResetSearch();

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
    /// Filters the job steps
    /// </summary>
    private void FilterJobSteps()
    {
        if (_manager?.SelectedResult?.BoundItem is not JobEntry job)
        {
            JobSteps = [];
            return;
        }

        JobSteps = (string.IsNullOrWhiteSpace(JobFilter)
            ? job.JobSteps
            : job.JobSteps.Where(w => w.Name.ContainsIgnoreCase(JobFilter) ||
                                      w.Command.ContainsIgnoreCase(JobFilter) ||
                                      w.FailAction.ContainsIgnoreCase(JobFilter) ||
                                      w.SuccessAction.ContainsIgnoreCase(JobFilter))).ToObservableCollection();
    }

    /// <summary>
    /// Sets the visibility of the specified type
    /// </summary>
    /// <param name="type">The type</param>
    private void SetVisibility(SearchViewType type)
    {
        // Grids
        ShowMain = false;
        ShowJobGrid = false;
        ShowSql = false;
        ShowTableGrid = false;

        // Set the selected index
        TableTabIndex = 0; // Back to the preview
        
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
            case SearchViewType.None:
            default:
                ShowMain = true;
                break;
        }
    }

    /// <summary>
    /// Enhances the selected search result
    /// </summary>
    private async void EnhanceData(bool forceReload)
    {
        if (_manager?.SelectedResult == null)
            return;

        if (_manager.EntryHasData() && !forceReload)
        {
            ShowData();
            return;
        }

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the entry data...");

        try
        {
            await _manager.EnrichDataAsync();
            
            ShowData();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            await controller.CloseAsync();
        }

        return;

        void ShowData()
        {
            ButtonShowIndexEnabled = false;

            // Reset the job / table list
            Columns = [];
            JobSteps = [];

            switch (_manager.SelectedResult.BoundItem)
            {
                case TableEntry table:
                    Columns = table.Columns.ToObservableCollection();
                    ButtonShowIndexEnabled = table.Indexes.Count > 0;
                    ButtonQueryWindowEnabled = true;
                    _tableDefinitionText = table.Definition;
                    _setTableDefinitionText?.Invoke(_tableDefinitionText);
                    break;
                case TableTypeEntry tableType:
                    Columns = tableType.Columns.ToObservableCollection();
                    ButtonShowIndexEnabled = false;
                    ButtonQueryWindowEnabled = false;
                    _tableDefinitionText = tableType.Definition;
                    _setTableDefinitionText?.Invoke(_tableDefinitionText);
                    break;
                case JobEntry:
                    FilterJobSteps();
                    break;
            }
        }
    }

    /// <summary>
    /// Loads the definition of the table / table type
    /// </summary>
    private async void LoadTableDefinition()
    {
        if (_manager == null)
            return;

        var definition = _manager.GetTableDefinition();

        if (!string.IsNullOrWhiteSpace(definition))
        {
            SetDefinitionText(definition);
            return;
        }

        var controller = await ShowProgressAsync("Please wait",
            "Please wait while loading the definition. This may take a while...");

        try
        {
            await _manager.LoadTableDefinitionAsync();

            // Show the definition
            definition = SelectedResult switch
            {
                { BoundItem: TableEntry table } => table.Definition,
                { BoundItem: TableTypeEntry tableType } => tableType.Definition,
                _ => string.Empty
            };

            SetDefinitionText(definition);

        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            await controller.CloseAsync();
        }

        return;

        void SetDefinitionText(string text)
        {
            _tableDefinitionText = text;
            _setTableDefinitionText?.Invoke(_tableDefinitionText);
        }
    }

    /// <summary>
    /// Saves the wildcard value
    /// </summary>
    private async void SaveWildcardValue()
    {
        try
        {
            await SettingsManager.SaveSettingsValueAsync(SettingsKey.AutoWildcard, AddWildcardAutomatically);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Can't save settings entry.");
        }
    }
}