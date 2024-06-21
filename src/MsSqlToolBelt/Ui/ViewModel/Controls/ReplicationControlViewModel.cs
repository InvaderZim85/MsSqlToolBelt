using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.DataObjects.Table;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Interaction logic for <see cref="View.Controls.ReplicationControl"/>
/// </summary>
internal partial class ReplicationControlViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the replication data
    /// </summary>
    private ReplicationManager? _manager;

    /// <summary>
    /// Contains the value which indicates if the data already loaded
    /// </summary>
    private bool _dataLoaded;

    /// <summary>
    /// The action to set the CMD text
    /// </summary>
    private Action<string>? _setCmdText;

    #region View Properties

    /// <summary>
    /// Gets or sets the tab index
    /// </summary>
    [ObservableProperty]
    private int _tabIndex;

    /// <summary>
    /// The value which indicates if the info panel should be shown
    /// </summary>
    [ObservableProperty]
    private bool _showInfo = true;

    /// <summary>
    /// The value which indicates if the control is enabled
    /// </summary>
    [ObservableProperty]
    private bool _controlEnabled;

    /// <summary>
    /// Gets or sets the list with the publications
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _publications = [];

    /// <summary>
    /// Gets or sets the selected publication
    /// </summary>
    [ObservableProperty]
    private string _selectedPublication = string.Empty;

    /// <summary>
    /// Occurs when another publication was selected
    /// </summary>
    /// <param name="value">The name of the publication</param>
    partial void OnSelectedPublicationChanged(string value)
    {
        FilterList();
    }

    /// <summary>
    /// The list with the tables
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TableEntry> _tables = [];

    /// <summary>
    /// Backing field for <see cref="SelectedTable"/>
    /// </summary>
    [ObservableProperty]
    private TableEntry? _selectedTable;

    /// <summary>
    /// Occurs when the selected table changed
    /// </summary>
    /// <param name="value">The selected table</param>
    partial void OnSelectedTableChanged(TableEntry? value)
    {
        if (_manager == null)
            return;

        _manager.SelectedTable = value;

        if (_manager.SelectedTable is { Columns.Count: > 0 })
            SetAdditionalInformation();
        else
            EnrichData();
    }

    /// <summary>
    /// The list with the table columns
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColumnEntry> _columns = [];

    /// <summary>
    /// The list with the indices
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IndexEntry> _indexes = [];

    /// <summary>
    /// Gets or sets the list with the replication articles
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ReplicationArticle> _repArticles = [];

    /// <summary>
    /// Gets or sets the selected rep article
    /// </summary>
    [ObservableProperty]
    private ReplicationArticle? _selectedRepArticle;

    /// <summary>
    /// Occurs when the content of <see cref="SelectedRepArticle"/> was changed
    /// </summary>
    /// <param name="value">The selected value</param>
    partial void OnSelectedRepArticleChanged(ReplicationArticle? value)
    {
        _setCmdText?.Invoke(value?.FilterQuery ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the filter
    /// </summary>
    [ObservableProperty]
    private string _filter = string.Empty;

    /// <summary>
    /// Occurs when the filter was changed
    /// </summary>
    /// <param name="value">The new filter value</param>
    partial void OnFilterChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            FilterList();
    }

    /// <summary>
    /// The list header
    /// </summary>
    [ObservableProperty]
    private string _headerList = "Tables";

    /// <summary>
    /// The column header
    /// </summary>
    [ObservableProperty]
    private string _headerColumns = "Columns";

    /// <summary>
    /// The index header
    /// </summary>
    [ObservableProperty]
    private string _headerIndex = "Indexes";

    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="setCmdText">The action to set the text of the cmd control</param>
    public void InitViewModel(Action<string> setCmdText)
    {
        _setCmdText = setCmdText;
    }

    /// <summary>
    /// The command to reload the table types
    /// </summary>
    public ICommand ReloadCommand => new RelayCommand(() =>
    {
        _dataLoaded = false;
        LoadData();
    });

    /// <summary>
    /// Sets the connection
    /// </summary>
    /// <param name="dataSource">The data source</param>
    /// <param name="database">The database</param>
    public void SetConnection(string dataSource, string database)
    {
        // Clear the current result
        Tables = [];
        Columns = [];

        // Reset the manager
        _manager?.Dispose();
        _manager = new ReplicationManager(dataSource, database);

        // Reset the info values
        ShowInfo = false;
        ControlEnabled = true;

        // Reset the data loaded value
        _dataLoaded = false;
    }

    /// <summary>
    /// Closes the current connection
    /// </summary>
    public void CloseConnection()
    {
        _manager?.Dispose();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    /// <param name="showProgress"><see langword="true"/> to show the progress, <see langword="false"/> to hide the progress information (optional)</param>
    public async void LoadData(bool showProgress = true)
    {
        ControlEnabled = false;

        if (_dataLoaded || _manager == null)
            return;

        ProgressDialogController? controller = null;

        if (showProgress)
            controller = await ShowProgressAsync("Loading", "Please wait while loading the replication information...");

        try
        {
            await _manager.LoadDataAsync();

            if (!_manager.HasReplicatedTables)
            {
                ShowInfo = true;
                return;
            }
            
            // Set the available publications
            Publications = _manager.Publications.ToObservableCollection();
            SelectedPublication = "All";

            _dataLoaded = true;

            ControlEnabled = true;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            if (controller != null)
                await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Filters the result
    /// </summary>
    [RelayCommand]
    private void FilterList()
    {
        if (_manager == null)
            return;

        // Filter the list
        _manager.FilterTables(Filter, SelectedPublication);

        Tables = _manager.Tables.ToObservableCollection();
        HeaderList = Tables.Count > 1
            ? $"{Tables.Count:N0} tables"
            : Tables.Count == 0
                ? "Tables"
                : "1 table";
    }

    /// <summary>
    /// Enriches the selected table type
    /// </summary>
    private async void EnrichData()
    {
        if (_manager?.SelectedTable == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the columns...");

        try
        {
            await _manager.EnrichTableAsync();

            // Set the columns
            SetAdditionalInformation();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Sets the additional information
    /// </summary>
    private void SetAdditionalInformation()
    {
        if (_manager?.SelectedTable == null)
            return;

        Columns = _manager.SelectedTable.Columns.ToObservableCollection();
        HeaderColumns = Columns.Count > 1 ? $"{Columns.Count} columns" : "1 column";

        Indexes = _manager.SelectedTable.Indexes.ToObservableCollection();
        HeaderIndex = Indexes.Count > 1 ? $"{Indexes.Count} indexes" : "1 index";

        RepArticles = _manager.SelectedTable.ReplicationInformation.ToObservableCollection();
        SelectedRepArticle = RepArticles.FirstOrDefault();
    }
}