using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Threading;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Updater;
using MsSqlToolBelt.Ui.View.Windows;
using Serilog;
using ZimLabs.CoreLib;
using ZimLabs.WpfBase.NetCore;
using Timer = System.Timers.Timer;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.MainWindow"/>
/// </summary>
internal class MainWindowViewModel : ViewModelBase
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
            SetField(ref _settingsOpen, value);

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
            SetField(ref _infoOpen, value);
            if (value)
                _initFlyOut?.Invoke(FlyOutType.Info);
        }
    }

    /// <summary>
    /// Backing field for <see cref="ServerList"/>
    /// </summary>
    private ObservableCollection<string> _serverList = new();

    /// <summary>
    /// Gets or sets the list with the available server
    /// </summary>
    public ObservableCollection<string> ServerList
    {
        get => _serverList;
        private set => SetField(ref _serverList, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedServer"/>
    /// </summary>
    private string _selectedServer = string.Empty;

    /// <summary>
    /// Gets or sets the selected server
    /// </summary>
    public string SelectedServer
    {
        get => _selectedServer;
        set => SetField(ref _selectedServer, value);
    }

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
        private set => SetField(ref _databaseList, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedDatabase"/>
    /// </summary>
    private string _selectedDatabase = string.Empty;

    /// <summary>
    /// Gets or sets the selected database
    /// </summary>
    public string SelectedDatabase
    {
        get => _selectedDatabase;
        set => SetField(ref _selectedDatabase, value);
    }

    /// <summary>
    /// Backing field for <see cref="ConnectionEstablished"/>
    /// </summary>
    private bool _connectionEstablished;

    /// <summary>
    /// Gets or sets the value which indicates if the connection was established
    /// </summary>
    public bool ConnectionEstablished
    {
        get => _connectionEstablished;
        set => SetField(ref _connectionEstablished, value);
    }

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
            if (SetField(ref _tabIndex, value) && value != 0)
                _loadData?.Invoke(value);
        }
    }

    /// <summary>
    /// Backing field for <see cref="ConnectionInfo"/>
    /// </summary>
    private string _connectionInfo = "Not connected";

    /// <summary>
    /// Gets or sets the connection info
    /// </summary>
    public string ConnectionInfo
    {
        get => _connectionInfo;
        private set => SetField(ref _connectionInfo, value);
    }

    /// <summary>
    /// Backing field for <see cref="MemoryUsage"/>
    /// </summary>
    private string _memoryUsage = string.Empty;

    /// <summary>
    /// Gets or sets the memory usage of the program
    /// </summary>
    public string MemoryUsage
    {
        get => _memoryUsage;
        private set => SetField(ref _memoryUsage, value);
    }

    /// <summary>
    /// Backing field for <see cref="BuildInfo"/>
    /// </summary>
    private string _buildInfo = string.Empty;

    /// <summary>
    /// Gets or sets the build information
    /// </summary>
    public string BuildInfo
    {
        get => _buildInfo;
        private set => SetField(ref _buildInfo, value);
    }

    /// <summary>
    /// Backing field for <see cref="HeaderApp"/>
    /// </summary>
    private string _headerApp = "MsSqlToolBelt";

    /// <summary>
    /// Gets or sets the app header
    /// </summary>
    public string HeaderApp
    {
        get => _headerApp;
        private set => SetField(ref _headerApp, value);
    }

    /// <summary>
    /// Backing field for <see cref="ButtonUpdateVisible"/>
    /// </summary>
    private Visibility _buttonUpdateVisible = Visibility.Hidden;

    /// <summary>
    /// Gets or sets the visibility value of the update button
    /// </summary>
    public Visibility ButtonUpdateVisible
    {
        get => _buttonUpdateVisible;
        set => SetField(ref _buttonUpdateVisible, value);
    }
    #endregion

    #region Commands
    /// <summary>
    /// The command to open the settings control
    /// </summary>
    public ICommand OpenSettingsCommand => new DelegateCommand(() => SettingsOpen = !SettingsOpen);

    /// <summary>
    /// The command to show the info
    /// </summary>
    public ICommand InfoCommand => new DelegateCommand(() => InfoOpen = !InfoOpen);

    /// <summary>
    /// The command to connect to the server
    /// </summary>
    public ICommand ConnectServerCommand => new DelegateCommand(ConnectServer);

    /// <summary>
    /// The command to set the database
    /// </summary>
    public ICommand ConnectDatabaseCommand => new DelegateCommand(ConnectDatabase);

    /// <summary>
    /// The command to show the update window
    /// </summary>
    public ICommand ShowUpdateInfoCommand => new DelegateCommand(() =>
    {
        var dialog = new UpdateWindow(_releaseInfo) { Owner = Application.Current.MainWindow };
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
    public async void InitViewModel(SettingsManager settingsManager, Action<FlyOutType> initFlyOut, Action<string, string> setConnection, Action<int> loadData)
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

        var (version, buildInfo) = Helper.GetVersionInfo();
        BuildInfo = buildInfo;
        HeaderApp = $"MsSqlToolBelt | v{version}";

        try
        {
            // Check for an update
            CheckUpdate();

            await _settingsManager.LoadServerAsync();
            ServerList =
                new ObservableCollection<string>(_settingsManager.ServerList.OrderBy(o => o.Order).Select(s => s.Name));
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
            await _settingsManager.LoadServerAsync();
            ServerList =
                new ObservableCollection<string>(_settingsManager.ServerList.OrderBy(o => o.Order).Select(s => s.Name));
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Creates a connection to the selected server
    /// </summary>
    private async void ConnectServer()
    {
        if (string.IsNullOrEmpty(SelectedServer))
            return;

        ConnectionEstablished = false;

        await ShowProgressAsync("Connect", "Please wait while the connection to the server is established...");

        try
        {
            _baseRepo = new BaseRepo(SelectedServer);

            // Load the databases
            var databases = await _baseRepo.LoadDatabasesAsync();
            DatabaseList = new ObservableCollection<string>(databases);

            if (_settingsManager == null)
                return;
            // Add the server if it's not in the list
            await _settingsManager.AddServerAsync(SelectedServer);

            var defaultDatabase = await _settingsManager.LoadDefaultDatabaseAsync(SelectedServer);
            SelectedDatabase =
                DatabaseList.FirstOrDefault(f => f.EqualsIgnoreCase(defaultDatabase)) ?? "";

            ConnectionInfo = $"Connected. Server: {SelectedServer}";
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
    /// Selects the desired database
    /// </summary>
    private async void ConnectDatabase()
    {
        if (_baseRepo == null || string.IsNullOrEmpty(SelectedDatabase))
            return;

        await ShowProgressAsync("Please wait",
            "Please wait while the connection to the database is established...");

        try
        {
            _setConnection?.Invoke(SelectedServer, SelectedDatabase);

            ConnectionEstablished = true;

            ConnectionInfo = $"Connected. Server: {SelectedServer} - Database: {SelectedDatabase}";
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
}