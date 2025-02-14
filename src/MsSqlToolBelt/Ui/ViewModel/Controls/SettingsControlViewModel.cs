using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.Ui.View.Windows;
using System.Collections.ObjectModel;
using System.Configuration;
using Microsoft.VisualStudio.Threading;
using ZimLabs.CoreLib;
using DataRetentionPeriodUnit = Microsoft.SqlServer.Management.Smo.DataRetentionPeriodUnit;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.SettingsControl"/>
/// </summary>
internal partial class SettingsControlViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private SettingsManager? _manager;

    #region View properties

    #region Color scheme
    /// <summary>
    /// The list with the color themes
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColorEntry> _colorThemeList = [];

    /// <summary>
    /// Backing field for <see cref="SelectedColorTheme"/>
    /// </summary>
    private ColorEntry? _selectedColorTheme;

    /// <summary>
    /// Gets or sets the selected color theme
    /// </summary>
    public ColorEntry? SelectedColorTheme
    {
        get => _selectedColorTheme;
        set
        {
            if (SetProperty(ref _selectedColorTheme, value) && value != null)
                Helper.SetColorTheme(value.Name);
        }
    }

    /// <summary>
    /// Gets or sets the value which indicates whether the search tab should be shown
    /// </summary>
    [ObservableProperty]
    private bool _tabSearch;

    /// <summary>
    /// Gets or sets the value which indicates whether the table type tab should be shown
    /// </summary>
    [ObservableProperty]
    private bool _tabTableTypes;

    /// <summary>
    /// Gets or sets the value which indicates whether the class generator tab should be shown
    /// </summary>
    [ObservableProperty]
    private bool _tabClassGen;

    /// <summary>
    /// Gets or sets the value which indicates whether the definition export tab should be shown
    /// </summary>
    [ObservableProperty]
    private bool _tabDefinitionExport;

    /// <summary>
    /// Gets or sets the value which indicates whether the replication info tab should be shown
    /// </summary>
    [ObservableProperty]
    private bool _tabReplicationInfo;
    #endregion

    #region Server
    /// <summary>
    /// The list with the server
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ServerEntry> _serverList = [];

    /// <summary>
    /// Backing field for <see cref="SelectedServer"/>
    /// </summary>
    private ServerEntry? _selectedServer;

    /// <summary>
    /// Gets or sets the selected server
    /// </summary>
    public ServerEntry? SelectedServer
    {
        get => _selectedServer;
        set
        {
            SetProperty(ref _selectedServer, value);
            ButtonMoveDownEnabled = false;
            ButtonMoveUpEnabled = false;

            if (value == null)
                return;

            SetMovementButtons(value);
        }
    }

    /// <summary>
    /// The value which indicates if the move up button is enabled
    /// </summary>
    [ObservableProperty]
    private bool _buttonMoveUpEnabled;

    /// <summary>
    /// The value which indicates if the move down button is enabled
    /// </summary>
    [ObservableProperty]
    private bool _buttonMoveDownEnabled;

    /// <summary>
    /// The value which indicates if the values should be overwritten during the import
    /// </summary>
    [ObservableProperty]
    private bool _importOverride;

    #endregion

    #region Filter
    /// <summary>
    /// The list with the filters
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<FilterEntry> _filterList = [];

    /// <summary>
    /// The selected filter
    /// </summary>
    [ObservableProperty]
    private FilterEntry? _selectedFilter;

    /// <summary>
    /// The list with the filter types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IdTextEntry> _filterTypeList = [];

    /// <summary>
    /// The selected filter type
    /// </summary>
    [ObservableProperty]
    private IdTextEntry? _selectedFilterType;

    /// <summary>
    /// The filter value
    /// </summary>
    [ObservableProperty]
    private string _filterValue = string.Empty;
    #endregion

    #region Various

    /// <summary>
    /// The list with the export types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IdTextEntry> _exportTypes = [];

    /// <summary>
    /// The selected export type
    /// </summary>
    [ObservableProperty]
    private IdTextEntry? _selectedExportType;

    /// <summary>
    /// The search history count
    /// </summary>
    [ObservableProperty]
    private int _searchHistoryCount = 50;

    /// <summary>
    /// Gets or sets the value which specifies whether only the name should be copied if only one row is selected in the grid.
    /// </summary>
    [ObservableProperty]
    private bool _copyGridSingleLineOnlyValue;
    #endregion

    #region ClassGen options

    /// <summary>
    /// Gets or sets the value which indicates if the class generator options should be saved
    /// </summary>
    [ObservableProperty]
    private bool _saveClassGenOptions;

    /// <summary>
    /// Gets or sets the list of available class modifiers
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _classModifiers = [];

    /// <summary>
    /// Gets or sets the default class modifier
    /// </summary>
    [ObservableProperty]
    private string _defaultClassModifier = ClassGenManager.ModifierFallback;

    /// <summary>
    /// Gets or sets the value which indicates if the SQL Query should be hidden
    /// </summary>
    [ObservableProperty]
    private bool _hideSqlQuery;
    #endregion

    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="settingsManager">The instance for the interaction with the settings</param>
    public void InitViewModel(SettingsManager settingsManager)
    {
        _manager = settingsManager;

        LoadSettings();
    }

    /// <summary>
    /// Loads the settings
    /// </summary>
    private async void LoadSettings()
    {
        try
        {
            // Load the settings
            var values = new List<SettingsValue>
            {
                new(SettingsKey.ColorScheme),
                new(SettingsKey.CopyToClipboardFormat),
                new(SettingsKey.SearchHistoryEntryCount),
                new(SettingsKey.CopyGridSingleLineOnlyValue),
                new(SettingsKey.SaveClassGenOptions),
                new(SettingsKey.ClassGenDefaultModifier),
                new(SettingsKey.ClassGenHideSqlQuery)
            };

            await SettingsManager.LoadSettingsValuesAsync(values);

            // Init the filter types
            FilterTypeList = Helper.CreateFilterTypeList().ToObservableCollection();

            // Preselect a theme
            var themeName = values.GetSettingsValue(SettingsKey.ColorScheme, DefaultEntries.ColorScheme);

            // Load the colors
            AddColors(themeName);

            // Load the server
            await LoadServerAsync();

            // Load the filters
            await LoadFilterAsync();

            // Set the various data
            var exportList = Helper.CreateExportTypeList(ExportDataType.List);
            ExportTypes = new ObservableCollection<IdTextEntry>(exportList);
            var exportType =
                values.GetSettingsValue(SettingsKey.CopyToClipboardFormat, DefaultEntries.CopyToClipboardFormat);
            SelectedExportType = exportList.FirstOrDefault(f => f.Id == exportType);

            SearchHistoryCount =
                values.GetSettingsValue(SettingsKey.SearchHistoryEntryCount, DefaultEntries.SearchHistoryCount);

            CopyGridSingleLineOnlyValue = values.GetSettingsValue(SettingsKey.CopyGridSingleLineOnlyValue, false);

            // Class gen options
            SaveClassGenOptions = values.GetSettingsValue(SettingsKey.SaveClassGenOptions, false);
            ClassModifiers = CommonValues.GetModifierList();
            DefaultClassModifier =
                values.GetSettingsValue(SettingsKey.ClassGenDefaultModifier, ClassGenManager.ModifierFallback);
            HideSqlQuery = values.GetSettingsValue(SettingsKey.ClassGenHideSqlQuery, false);

            var tabSettings = SettingsManager.LoadTabSettings();
            SetTabSettings(tabSettings);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
    }

    /// <summary>
    /// Sets the tab settings
    /// </summary>
    /// <param name="settings">The list with the settings</param>
    private void SetTabSettings(SortedList<TabControlEntry, bool> settings)
    {
        foreach (var entry in settings)
        {
            switch (entry.Key)
            {
                case TabControlEntry.Search:
                    TabSearch = entry.Value;
                    break;
                case TabControlEntry.TableTypes:
                    TabTableTypes = entry.Value;
                    break;
                case TabControlEntry.ClassGenerator:
                    TabClassGen = entry.Value;
                    break;
                case TabControlEntry.DefinitionExport:
                    TabDefinitionExport = entry.Value;
                    break;
                case TabControlEntry.ReplicationInfo:
                    TabReplicationInfo = entry.Value;
                    break;
            }
        }
    }

    /// <summary>
    /// Adds the colors
    /// </summary>
    /// <param name="preSelection">The name of the color which should be selected</param>
    private void AddColors(string preSelection)
    {
        var tmpList = new List<ColorEntry>();
        tmpList.AddRange(ThemeManager.Current.ColorSchemes.Select(s => new ColorEntry(s, false)));

        // Add the custom colors
        var customColors = Helper.LoadCustomColors().Select(s => s.Name).Select(s => new ColorEntry(s, true)).ToList();
        if (customColors.Count > 0)
        {
            tmpList.AddRange(customColors);
        }
        else
        {
            // No custom colors available, reset the pre selection to the default value
            preSelection = DefaultEntries.ColorScheme;
        }

        ColorThemeList = tmpList.ToObservableCollection();

        SelectedColorTheme = ColorThemeList.FirstOrDefault(f => f.Name.EqualsIgnoreCase(preSelection));
    }

    /// <summary>
    /// Adds the window to add a new color
    /// </summary>
    [RelayCommand]
    private void AddCustomColor()
    {
        var dialog = new CustomColorWindow
        {
            Owner = GetMainWindow()
        };

        if (dialog.ShowDialog() != true)
            return;
            
        AddColors(dialog.ColorName);
    }

    /// <summary>
    /// Deletes the selected custom color
    /// </summary>
    [RelayCommand]
    private async Task DeleteCustomColorAsync()
    {
        if (SelectedColorTheme == null)
            return;

        if (!SelectedColorTheme.CustomColor)
        {
            await ShowMessageAsync("Color", "You can't delete a default color. Please select a custom color.");
            return;
        }

        // Save the custom colors
        var result = Helper.RemoveCustomColor(SelectedColorTheme.Name);

        if (result)
        {
            AddColors(ColorThemeList.FirstOrDefault()?.Name ?? string.Empty);
        }
        else
        {
            await ShowMessageAsync("Color",
                $"An error has occurred while deleting the color '{SelectedColorTheme.Name}'");
        }
    }

    /// <summary>
    /// Saves the appearance settings
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private Task SaveAppearanceSettingsAsync()
    {
        return SaveSettingsAsync(SaveFuncAsync);

        async Task SaveFuncAsync()
        {
            await SaveThemeAsync();

            await SaveTabSettingsAsync();
        }
    }

    /// <summary>
    /// Saves the current theme
    /// </summary>
    /// <returns>The awaitable task</returns>
    private Task SaveThemeAsync()
    {
        return SelectedColorTheme == null
            ? Task.CompletedTask
            : SettingsManager.SaveSettingsValueAsync(SettingsKey.ColorScheme, SelectedColorTheme.Name);
    }

    /// <summary>
    /// Saves the tab settings
    /// </summary>
    /// <returns>The awaitable task</returns>
    private Task SaveTabSettingsAsync()
    {
        var settings = new SortedList<TabControlEntry, bool>
        {
            { TabControlEntry.Search, TabSearch },
            { TabControlEntry.TableTypes, TabTableTypes },
            { TabControlEntry.ClassGenerator, TabClassGen },
            { TabControlEntry.DefinitionExport, TabDefinitionExport },
            { TabControlEntry.ReplicationInfo, TabReplicationInfo }
        };

        return SettingsManager.SaveTabSettingsAsync(settings);
    }

    #region Server
    /// <summary>
    /// Loads and shows the server
    /// </summary>
    /// <param name="preSelection">The server which should be selected (optional)</param>
    /// <returns>The awaitable task</returns>
    private async Task LoadServerAsync(ServerEntry? preSelection = null)
    {
        await _manager!.LoadServerAsync(true);
        
        SetServerList(preSelection);
    }

    /// <summary>
    /// Sets the server list
    /// </summary>
    private void SetServerList(ServerEntry? preSelection = null)
    {
        ServerList = _manager!.ServerList.OrderBy(o => o.Order).ToObservableCollection();

        SelectedServer = preSelection ?? _manager.ServerList.FirstOrDefault(); // Select the first server
    }

    /// <summary>
    /// Adds a new server
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task AddServerAsync()
    {
        var dialog = new EditServerWindow(SettingsManager.ServerDatabaseUniqueAsync) { Owner = GetMainWindow() };
        if (dialog.ShowDialog() == false)
            return;


        var existingServer = ServerList.FirstOrDefault(f =>
            f.Name.Equals(dialog.SelectedServer.Name, StringComparison.OrdinalIgnoreCase) &&
            f.DefaultDatabase.Equals(dialog.SelectedServer.DefaultDatabase, StringComparison.OrdinalIgnoreCase));
        if (existingServer != null)
        {
            SelectedServer = existingServer;
            return;
        }

        // Add the new server
        try
        {
            await _manager!.AddServerAsync(dialog.SelectedServer);

            SetServerList(dialog.SelectedServer);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }

    /// <summary>
    /// Edits the selected server
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task EditServerAsync()
    {
        if (SelectedServer == null)
            return;

        var dialog = new EditServerWindow(SettingsManager.ServerDatabaseUniqueAsync)
        {
            SelectedServer = SelectedServer,
            Owner = GetMainWindow()
        };
        if (dialog.ShowDialog() == false)
            return;

        // Update the server
        try
        {
            await SettingsManager.UpdateServerAsync(SelectedServer);

            SetServerList(SelectedServer);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }

    /// <summary>
    /// Deletes the selected server
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task DeleteServerAsync()
    {
        if (SelectedServer == null)
            return;

        if (await ShowQuestionAsync("Delete server", $"Do you really want to delete the server '{SelectedServer.Name}'?", "Yes", "No") !=
            MessageDialogResult.Affirmative)
            return;

        try
        {
            await _manager!.DeleteServerAsync(SelectedServer);

            SelectedServer = null;

            SetServerList();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }

    /// <summary>
    /// Moves the selected server up or down
    /// </summary>
    /// <param name="direction">The desired direction</param>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task MoveServerAsync(MoveDirection direction)
    {
        if (SelectedServer == null)
            return;

        try
        {
            await SettingsManager.MoveServerOrderAsync(SelectedServer, direction == MoveDirection.Up);

            SetMovementButtons(SelectedServer);

            ServerList = ServerList.OrderBy(o => o.Order).ToObservableCollection();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }

    /// <summary>
    /// Sets the movement buttons
    /// </summary>
    /// <param name="server">The selected server</param>
    private void SetMovementButtons(ServerEntry server)
    {
        ButtonMoveDownEnabled = false;
        ButtonMoveUpEnabled = false;

        var maxOrder = ServerList.Max(m => m.Order);
        if (server.Order >= 1 && server.Order < maxOrder)
            ButtonMoveDownEnabled = true;

        if (server.Order > 1 && server.Order <= maxOrder)
            ButtonMoveUpEnabled = true;
    }
    #endregion

    #region Filter
    /// <summary>
    /// Loads the filters
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async Task LoadFilterAsync()
    {
        await _manager!.LoadFilterAsync();

        SetFilterList();
    }

    /// <summary>
    /// Sets the list of filters
    /// </summary>
    /// <param name="preSelection">The entry which should be selected</param>
    private void SetFilterList(FilterEntry? preSelection = null)
    {
        FilterList = _manager!.FilterList.ToObservableCollection();

        if (preSelection == null)
            return;

        SelectedFilter = preSelection;
    }

    /// <summary>
    /// Saves the current filter
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task AddFilterAsync()
    {
        if (SelectedFilterType == null || string.IsNullOrEmpty(FilterValue))
            return;

        var entry = new FilterEntry
        {
            FilterTypeId = SelectedFilterType.Id,
            Value = FilterValue
        };

        try
        {
            await _manager!.AddFilterAsync(entry);

            SetFilterList(entry);

            // Reset the input fields
            SelectedFilterType = null;
            FilterValue = string.Empty;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }

    /// <summary>
    /// Deletes the selected filter
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task DeleteFilterAsync()
    {
        if (SelectedFilter == null)
            return;

        var result = await ShowQuestionAsync("Delete", $"Do you really want to delete filter \"{SelectedFilter.GetInfo()}\"?", "Yes",
            "No");

        if (result != MessageDialogResult.Affirmative)
            return;

        try
        {
            await _manager!.DeleteFilterAsync(SelectedFilter);

            SetFilterList();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }
    #endregion

    #region Various

    /// <summary>
    /// Saves the various settings
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private Task SaveVariousSettingsAsync()
    {
        return SelectedExportType == null 
            ? Task.CompletedTask 
            : SaveSettingsAsync(SaveFuncAsync);

        async Task SaveFuncAsync()
        {
            var saveList = new SortedList<SettingsKey, object>
            {
                { SettingsKey.CopyToClipboardFormat, SelectedExportType.Id },
                { SettingsKey.SearchHistoryEntryCount, SearchHistoryCount },
                { SettingsKey.CopyGridSingleLineOnlyValue, CopyGridSingleLineOnlyValue }
            };

            await SettingsManager.SaveSettingsValuesAsync(saveList);
        }
    }

    /// <summary>
    /// Saves the class generator options
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private Task SaveClassGenOptionsAsync()
    {
        // Save the class gen options
        return SaveSettingsAsync(SaveFuncAsync);

        async Task SaveFuncAsync()
        {
            var saveList = new SortedList<SettingsKey, object>
            {
                { SettingsKey.ClassGenDefaultModifier, DefaultClassModifier },
                { SettingsKey.SaveClassGenOptions, SaveClassGenOptions },
                { SettingsKey.ClassGenHideSqlQuery, HideSqlQuery }
            };

            await SettingsManager.SaveSettingsValuesAsync(saveList);

            await Mediator.ExecuteFunctionAsync(MediatorKey.SetClassGenQueryVisibility);
        }
    }

    /// <summary>
    /// Exports the settings as JSON file
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ExportSettingsAsync()
    {
        if (_manager == null)
            return;

        var dialog = new CommonSaveFileDialog
        {
            Filters = {new CommonFileDialogFilter("JSON file", "*.json")},
            DefaultFileName = "MsSqlToolBelt_Settings",
            DefaultExtension = "json"
        };

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            return;

        var controller = await ShowProgressAsync("Export", "Please wait while exporting the settings...");

        try
        {
            await SettingsManager.ExportSettingsAsync(dialog.FileName);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Imports the selected settings (JSON file)
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ImportSettingsAsync()
    {
        if (_manager == null)
            return;

        var dialog = new CommonOpenFileDialog
        {
            Filters = {new CommonFileDialogFilter("JSON file", "*.json")},
            EnsureFileExists = true
        };

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            return;

        var controller = await ShowProgressAsync("Import", "Please wait while importing the settings...");
        try
        {
            await SettingsManager.ImportSettingsAsync(dialog.FileName, ImportOverride);

            // Reload the settings
            LoadSettings();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Import);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Executes the specified function to save the settings
    /// </summary>
    /// <param name="saveFunc">The function to save the settings</param>
    /// <returns>The awaitable task</returns>
    private async Task SaveSettingsAsync(Func<Task> saveFunc)
    {
        var controller = await ShowProgressAsync("Save", "Please wait while saving the settings...");

        try
        {
            await saveFunc();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    #endregion
}