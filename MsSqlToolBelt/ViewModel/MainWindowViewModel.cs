using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.View;
using ZimLabs.Database.MsSql;
using ZimLabs.WpfBase;
using Application = System.Windows.Application;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Contains the logic for the main window
    /// </summary>
    internal sealed class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// The action to set the connector of the user controls
        /// </summary>
        private Action<Connector> _setConnector;

        /// <summary>
        /// The action to load the data of the selected tab
        /// </summary>
        private Action<int> _loadData;

        /// <summary>
        /// The action to clear the content of the controls
        /// </summary>
        private Action _clearControls;

        /// <summary>
        /// The instance for the interaction with the database
        /// </summary>
        private Repo _repo;

        /// <summary>
        /// Backing field for <see cref="ServerList"/>
        /// </summary>
        private ObservableCollection<string> _serverList = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the list with the last 10 servers
        /// </summary>
        public ObservableCollection<string> ServerList
        {
            get => _serverList;
            set => SetField(ref _serverList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedServer"/>
        /// </summary>
        private string _selectedServer;

        /// <summary>
        /// Gets or sets the name of the server
        /// </summary>
        public string SelectedServer
        {
            get => _selectedServer;
            set => SetField(ref _selectedServer, value);
        }

        /// <summary>
        /// Backing field for <see cref="Databases"/>
        /// </summary>
        private ObservableCollection<string> _databases;

        /// <summary>
        /// Gets or sets the available databases
        /// </summary>
        public ObservableCollection<string> Databases
        {
            get => _databases;
            set => SetField(ref _databases, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedDatabase"/>
        /// </summary>
        private string _selectedDatabase;

        /// <summary>
        /// Gets or sets the selected database
        /// </summary>
        public string SelectedDatabase
        {
            get => _selectedDatabase;
            set => SetField(ref _selectedDatabase, value);
        }

        /// <summary>
        /// Backing field for <see cref="ServerConnected"/>
        /// </summary>
        private bool _serverConnected;

        /// <summary>
        /// Gets or sets the value which indicates if a connection was established to the server
        /// </summary>
        public bool ServerConnected
        {
            get => _serverConnected;
            set => SetField(ref _serverConnected, value);
        }

        /// <summary>
        /// Backing field for <see cref="Connected"/>
        /// </summary>
        private bool _connected;

        /// <summary>
        /// Gets or sets the value which indicates if a connection to the database was established
        /// </summary>
        public bool Connected
        {
            get => _connected;
            set => SetField(ref _connected, value);
        }

        /// <summary>
        /// Backing field for <see cref="Connection"/>
        /// </summary>
        private string _connection = "Not connected.";

        /// <summary>
        /// Gets or sets the connection info
        /// </summary>
        public string Connection
        {
            get => _connection;
            set => SetField(ref _connection, value);
        }

        /// <summary>
        /// Backing field for <see cref="Header"/>
        /// </summary>
        private string _header = "MsSql - Table Types";

        /// <summary>
        /// Gets or sets the header of the main window
        /// </summary>
        public string Header
        {
            get => _header;
            set => SetField(ref _header, value);
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
                    _loadData(value);
            }
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="setConnector">The action to set the connector of the user controls</param>
        /// <param name="loadData">The action to load the data</param>
        public void InitViewModel(Action<Connector> setConnector, Action<int> loadData, Action clearControls)
        {
            _setConnector = setConnector;
            _loadData = loadData;
            _clearControls = clearControls;

            LoadServerList();

            Header = $"MsSqlToolBelt - V{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        /// <summary>
        /// The command to create a connection
        /// </summary>
        public ICommand ConnectCommand => new DelegateCommand(Connect);

        /// <summary>
        /// The command to switch the database
        /// </summary>
        public ICommand SwitchCommand => new DelegateCommand(SwitchDatabase);

        /// <summary>
        /// The command to show the info
        /// </summary>
        public ICommand InfoCommand => new DelegateCommand(() =>
        {
            var infoWindow = new InfoWindow { Owner = Application.Current.MainWindow };
            infoWindow.ShowDialog();
        });

        /// <summary>
        /// The command to show the settings window
        /// </summary>
        public ICommand SettingsCommand => new DelegateCommand(() =>
        {
            var settingsWindow = new SettingsWindow {Owner = Application.Current.MainWindow};
            settingsWindow.ShowDialog();

            LoadServerList();
        });

        /// <summary>
        /// Loads the server list and adds them to the property
        /// </summary>
        private void LoadServerList()
        {
            ServerList = new ObservableCollection<string>(Helper.LoadServerList());
        }

        /// <summary>
        /// Tries to connect to the database
        /// </summary>
        private async void Connect()
        {
            if (string.IsNullOrEmpty(SelectedServer))
                return;

            ServerConnected = false;
            Connected = false;
            _clearControls();

            try
            {
                _repo = new Repo(SelectedServer);
                ServerConnected = true;
                Connection = $"Connected. Server: {SelectedServer}";

                Databases = new ObservableCollection<string>(_repo.LoadDatabases());
                SelectedDatabase = Databases.FirstOrDefault();

                SaveServer();
            }
            catch (Exception ex)
            {
                Connection = "Connection error.";
                await ShowMessage("Error", $"An error has occured: {ex.Message}");
                ServerConnected = false;
                Connected = false;
            }
        }

        /// <summary>
        /// Switches the database
        /// </summary>
        private async void SwitchDatabase()
        {
            if (string.IsNullOrEmpty(SelectedDatabase))
                return;

            try
            {
                _repo.SwitchDatabase(SelectedDatabase);
                Connected = true;
                Connection = $"Connected. Server: {SelectedServer} - Database: {SelectedDatabase}";

                _setConnector(_repo.Connector);
            }
            catch (Exception ex)
            {
                Connection = "Connection error.";
                await ShowMessage("Error", $"An error has occured: {ex.Message}");
                Connected = false;
            }
        }

        /// <summary>
        /// Saves the new server into the server list (properties)
        /// </summary>
        private void SaveServer()
        {
            Helper.SaveServer(SelectedServer);
        }

        /// <summary>
        /// Shows the info window
        /// </summary>
        private void ShowInfo()
        {
            
        }
    }
}
