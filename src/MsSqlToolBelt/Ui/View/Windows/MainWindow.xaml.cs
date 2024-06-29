using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Ui.View.Controls;
using MsSqlToolBelt.Ui.ViewModel.Windows;
using Serilog;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    /// <summary>
    /// The start time of the application
    /// </summary>
    private readonly DateTime _startTime;

    /// <summary>
    /// The instance of the settings manager
    /// </summary>
    private readonly SettingsManager _settingsManager;

    /// <summary>
    /// Creates a new instance of the <see cref="MainWindow"/>
    /// </summary>
    /// <param name="startTime">The start time of the application</param>
    /// <param name="settingsManager">The instance of the settings manager</param>
    public MainWindow(DateTime startTime, SettingsManager settingsManager)
    {
        InitializeComponent();

        _startTime = startTime;
        _settingsManager = settingsManager;
    }

    /// <summary>
    /// Init the fly out
    /// </summary>
    /// <param name="type">The type of the fly out</param>
    private void InitFlyOut(FlyOutType type)
    {
        switch (type)
        {
            case FlyOutType.Settings:
                ControlSettings.InitControl(_settingsManager);
                break;
            case FlyOutType.Info:
                ControlInfo.InitControl(_startTime, _settingsManager);
                break;
        }
    }

    /// <summary>
    /// Sets the connection
    /// </summary>
    /// <param name="dataSource">The name / path of the server</param>
    /// <param name="database">The name of the database</param>
    /// <param name="firstConnection"><see langword="true"/> when the first connection is being established, <see langword="false"/> if it's a reconnect</param>
    private void SetConnection(string dataSource, string database, bool firstConnection)
    {
        ControlSearch.SetConnection(dataSource, database);
        ControlTableTypes.SetConnection(dataSource, database);
        ControlReplication.SetConnection(dataSource, database);
        ControlClassGen.SetConnection(dataSource, database);
        ControlDefinitionExport.SetConnection(dataSource, database);

        // Reload the data
        LoadData(ControlTabs.SelectedIndex, firstConnection);
    }

    /// <summary>
    /// Closes the connection of all sub controls
    /// </summary>
    public void CloseConnection()
    {
        ControlSearch.CloseConnection();
        ControlTableTypes.CloseConnection();
        ControlReplication.CloseConnection();
        ControlClassGen.CloseConnection();
        ControlDefinitionExport.CloseConnection();
    }

    /// <summary>
    /// Loads the data of the selected tab
    /// </summary>
    /// <param name="tabIndex">The tab index</param>
    /// <param name="showProgress"><see langword="true"/> to show the progress dialog of the control, otherwise <see langword="false"/></param>
    private void LoadData(int tabIndex, bool showProgress)
    {
        switch (tabIndex)
        {
            case 1: // Table types
                ControlTableTypes.LoadData(showProgress);
                break;
            case 2: // Class generator
                ControlClassGen.LoadData(showProgress);
                break;
            case 3: // Definition export
                ControlDefinitionExport.LoadData(showProgress);
                break;
            case 4: // Replication control
                ControlReplication.LoadData(showProgress);
                break;
        }
    }

    /// <summary>
    /// Occurs when the fly out was closed
    /// </summary>
    /// <param name="sender">The settings fly out</param>
    /// <param name="e">The event arguments</param>
    private async void Flyout_OnClosingFinished(object sender, RoutedEventArgs e)
    {
        try
        {
            var scheme = await SettingsManager.LoadSettingsValueAsync(SettingsKey.ColorScheme, DefaultEntries.ColorScheme);
            Helper.SetColorTheme(scheme);

            // Stops the info timer (only if needed)
            ControlInfo.StopUpTimeTimer();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Can't load color scheme.");
        }
    }

    /// <summary>
    /// Occurs when the main window was loaded
    /// </summary>
    /// <param name="sender">The main window</param>
    /// <param name="e">The event arguments</param>
    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.InitViewModel(_settingsManager, InitFlyOut, SetConnection, LoadData);
            viewModel.LoadData();
        }

        // Init the other controls
        ControlSearch.InitControl(_settingsManager);
        ControlClassGen.InitControl(_settingsManager);
        ControlDefinitionExport.InitControl(_settingsManager);
        ControlTableTypes.InitControl();
        ControlReplication.InitControl();

        // Set the event
        ControlSearch.OpenInClassGenerator += (_, searchResult) =>
        {
            // Set the preselection
            ControlClassGen.Preselection = searchResult.Name;

            // Switch to the class generator
            ControlTabs.SelectedIndex = 2;
        };

        ControlClassGen.OpenInSearch += OnOpenInSearch;
        ControlTableTypes.OpenInSearch += OnOpenInSearch;
    }

    /// <summary>
    /// Opens the desired value in the search
    /// </summary>
    /// <param name="sender">The <see cref="ClassGenControl"/> or <see cref="TableTypesControl"/></param>
    /// <param name="e">The string to search for</param>
    private async void OnOpenInSearch(object? sender, string e)
    {
        // Switch to the search
        ControlTabs.SelectedIndex = 0;

        // Execute the search
        await ControlSearch.ExecuteSearchAsync(e);
    }

    /// <summary>
    /// Occurs when the user double-clicks the build info
    /// </summary>
    /// <param name="sender">The build info (on the bottom right)</param>
    /// <param name="e">The event arguments</param>
    private void BuildInfo_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        InfoBorder.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Occurs when the user hits the close icon with the left button
    /// </summary>
    /// <param name="sender">The <see cref="CloseIcon"/></param>
    /// <param name="e">The event arguments</param>
    private void CloseIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        InfoBorder.Visibility = Visibility.Hidden;
    }

    /// <summary>
    /// Occurs when the user enters the area of the close icon
    /// </summary>
    /// <param name="sender">The <see cref="CloseIcon"/></param>
    /// <param name="e">The event arguments</param>
    private void CloseIcon_MouseEnter(object sender, MouseEventArgs e)
    {
        CloseIcon.Foreground = new SolidColorBrush(Colors.Red);
        CloseIcon.Kind = PackIconMaterialKind.CloseBox;
    }

    /// <summary>
    /// Occurs when the user leaves the area of the close icon
    /// </summary>
    /// <param name="sender">The <see cref="CloseIcon"/></param>
    /// <param name="e">The event arguments</param>
    private void CloseIcon_MouseLeave(object sender, MouseEventArgs e)
    {
        CloseIcon.Foreground = new SolidColorBrush(Colors.DarkRed);
        CloseIcon.Kind = PackIconMaterialKind.CloseBoxOutline;
    }

    /// <summary>
    /// Occurs when the user clicks the hyperlink of the project file
    /// </summary>
    /// <param name="sender">The project link"/></param>
    /// <param name="e">The event arguments</param>
    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Helper.OpenLink(e.Uri.ToString());
    }

    /// <summary>
    /// Occurs when the user hits the close menu (program > close)
    /// </summary>
    /// <param name="sender">The close menu item</param>
    /// <param name="e">The event arguments</param>
    private void MainMenuClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}