using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MsSqlToolBelt.Data;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.EditServerWindow"/>
/// </summary>
internal class EditServerWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Backing field for <see cref="Server"/>
    /// </summary>
    private string? _server;

    /// <summary>
    /// Gets or sets the name / path of the desired server
    /// </summary>
    public string? Server
    {
        get => _server;
        set => SetField(ref _server, value);
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
        set => SetField(ref _databaseList, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedDatabase"/>
    /// </summary>
    private string? _selectedDatabase;

    /// <summary>
    /// Gets or sets the selected database
    /// </summary>
    public string? SelectedDatabase
    {
        get => _selectedDatabase;
        set => SetField(ref _selectedDatabase, value);
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
    /// The command to establish a connection to the desired server
    /// </summary>
    public ICommand ConnectCommand => new DelegateCommand(Connect);

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="selectedServer">The name / path of the selected server</param>
    public void InitViewModel(string selectedServer)
    {
        if (string.IsNullOrWhiteSpace(selectedServer))
            return;

        Server = selectedServer;

        Connect();
    }

    /// <summary>
    /// Creates a connection to the MSSQL server and loads the available databases
    /// </summary>
    private async void Connect()
    {
        if (string.IsNullOrWhiteSpace(Server))
            return;

        DatabaseList.Clear();

        try
        {
            using var baseRepo = new BaseRepo(Server);
            var databases = await baseRepo.LoadDatabasesAsync();

            const string defaultSelection = "<Select database>";
            DatabaseList = new ObservableCollection<string>
            {
                defaultSelection
            };

            foreach (var database in databases.OrderBy(o => o))
            {
                DatabaseList.Add(database);
            }

            SelectedDatabase = defaultSelection;
            ButtonSelectedEnabled = databases.Any();
        }
        catch (Exception ex)
        {
            LogError(ex);
            ShowInfoMessage("Error: Can't connect to server!");
        }
    }
}