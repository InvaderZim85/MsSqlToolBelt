using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Windows;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.SettingsControl"/>
/// </summary>
internal class SettingsControlViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private SettingsManager? _manager;

    #region View properties

    #region Color scheme
    /// <summary>
    /// Backing field for <see cref="ColorThemeList"/>
    /// </summary>
    private ObservableCollection<string> _colorThemeList = new();

    /// <summary>
    /// Gets or sets the list with the color themes
    /// </summary>
    public ObservableCollection<string> ColorThemeList
    {
        get => _colorThemeList;
        private set => SetProperty(ref _colorThemeList, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedColorTheme"/>
    /// </summary>
    private string? _selectedColorTheme = string.Empty;

    /// <summary>
    /// Gets or sets the selected color theme
    /// </summary>
    public string? SelectedColorTheme
    {
        get => _selectedColorTheme;
        set
        {
            if (SetProperty(ref _selectedColorTheme, value) && !string.IsNullOrEmpty(value))
                Helper.SetColorTheme(value);
        }
    }
    #endregion

    #region Server
    /// <summary>
    /// Backing field for <see cref="ServerList"/>
    /// </summary>
    private ObservableCollection<ServerEntry> _serverList = new();

    /// <summary>
    /// Gets or sets the list with the server
    /// </summary>
    public ObservableCollection<ServerEntry> ServerList
    {
        get => _serverList;
        private set => SetProperty(ref _serverList, value);
    }

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
    /// Backing field for <see cref="ButtonMoveUpEnabled"/>
    /// </summary>
    private bool _buttonMoveUpEnabled;

    /// <summary>
    /// Gets or sets the value which indicates if the move up button is enabled
    /// </summary>
    public bool ButtonMoveUpEnabled
    {
        get => _buttonMoveUpEnabled;
        set => SetProperty(ref _buttonMoveUpEnabled, value);
    }

    /// <summary>
    /// Backing field for <see cref="ButtonMoveDownEnabled"/>
    /// </summary>
    private bool _buttonMoveDownEnabled;

    /// <summary>
    /// Gets or sets the value which indicates if the move down button is enabled
    /// </summary>
    public bool ButtonMoveDownEnabled
    {
        get => _buttonMoveDownEnabled;
        set => SetProperty(ref _buttonMoveDownEnabled, value);
    }

    /// <summary>
    /// Backing field for <see cref="ImportOverride"/>
    /// </summary>
    private bool _importOverride;

    /// <summary>
    /// Gets or sets the value which indicates if the values should be overwritten during the import
    /// </summary>
    public bool ImportOverride
    {
        get => _importOverride;
        set => SetProperty(ref _importOverride, value);
    }

    #endregion

    #region Filter
    /// <summary>
    /// Backing field for <see cref="FilterList"/>
    /// </summary>
    private ObservableCollection<FilterEntry> _filterList = new();

    /// <summary>
    /// Gets or sets the list with the filters
    /// </summary>
    public ObservableCollection<FilterEntry> FilterList
    {
        get => _filterList;
        private set => SetProperty(ref _filterList, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedFilter"/>
    /// </summary>
    private FilterEntry? _selectedFilter;

    /// <summary>
    /// Gets or sets the selected filter
    /// </summary>
    public FilterEntry? SelectedFilter
    {
        get => _selectedFilter;
        set => SetProperty(ref _selectedFilter, value);
    }

    /// <summary>
    /// Backing field for <see cref="FilterTypeList"/>
    /// </summary>
    private ObservableCollection<IdTextEntry> _filterTypeList = new();

    /// <summary>
    /// Gets or sets the list with the filter types
    /// </summary>
    public ObservableCollection<IdTextEntry> FilterTypeList
    {
        get => _filterTypeList;
        private set => SetProperty(ref _filterTypeList, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedFilterType"/>
    /// </summary>
    private IdTextEntry? _selectedFilterType;

    /// <summary>
    /// Gets or sets the selected filter type
    /// </summary>
    public IdTextEntry? SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    /// <summary>
    /// Backing field for <see cref="FilterValue"/>
    /// </summary>
    private string _filterValue = string.Empty;

    /// <summary>
    /// Gets or sets the filter value
    /// </summary>
    public string FilterValue
    {
        get => _filterValue;
        set => SetProperty(ref _filterValue, value);
    }
    #endregion

    #region Various

    /// <summary>
    /// Backing field for <see cref="ExportTypes"/>
    /// </summary>
    private ObservableCollection<IdTextEntry> _exportTypes = new();

    /// <summary>
    /// Gets or sets the list with the export types
    /// </summary>
    public ObservableCollection<IdTextEntry> ExportTypes
    {
        get => _exportTypes;
        private set => SetProperty(ref _exportTypes, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedExportType"/>
    /// </summary>
    private IdTextEntry? _selectedExportType;

    /// <summary>
    /// Gets or sets the selected export type
    /// </summary>
    public IdTextEntry? SelectedExportType
    {
        get => _selectedExportType;
        set => SetProperty(ref _selectedExportType, value);
    }

    /// <summary>
    /// Backing field for <see cref="SearchHistoryCount"/>
    /// </summary>
    private int _searchHistoryCount = 50;

    /// <summary>
    /// Gets or sets the search history count
    /// </summary>
    public int SearchHistoryCount
    {
        get => _searchHistoryCount;
        set => SetProperty(ref _searchHistoryCount, value);
    }
    #endregion

    #endregion

    #region Commands
    /// <summary>
    /// The command to save the theme
    /// </summary>
    public ICommand SaveThemeCommand => new RelayCommand(SaveTheme);

    /// <summary>
    /// The command to add a new server
    /// </summary>
    public ICommand AddServerCommand => new RelayCommand(AddServer);

    /// <summary>
    /// The command to edit the selected server
    /// </summary>
    public ICommand EditServerCommand => new RelayCommand(EditServer);

    /// <summary>
    /// The command to delete the selected server
    /// </summary>
    public ICommand DeleteServerCommand => new RelayCommand(DeleteServer);

    /// <summary>
    /// The command to move the serer up / down
    /// </summary>
    public ICommand MoveServerCommand => new RelayCommand<MoveDirection>(MoveServer);

    /// <summary>
    /// The command to add a new filter
    /// </summary>
    public ICommand AddFilterCommand => new RelayCommand(AddFilter);

    /// <summary>
    /// The command to delete the selected filter
    /// </summary>
    public ICommand DeleteFilterCommand => new RelayCommand(DeleteFilter);

    /// <summary>
    /// The command to save the various settings
    /// </summary>
    public ICommand SaveVariousCommand => new RelayCommand(SaveVariousSettings);

    /// <summary>
    /// The command which occurs when the user hits the export button
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(ExportSettings);

    /// <summary>
    /// The command which occurs when the user hits the import button
    /// </summary>
    public ICommand ImportCommand => new RelayCommand(ImportSettings);
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
            // Init the filter types
            FilterTypeList = Helper.CreateFilterTypeList().ToObservableCollection();

            // Load the colors
            ColorThemeList = ThemeManager.Current.ColorSchemes.ToObservableCollection();
            var themeName = await _manager!.LoadSettingsValueAsync(SettingsKey.ColorScheme, DefaultEntries.ColorScheme);
            SelectedColorTheme = ColorThemeList.FirstOrDefault(f => f.Equals(themeName, StringComparison.OrdinalIgnoreCase));

            // Load the server
            await LoadServerAsync();

            // Load the filters
            await LoadFilterAsync();

            // Set the various data
            var exportList = Helper.CreateExportTypeList(ExportDataType.List);
            ExportTypes = new ObservableCollection<IdTextEntry>(exportList);
            var exportType = await _manager.LoadSettingsValueAsync(SettingsKey.CopyToClipboardFormat, DefaultEntries.CopyToClipboardFormat); // 1 = CSV
            SelectedExportType = exportList.FirstOrDefault(f => f.Id == exportType);

            SearchHistoryCount = await _manager.LoadSettingsValueAsync(SettingsKey.SearchHistoryEntryCount, DefaultEntries.SearchHistoryCount);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Saves the current theme
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async void SaveTheme()
    {
        if (string.IsNullOrEmpty(SelectedColorTheme))
            return;

        await ShowProgressAsync("Save", "Please wait while saving the theme...");

        try
        {
            await _manager!.SaveSettingsValueAsync(SettingsKey.ColorScheme, SelectedColorTheme);
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
    private async void AddServer()
    {
        var dialog = new EditServerWindow { Owner = Application.Current.MainWindow };
        if (dialog.ShowDialog() == false)
            return;


        var existingServer = ServerList.FirstOrDefault(f => f.Name.Equals(dialog.SelectedServer.Name, StringComparison.OrdinalIgnoreCase));
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
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Edits the selected server
    /// </summary>
    private async void EditServer()
    {
        if (SelectedServer == null)
            return;

        var dialog = new EditServerWindow
        {
            SelectedServer = SelectedServer,
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == false)
            return;

        // Update the server
        try
        {
            await _manager!.UpdateServerAsync(SelectedServer);

            SetServerList(SelectedServer);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Deletes the selected server
    /// </summary>
    private async void DeleteServer()
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
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Moves the selected server up or down
    /// </summary>
    /// <param name="direction">The desired direction</param>
    private async void MoveServer(MoveDirection direction)
    {
        if (SelectedServer == null)
            return;

        try
        {
            await _manager!.MoveServerOrderAsync(SelectedServer, direction == MoveDirection.Up);

            SetMovementButtons(SelectedServer);

            ServerList = ServerList.OrderBy(o => o.Order).ToObservableCollection();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
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
    private async void AddFilter()
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
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Deletes the selected filter
    /// </summary>
    private async void DeleteFilter()
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
            await ShowErrorAsync(ex);
        }
    }
    #endregion

    #region Various

    /// <summary>
    /// Saves the various settings
    /// </summary>
    private async void SaveVariousSettings()
    {
        if (SelectedExportType == null)
            return;

        try
        {
            var saveList = new SortedList<SettingsKey, object>
            {
                {SettingsKey.CopyToClipboardFormat, SelectedExportType.Id},
                {SettingsKey.SearchHistoryEntryCount, SearchHistoryCount}
            };

            await _manager!.SaveSettingsValuesAsync(saveList);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Exports the settings as JSON file
    /// </summary>
    private async void ExportSettings()
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

        await ShowProgressAsync("Export", "Please wait while exporting the settings...");
        try
        {
            await _manager.ExportSettingsAsync(dialog.FileName);
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
    /// Imports the selected settings (JSON file)
    /// </summary>
    private async void ImportSettings()
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

        await ShowProgressAsync("Import", "Please wait while importing the settings...");
        try
        {
            await _manager.ImportSettingsAsync(dialog.FileName, ImportOverride);
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
    #endregion
}