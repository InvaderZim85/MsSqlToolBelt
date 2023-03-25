using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.View.Windows;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
    private ObservableCollection<SearchResult> _searchResults = new();

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
            if (!SetProperty(ref _selectedResult, value) || _manager == null) 
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
    /// The list with the result types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _objectTypes = new();

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

    #endregion

    #region Table grid

    /// <summary>
    /// The list with the table columns
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColumnEntry> _columns = new();
    #endregion

    #region Job grid

    /// <summary>
    /// The list with the job steps
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<JobStepEntry> _jobSteps = new();

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
            if (SetProperty(ref _selectedJobStep, value))
                _setCmdText?.Invoke(value?.Command ?? "");
        }
    }

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
    /// The value which indicates if the sql editor should be shown
    /// </summary>
    [ObservableProperty]
    private bool _showSql = true;

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
    /// The command to copy the sql text
    /// </summary>
    public ICommand CopySqlCommand => new RelayCommand(() => CopyToClipboard(_sqlText));
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
        _resettingConnection = true;

        // Clear the old search result
        ResetSearch(true);

        // Dispose the old instance
        _manager?.Dispose();

        // Create a new instance
        _manager = new SearchManager(dataSource, database);

        _resettingConnection = false;
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        _manager?.Dispose();
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

        // Only continue when a complete reset is wanted
        if (!completeReset)
            return;

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

            // Execute the search
            await _manager.SearchAsync(SearchString, _settingsManager?.FilterList ?? new List<FilterEntry>());
            ObjectTypes = _manager.ResultTypes.ToObservableCollection();
            SelectedObjectType = ObjectTypes.FirstOrDefault() ?? "All";

            // Note: 
            // Actually, at this point the result was set with the help of the "FilterResult" method.
            // But this does not have to be done, because the method is called automatically when
            // the "SelectedObjectType" is changed, which happens in the line before.
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
    /// Enhances the selected search result
    /// </summary>
    private async void EnhanceData()
    {
        if (_manager?.SelectedResult == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the entry data...");

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
            await controller.CloseAsync();
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
    /// Shows the window with the table indexes
    /// </summary>
    [RelayCommand]
    private void ShowTableIndices()
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
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ShowHistoryAsync()
    {
        _searchHistoryManager ??= new SearchHistoryManager();
        var window = new SearchHistoryWindow(_searchHistoryManager) {Owner = Application.Current.MainWindow};

        if (window.ShowDialog() == false || string.IsNullOrEmpty(window.SelectedEntry))
            return;

        SearchString = window.SelectedEntry;

        await ExecuteSearchAsync();
    }
}