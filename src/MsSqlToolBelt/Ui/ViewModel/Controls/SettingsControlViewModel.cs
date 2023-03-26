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
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
    private ObservableCollection<string> _colorThemeList = new();

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
    /// The list with the server
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ServerEntry> _serverList = new();

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
    private ObservableCollection<FilterEntry> _filterList = new();

    /// <summary>
    /// The selected filter
    /// </summary>
    [ObservableProperty]
    private FilterEntry? _selectedFilter;

    /// <summary>
    /// The list with the filter types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IdTextEntry> _filterTypeList = new();

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
    private ObservableCollection<IdTextEntry> _exportTypes = new();

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
            // Init the filter types
            FilterTypeList = Helper.CreateFilterTypeList().ToObservableCollection();

            // Load the colors
            ColorThemeList = ThemeManager.Current.ColorSchemes.ToObservableCollection();
            var themeName = await SettingsManager.LoadSettingsValueAsync(SettingsKey.ColorScheme, DefaultEntries.ColorScheme);
            SelectedColorTheme = ColorThemeList.FirstOrDefault(f => f.Equals(themeName, StringComparison.OrdinalIgnoreCase));

            // Load the server
            await LoadServerAsync();

            // Load the filters
            await LoadFilterAsync();

            // Set the various data
            var exportList = Helper.CreateExportTypeList(ExportDataType.List);
            ExportTypes = new ObservableCollection<IdTextEntry>(exportList);
            var exportType = await SettingsManager.LoadSettingsValueAsync(SettingsKey.CopyToClipboardFormat, DefaultEntries.CopyToClipboardFormat); // 1 = CSV
            SelectedExportType = exportList.FirstOrDefault(f => f.Id == exportType);

            SearchHistoryCount = await SettingsManager.LoadSettingsValueAsync(SettingsKey.SearchHistoryEntryCount, DefaultEntries.SearchHistoryCount);
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
    [RelayCommand]
    private async Task SaveThemeAsync()
    {
        if (string.IsNullOrEmpty(SelectedColorTheme))
            return;

        var controller = await ShowProgressAsync("Save", "Please wait while saving the theme...");

        try
        {
            await SettingsManager.SaveSettingsValueAsync(SettingsKey.ColorScheme, SelectedColorTheme);
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
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task EditServerAsync()
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
            await SettingsManager.UpdateServerAsync(SelectedServer);

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
            await ShowErrorAsync(ex);
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
            await ShowErrorAsync(ex);
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
            await ShowErrorAsync(ex);
        }
    }
    #endregion

    #region Various

    /// <summary>
    /// Saves the various settings
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task SaveVariousSettingsAsync()
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

            await SettingsManager.SaveSettingsValuesAsync(saveList);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
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
            await ShowErrorAsync(ex);
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
            await ShowErrorAsync(ex);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }
    #endregion
} // 717