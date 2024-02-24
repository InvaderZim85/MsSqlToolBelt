using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.Internal;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.EditServerWindow"/>
/// </summary>
internal partial class EditServerWindowViewModel : ViewModelBase
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
    /// Function to check if the name / default database is unique
    /// </summary>
    private Func<string, string, Task<bool>>? _checkFunc;

    /// <summary>
    /// The selected server
    /// </summary>
    [ObservableProperty]
    private ServerEntry _selectedServer = new();

    /// <summary>
    /// The list with the databases
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _databaseList = [];

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
            SetProperty(ref _selectedDatabase, value);
            AutoConnectEnabled = !string.IsNullOrEmpty(value) && !value.Equals(DefaultEntry);
        }
    }

    /// <summary>
    /// The value which indicates if the select button is enabled
    /// </summary>
    [ObservableProperty]
    private bool _buttonSelectedEnabled;

    /// <summary>
    /// The value which indicates if the auto connect checkbox is enabled
    /// </summary>
    [ObservableProperty]
    private bool _autoConnectEnabled;

    /// <summary>
    /// The command to close the window
    /// </summary>
    public ICommand CloseCommand => new RelayCommand(() => _closeWindow?.Invoke(false));

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="closeWindow">The action to close the window with the desired dialog result</param>
    /// <param name="checkEntry">The function to check if the entry is unique or not</param>
    public async void InitViewModel(Action<bool> closeWindow, Func<string, string, Task<bool>> checkEntry)
    {
        _closeWindow = closeWindow;
        _checkFunc = checkEntry;

        await ConnectAsync();
    }

    /// <summary>
    /// Creates a connection to the MSSQL server and loads the available databases
    /// </summary>
    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedServer.Name))
            return;

        DatabaseList.Clear();

        try
        {
            using var baseRepo = new BaseRepo(SelectedServer.Name);
            var databases = await baseRepo.LoadDatabasesAsync();

            DatabaseList = [DefaultEntry];

            foreach (var database in databases.OrderBy(o => o))
            {
                DatabaseList.Add(database);
            }

            ButtonSelectedEnabled = databases.Count > 0;

            if (!string.IsNullOrEmpty(SelectedServer.DefaultDatabase))
            {
                SelectedDatabase = DatabaseList.FirstOrDefault(f => f.Equals(SelectedServer.DefaultDatabase)) ??
                                   DefaultEntry;
            }
            else
            {
                SelectedDatabase = DefaultEntry;
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
    [RelayCommand]
    private async Task SetDataAsync()
    {
        if (string.IsNullOrEmpty(SelectedServer.Name))
        {
            ShowInfoMessage("Please specify a server...");
            return;
        }

        // Check name / database
        if (_checkFunc != null && !await _checkFunc(SelectedServer.Name, SelectedDatabase))
        {
            ShowInfoMessage("Server / Database already exists...");
            return;
        }

        SelectedServer.DefaultDatabase = SelectedDatabase;


        _closeWindow?.Invoke(true);
    }
}