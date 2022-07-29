using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Internal;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.EditServerWindow"/>
/// </summary>
internal class EditServerWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The default database entry
    /// </summary>
    private const string DefaultEntry = "<Select database>";

    /// <summary>
    /// The action to close the window
    /// </summary>
    private Action<bool>? _closeWindow;

    /// <summary>
    /// Backing field for <see cref="SelectedServer"/>
    /// </summary>
    private ServerEntry _selectedServer = new();

    /// <summary>
    /// Gets or sets the selected server
    /// </summary>
    public ServerEntry SelectedServer
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
        set
        {
            SetField(ref _selectedDatabase, value);
            AutoConnectEnabled = !string.IsNullOrEmpty(value) && !value.Equals(DefaultEntry);
        }
    }

    /// <summary>
    /// Backing field for <see cref="ButtonSelectedEnabled"/>
    /// </summary>
    private bool _buttonSelectedEnabled;

    /// <summary>
    /// Gets or sets the value which indicates if the select button is enabled
    /// </summary>
    public bool ButtonSelectedEnabled
    {
        get => _buttonSelectedEnabled;
        set => SetField(ref _buttonSelectedEnabled, value);
    }

    /// <summary>
    /// Backing field for <see cref="AutoConnectEnabled"/>
    /// </summary>
    private bool _autoConnectEnabled;

    /// <summary>
    /// Gets or sets the value which indicates if the auto connect checkbox is enabled
    /// </summary>
    public bool AutoConnectEnabled
    {
        get => _autoConnectEnabled;
        set => SetField(ref _autoConnectEnabled, value);
    }

    /// <summary>
    /// The command to establish a connection to the desired server
    /// </summary>
    public ICommand ConnectCommand => new DelegateCommand(Connect);

    /// <summary>
    /// The command to set the data
    /// </summary>
    public ICommand OkCommand => new DelegateCommand(SetData);

    /// <summary>
    /// The command to close the window
    /// </summary>
    public ICommand CloseCommand => new DelegateCommand(() => _closeWindow?.Invoke(false));

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="closeWindow">The action to close the window with the desired dialog result</param>
    public void InitViewModel(Action<bool> closeWindow)
    {
        _closeWindow = closeWindow;

        Connect();
    }

    /// <summary>
    /// Creates a connection to the MSSQL server and loads the available databases
    /// </summary>
    private async void Connect()
    {
        if (string.IsNullOrWhiteSpace(SelectedServer.Name))
            return;

        DatabaseList.Clear();

        try
        {
            using var baseRepo = new BaseRepo(SelectedServer.Name);
            var databases = await baseRepo.LoadDatabasesAsync();

            DatabaseList = new ObservableCollection<string>
            {
                DefaultEntry
            };

            foreach (var database in databases.OrderBy(o => o))
            {
                DatabaseList.Add(database);
            }

            ButtonSelectedEnabled = databases.Any();

            if (!string.IsNullOrEmpty(SelectedServer.DefaultDatabase))
            {
                SelectedDatabase = DatabaseList.FirstOrDefault(f => f.Equals(SelectedServer.DefaultDatabase)) ??
                                   DefaultEntry;
            }
        }
        catch (Exception ex)
        {
            LogError(ex);
            ShowInfoMessage("Error: Can't connect to server!");
        }
    }

    /// <summary>
    /// Selects the database 
    /// </summary>
    private void SetData()
    {
        if (string.IsNullOrEmpty(SelectedServer.Name))
        {
            ShowInfoMessage("Please specify a server...");
            return;
        }

        SelectedServer.DefaultDatabase = SelectedDatabase;

        _closeWindow?.Invoke(true);
    }
}