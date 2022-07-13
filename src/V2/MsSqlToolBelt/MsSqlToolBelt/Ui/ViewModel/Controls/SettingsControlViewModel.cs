﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.Ui.View.Windows;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.SettingsControl"/>
/// </summary>
internal class SettingsControlViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private readonly SettingsManager _manager = new();

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
        set => SetField(ref _colorThemeList, value);
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
            if (SetField(ref _selectedColorTheme, value) && !string.IsNullOrEmpty(value))
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
        private set => SetField(ref _serverList, value);
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
            SetField(ref _selectedServer, value);
            ButtonMoveDownEnabled = false;
            ButtonMoveUpEnabled = false;

            if (value == null)
                return;

            var maxOrder = ServerList.Max(m => m.Order);
            if (value.Order >= 1 && value.Order < maxOrder)
                ButtonMoveDownEnabled = true;

            if (value.Order > 1 && value.Order <= maxOrder)
                ButtonMoveUpEnabled = true;
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
        set => SetField(ref _buttonMoveUpEnabled, value);
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
        set => SetField(ref _buttonMoveDownEnabled, value);
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
        private set => SetField(ref _filterList, value);
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
        set => SetField(ref _selectedFilter, value);
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
        private set => SetField(ref _filterTypeList, value);
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
        set => SetField(ref _selectedFilterType, value);
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
        set => SetField(ref _filterValue, value);
    }
    #endregion

    #endregion

    #region Commands
    /// <summary>
    /// The command to save the theme
    /// </summary>
    public ICommand SaveThemeCommand => new DelegateCommand(SaveTheme);

    /// <summary>
    /// The command to add a new server
    /// </summary>
    public ICommand AddServerCommand => new DelegateCommand(AddServer);

    /// <summary>
    /// The command to delete the selected server
    /// </summary>
    public ICommand DeleteServerCommand => new DelegateCommand(DeleteServer);

    /// <summary>
    /// The command to move the serer up / down
    /// </summary>
    public ICommand MoveServerCommand => new RelayCommand<MoveDirection>(MoveServer);

    /// <summary>
    /// The command to add a new filter
    /// </summary>
    public ICommand SaveFilterCommand => new DelegateCommand(SaveFilter);

    /// <summary>
    /// The command to delete the selected filter
    /// </summary>
    public ICommand DeleteFilterCommand => new DelegateCommand(DeleteFilter);
    #endregion


    /// <summary>
    /// Init the view model
    /// </summary>
    public void InitViewModel()
    {
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
            FilterTypeList = new ObservableCollection<IdTextEntry>(Helper.CreateFilterTypeList());

            // Load the colors
            ColorThemeList = new ObservableCollection<string>(ThemeManager.Current.ColorSchemes);
            var themeName = await _manager.LoadSettingsValueAsync(SettingsKey.ColorScheme, "Emerald");
            SelectedColorTheme = ColorThemeList.FirstOrDefault(f => f.Equals(themeName, StringComparison.OrdinalIgnoreCase));

            // Load the server
            await LoadServerAsync();

            // Load the filters
            await LoadFilterAsync();
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

        var controller = await ShowProgressAsync("Save", "Please wait while saving the theme...");

        try
        {
            await _manager.SaveSettingsValueAsync(SettingsKey.ColorScheme, SelectedColorTheme);
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
        await _manager.LoadServerAsync(true);
        
        SetServerList(preSelection);
    }

    /// <summary>
    /// Sets the server list
    /// </summary>
    private void SetServerList(ServerEntry? preSelection = null)
    {
        ServerList = new ObservableCollection<ServerEntry>(_manager.ServerList.OrderBy(o => o.Order));

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


        var existingServer = ServerList.FirstOrDefault(f => f.Name.Equals(dialog.SelectedServer, StringComparison.OrdinalIgnoreCase));
        if (existingServer != null)
        {
            SelectedServer = existingServer;
            return;
        }

        // Add the new server
        try
        {
            var newServer = new ServerEntry(dialog.SelectedServer, dialog.SelectedDatabase);
            await _manager.AddServerAsync(newServer);

            SetServerList(newServer);
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
            await _manager.DeleteServerAsync(SelectedServer);

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
            await _manager.MoveServerOrderAsync(SelectedServer, direction == MoveDirection.Up);

            ServerList = new ObservableCollection<ServerEntry>(ServerList.OrderBy(o => o.Order));
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }
    #endregion

    #region Filter
    /// <summary>
    /// Loads the filters
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async Task LoadFilterAsync()
    {
        await _manager.LoadFilterAsync();

        SetFilterList();
    }

    /// <summary>
    /// Sets the list of filters
    /// </summary>
    /// <param name="preSelection">The entry which should be selected</param>
    private void SetFilterList(FilterEntry? preSelection = null)
    {
        FilterList = new ObservableCollection<FilterEntry>(_manager.FilterList);

        if (preSelection == null)
            return;

        SelectedFilter = preSelection;
    }

    /// <summary>
    /// Saves the current filter
    /// </summary>
    private async void SaveFilter()
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
            await _manager.AddFilterAsync(entry);

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
            await _manager.DeleteFilterAsync(SelectedFilter);

            SetFilterList();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }
    #endregion
}