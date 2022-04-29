using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ControlzEx.Theming;
using MahApps.Metro.Accessibility;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
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
    /// Contains the value which indicates if the control is loading
    /// </summary>
    private bool _init;

    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private readonly SettingsManager _manager = new();

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
        set => SetField(ref _selectedColorTheme, value);
    }

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
        set => SetField(ref _serverList, value);
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
    /// Backing field for <see cref="FilterList"/>
    /// </summary>
    private ObservableCollection<FilterEntry> _filterList = new();

    /// <summary>
    /// Gets or sets the list with the filters
    /// </summary>
    public ObservableCollection<FilterEntry> FilterList
    {
        get => _filterList;
        set => SetField(ref _filterList, value);
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
        set => SetField(ref _filterTypeList, value);
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
        _init = true;

        try
        {
            // Init the filter types
            FilterTypeList = new ObservableCollection<IdTextEntry>(IdTextEntry.CreateList(typeof(FilterType)));

            // Load the colors
            ColorThemeList = new ObservableCollection<string>(ThemeManager.Current.ColorSchemes);
            var themeName = await _manager.LoadSettingsValueAsync(SettingsKey.ColorTheme, "Emarald");
            SelectedColorTheme = ColorThemeList.FirstOrDefault(f => f.Equals(themeName, StringComparison.OrdinalIgnoreCase));

            // Load the server
            await LoadServerAsync();

            // Load the filter
            FilterList = new ObservableCollection<FilterEntry>(await _manager.LoadFilterAsync(true));
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }

        _init = false;
    }

    /// <summary>
    /// Loads and shows the server
    /// </summary>
    /// <param name="preSelection">The server which should be selected (optional)</param>
    /// <returns>The awaitable task</returns>
    private async Task LoadServerAsync(ServerEntry? preSelection = null)
    {
        var serverList = await _manager.LoadServerAsync(true);
        ServerList = new ObservableCollection<ServerEntry>(serverList.OrderBy(o => o.Order));

        SelectedServer = preSelection ?? serverList.FirstOrDefault(); // Select the first server
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

            // Reload the server list...
            await LoadServerAsync(newServer);
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
            ServerList.Remove(SelectedServer);

            await _manager.DeleteServerAsync(SelectedServer);

            SelectedServer = null;
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
}