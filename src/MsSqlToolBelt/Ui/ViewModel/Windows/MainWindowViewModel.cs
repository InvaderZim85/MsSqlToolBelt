﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.Threading;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.DataObjects.Updater;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Windows;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ZimLabs.CoreLib;
using Timer = System.Timers.Timer;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.MainWindow"/>
/// </summary>
internal partial class MainWindowViewModel : ViewModelBase
{
    #region Actions
    /// <summary>
    /// The action to initialize the specified fly out
    /// </summary>
    private Action<FlyOutType>? _initFlyOut;

    /// <summary>
    /// The action to set the repo
    /// </summary>
    private Action<string, string>? _setConnection;

    /// <summary>
    /// The action to load the data of the selected tab
    /// </summary>
    private Action<int>? _loadData;
    #endregion

    #region Global variables

    /// <summary>
    /// The instance of the base repo
    /// </summary>
    private BaseRepo? _baseRepo;

    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private SettingsManager? _settingsManager;

    /// <summary>
    /// The instance for the memory analysis
    /// </summary>
    private Timer? _memoryTimer;

    /// <summary>
    /// Contains the max. memory usage
    /// </summary>
    private long _maxMemUsage;

    /// <summary>
    /// Contains the release info
    /// </summary>
    private ReleaseInfo _releaseInfo = new();
    #endregion

    #region View Properties
    /// <summary>
    /// Backing field for <see cref="SettingsOpen"/>
    /// </summary>
    private bool _settingsOpen;

    /// <summary>
    /// Gets or sets the value which indicates if the settings fly out is open
    /// </summary>
    public bool SettingsOpen
    {
        get => _settingsOpen;
        set
        {
            SetProperty(ref _settingsOpen, value);

            switch (value)
            {
                case true:
                    _initFlyOut?.Invoke(FlyOutType.Settings);
                    break;
                case false:
                    LoadServerList();
                    break;
            }
        }
    }

    /// <summary>
    /// Backing field for <see cref="InfoOpen"/>
    /// </summary>
    private bool _infoOpen;

    /// <summary>
    /// Gets or sets the value which indicates if the info control is open
    /// </summary>
    public bool InfoOpen
    {
        get => _infoOpen;
        set
        {
            SetProperty(ref _infoOpen, value);
            if (value)
                _initFlyOut?.Invoke(FlyOutType.Info);
        }
    }

    /// <summary>
    /// The list with the available server
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ServerEntry> _serverList = new();

    /// <summary>
    /// The selected server
    /// </summary>
    [ObservableProperty]
    private ServerEntry? _selectedServer;

    /// <summary>
    /// Backing field for <see cref="DatabaseList"/>
    /// </summary>
    private ObservableCollection<string> _databaseList = new();

    /// <summary>
    /// Gets or sets the list with the databases
    /// </summary>
    public ObservableCollection<string> DatabaseList
    {
        get => _databaseList;
        private set
        {
            SetProperty(ref _databaseList, value);
            ConnectedToServer = value.Any();
        }
    }

    /// <summary>
    /// The selected database
    /// </summary>
    [ObservableProperty]
    private string _selectedDatabase = string.Empty;

    /// <summary>
    /// The value which indicates if a server connection is established
    /// </summary>
    [ObservableProperty]
    private bool _connectedToServer;

    /// <summary>
    /// The value which indicates if the connection was established
    /// </summary>
    [ObservableProperty] 
    private bool _connectionEstablished;

    /// <summary>
    /// Backing field for <see cref="TabIndex"/>
    /// </summary>
    private int _tabIndex;

    /// <summary>
    /// Gets or sets the tab index
    /// </summary>
    public int TabIndex
    {
        get => _tabIndex;
        set
        {
            if (SetProperty(ref _tabIndex, value))
                _loadData?.Invoke(value);
        }
    }

    /// <summary>
    /// The connection info
    /// </summary>
    [ObservableProperty]
    private string _connectionInfo = "Not connected";

    /// <summary>
    /// The memory usage of the program
    /// </summary>
    [ObservableProperty]
    private string _memoryUsage = string.Empty;

    /// <summary>
    /// The build information
    /// </summary>
    [ObservableProperty]
    private string _buildInfo = "Build info";

    /// <summary>
    /// The app header
    /// </summary>
    [ObservableProperty]
    private string _headerApp = "MsSqlToolBelt";

    /// <summary>
    /// The visibility value of the update button
    /// </summary>
    [ObservableProperty]
    private Visibility _buttonUpdateVisible = Visibility.Hidden;

    #endregion

    #region Commands
    /// <summary>
    /// The command to open the settings control
    /// </summary>
    public ICommand OpenSettingsCommand => new RelayCommand(() => SettingsOpen = !SettingsOpen);

    /// <summary>
    /// The command to show the info
    /// </summary>
    public ICommand InfoCommand => new RelayCommand(() => InfoOpen = !InfoOpen);

    /// <summary>
    /// The command to show the update window
    /// </summary>
    public ICommand ShowUpdateInfoCommand => new RelayCommand(() =>
    {
        var dialog = new UpdateWindow(_releaseInfo) { Owner = Application.Current.MainWindow };
        dialog.ShowDialog();
    });

    /// <summary>
    /// The command which occurs when the user hits the template manager menu item (main menu)
    /// </summary>
    public ICommand ShowTemplateManagerCommand => new RelayCommand(() =>
    {
        var dialog = new TemplateWindow {Owner = Application.Current.MainWindow};
        dialog.ShowDialog();
    });

    /// <summary>
    /// The command which occurs when the user hits the show data type menu item
    /// </summary>
    public ICommand ShowDataTypeCommand => new RelayCommand(() =>
    {
        var dialog = new DataTypeWindow {Owner = Application.Current.MainWindow};
        dialog.ShowDialog();
    });
    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="settingsManager">The instance of the settings manager</param>
    /// <param name="initFlyOut">The action to initialize the flyout</param>
    /// <param name="setConnection">The action to set the connection</param>
    /// <param name="loadData">The action to load the data of the selected tab</param>
    public void InitViewModel(SettingsManager settingsManager, Action<FlyOutType> initFlyOut, Action<string, string> setConnection, Action<int> loadData)
    {
        _settingsManager = settingsManager;
        _initFlyOut = initFlyOut;
        _setConnection = setConnection;
        _loadData = loadData;

        _memoryTimer = new Timer(1000);
        _memoryTimer.Elapsed += (_, _) =>
        {
            var memUsage = Process.GetCurrentProcess().PrivateMemorySize64;
            MemoryUsage = $"Memory usage: {memUsage.ConvertSize()}";

            if (memUsage > _maxMemUsage)
                _maxMemUsage = memUsage;
        };
        _memoryTimer.Start();

        var (_, buildInfo) = Helper.GetVersionInfo();
        BuildInfo = $"Build date: {buildInfo}";
        SetHeaderInfo();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    public async void LoadData()
    {
        try
        {
            // Check for an update
            CheckUpdate();

            LoadServerList();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Closes the view model and stops / dispose all necessary
    /// </summary>
    public void CloseViewModel()
    {
        _memoryTimer?.Dispose();
        _baseRepo?.Dispose();

        // Log the max. memory usage
        Log.Information($"Maximal memory usage: {_maxMemUsage.ConvertSize()} ({_maxMemUsage:N0} bytes)");
    }

    /// <summary>
    /// Reloads the server list
    /// </summary>
    private async void LoadServerList()
    {
        if (_settingsManager == null)
            return;

        try
        {
            var tmpSelectedServer = SelectedServer?.Clone() ?? null;
            await _settingsManager.LoadServerAsync();
            ServerList = _settingsManager.ServerList.OrderBy(o => o.Order).ToObservableCollection();

            if (tmpSelectedServer != null)
                SelectedServer = ServerList.FirstOrDefault(f => f.Id == tmpSelectedServer.Id);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Creates a connection to the selected server
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ConnectServerAsync()
    {
        if (SelectedServer == null)
            return;

        ConnectionEstablished = false;

        var controller = await ShowProgressAsync("Connect", "Please wait while the connection to the server is established...");

        try
        {
            _baseRepo = new BaseRepo(SelectedServer.Name);

            // Load the databases
            var databases = await _baseRepo.LoadDatabasesAsync();
            DatabaseList = new ObservableCollection<string>(databases);

            if (_settingsManager == null)
                return;
            // Add the server if it's not in the list
            await _settingsManager.AddServerAsync(SelectedServer);

            if (!string.IsNullOrEmpty(SelectedServer.DefaultDatabase))
                SelectedDatabase = DatabaseList.FirstOrDefault(f => f.EqualsIgnoreCase(SelectedServer.DefaultDatabase)) ?? "";

            if (SelectedServer.AutoConnect && !string.IsNullOrEmpty(SelectedDatabase))
                SetDatabase();
            else
            {
                ConnectionInfo = $"Connected. Server: {SelectedServer}";
                SetHeaderInfo($"Server: {SelectedServer}");
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
    /// Selects the desired database
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ConnectDatabaseAsync()
    {
        if (_baseRepo == null || SelectedServer == null || string.IsNullOrEmpty(SelectedDatabase))
            return;

        var controller = await ShowProgressAsync("Please wait",
            "Please wait while the connection to the database is established...");

        try
        {
            SetDatabase();
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
    /// Sets the database
    /// </summary>
    private void SetDatabase()
    {
        if (string.IsNullOrEmpty(SelectedDatabase))
            return;

        _setConnection?.Invoke(SelectedServer!.Name, SelectedDatabase);

        ConnectionEstablished = true;

        ConnectionInfo = $"Connected. Server: {SelectedServer} - Database: {SelectedDatabase}";
        SetHeaderInfo($"Server: {SelectedServer} - Database: {SelectedDatabase}");
    }

    /// <summary>
    /// Checks if a new version is available
    /// </summary>
    private void CheckUpdate()
    {
        // The "Forget()" method is used to let the async task run without waiting.
        // More information: https://docs.microsoft.com/en-us/answers/questions/186037/taskrun-without-wait.html
        // To use "Forget" you need the following nuget package: https://www.nuget.org/packages/Microsoft.VisualStudio.Threading/
        UpdateHelper.LoadReleaseInfoAsync(SetReleaseInfo).Forget();
    }

    /// <summary>
    /// Sets the release info and shows the update button
    /// </summary>
    /// <param name="releaseInfo">The infos of the latest release</param>
    private void SetReleaseInfo(ReleaseInfo releaseInfo)
    {
        _releaseInfo = releaseInfo;
        ButtonUpdateVisible = !string.IsNullOrEmpty(releaseInfo.Name) ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Sets the header info
    /// </summary>
    /// <param name="additionalText">The additional info</param>
    private void SetHeaderInfo(string additionalText = "")
    {
        HeaderApp = $"MsSqlToolBelt - v{Helper.GetVersionInfo().Version}";

        if (!string.IsNullOrEmpty(additionalText))
            HeaderApp += $" | {additionalText}";
    }
}