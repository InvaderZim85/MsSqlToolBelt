using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data;
using ZimLabs.CoreLib;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

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

    /// <summary>
    /// The instance of the base repo
    /// </summary>
    private BaseRepo? _baseRepo;

    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private SettingsManager? _settingsManager;


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

    #endregion

    #region Commands
    /// <summary>
    /// The command to open the settings control
    /// </summary>
    public ICommand OpenSettingsCommand => new DelegateCommand(() => SettingsOpen = !SettingsOpen);

    /// <summary>
    /// The command to connect to the server
    /// </summary>
    public ICommand ConnectServerCommand => new DelegateCommand(ConnectServer);

    /// <summary>
    /// The command to set the database
    /// </summary>
    public ICommand ConnectDatabaseCommand => new DelegateCommand(ConnectDatabase);
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

        var controller = await ShowProgressAsync("Connect", "Please wait while the connection to the server is established...");

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
    private async void ConnectDatabase()
    {
        if (_baseRepo == null || string.IsNullOrEmpty(SelectedDatabase))
            return;

        var controller = await ShowProgressAsync("Please wait",
            "Please wait while the connection to the database is established...");

        try
        {
            _setConnection?.Invoke(SelectedServer, SelectedDatabase);

            ConnectionEstablished = true;
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
}