using System;
using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Windows;
using Serilog;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    /// <summary>
    /// The instance of the settings manager
    /// </summary>
    private readonly SettingsManager _settingsManager;

    /// <summary>
    /// Creates a new instance of the <see cref="MainWindow"/>
    /// </summary>
    /// <param name="settingsManager">The instance of the settings manager</param>
    public MainWindow(SettingsManager settingsManager)
    {
        InitializeComponent();

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
                SettingsControl.InitControl();
                break;
            case FlyOutType.Info:
                InfoControl.InitControl();
                break;
        }
    }

    /// <summary>
    /// Sets the connection
    /// </summary>
    /// <param name="dataSource">The name / path of the server</param>
    /// <param name="database">The name of the database</param>
    private void SetConnection(string dataSource, string database)
    {
        SearchControl.SetConnection(dataSource, database);
        TableTypesControl.SetConnection(dataSource, database);
        ReplicationControl.SetConnection(dataSource, database);
        ClassGenControl.SetConnection(dataSource, database);
        DefinitionExportControl.SetConnection(dataSource, database);
    }

    /// <summary>
    /// Closes the connection of all sub controls
    /// </summary>
    public void CloseConnection()
    {
        SearchControl.CloseConnection();
        TableTypesControl.CloseConnection();
        ReplicationControl.CloseConnection();
        ClassGenControl.CloseConnection();
        DefinitionExportControl.CloseConnection();
    }

    /// <summary>
    /// Loads the data of the selected tab
    /// </summary>
    /// <param name="tabIndex">The tab index</param>
    private void LoadData(int tabIndex)
    {
        switch (tabIndex)
        {
            case 1: // Table types
                TableTypesControl.LoadData();
                break;
            case 2: // Replication control
                ReplicationControl.LoadData();
                break;
            case 3: // Class generator
                ClassGenControl.LoadData();
                break;
            case 4: // Definition export
                DefinitionExportControl.LoadData();
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
            var scheme = await _settingsManager.LoadSettingsValueAsync(SettingsKey.ColorScheme, "Emerald");
            Helper.SetColorTheme(scheme);
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
        SearchControl.InitControl(_settingsManager);
        ClassGenControl.InitControl();
    }
}