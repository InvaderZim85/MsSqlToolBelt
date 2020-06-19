using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.View;
using ZimLabs.CoreLib.Extensions;
using ZimLabs.Database.MsSql;
using ZimLabs.WpfBase;

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
        /// Init the view model
        /// </summary>
        /// <param name="setConnector">The action to set the connector of the user controls</param>
        public void InitViewModel(Action<Connector> setConnector)
        {
            _setConnector = setConnector;

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
        public ICommand InfoCommand => new DelegateCommand(ShowInfo);

        /// <summary>
        /// Loads the server list and adds them to the property
        /// </summary>
        private void LoadServerList()
        {
            var server = Properties.Settings.Default.ServerList;
            if (string.IsNullOrEmpty(server))
                return;

            var content =
                Properties.Settings.Default.ServerList.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if (content.Length == 0)
                return;

            ServerList = new ObservableCollection<string>(content);
        }

        /// <summary>
        /// Tries to connect to the database
        /// </summary>
        private async void Connect()
        {
            if (string.IsNullOrEmpty(SelectedServer))
                return;

            SaveServer();

            ServerConnected = false;
            Connected = false;

            try
            {
                _repo = new Repo(SelectedServer);
                ServerConnected = true;
                Connection = $"Connected. Server: {SelectedServer}";

                Databases = new ObservableCollection<string>(_repo.LoadDatabases());
                SelectedDatabase = Databases.FirstOrDefault();
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
            if (ServerList.Contains(SelectedServer))
                return;

            // If there are already 10 entries, remove the first one
            if (ServerList.Count == 10)
                ServerList.RemoveAt(0);

            ServerList.Add(SelectedServer);

            var server = ServerList.ToList().ToSeparatedString();
            Properties.Settings.Default.ServerList = server;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Shows the info window
        /// </summary>
        private void ShowInfo()
        {
            var infoWindow = new InfoWindow {Owner = Application.Current.MainWindow};
            infoWindow.ShowDialog();
        }
    }
}
