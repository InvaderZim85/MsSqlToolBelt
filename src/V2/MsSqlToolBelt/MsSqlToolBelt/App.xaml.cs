using System;
using System.Windows;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Ui.View.Windows;
using MsSqlToolBelt.Ui.ViewModel.Windows;
using Log = Serilog.Log;

namespace MsSqlToolBelt;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// The instance of the main window
    /// </summary>
    private MainWindow? _mainWindow;

    /// <summary>
    /// The instance of the settings manager
    /// </summary>
    private SettingsManager? _settingsManager;

    /// <summary>
    /// The start up date
    /// </summary>
    private DateTime _startUp = DateTime.Now;

    /// <summary>
    /// Occurs when the application is starting
    /// </summary>
    /// <param name="sender">The application</param>
    /// <param name="e">The event arguments</param>
    private async void App_OnStartup(object sender, StartupEventArgs e)
    {
        _startUp = DateTime.Now;
        Helper.InitHelper();

        try
        {
            _settingsManager ??= new SettingsManager();

            _mainWindow = new MainWindow(_settingsManager);
            _mainWindow.Show();

            // Set the color theme
            Helper.SetColorTheme(await _settingsManager.LoadSettingsValueAsync(SettingsKey.ColorScheme, "Emerald"));
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An fatal error has occurred.");
            MessageBox.Show("A fatal error has occurred. The program will be closed.", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Occurs when the application is closing
    /// </summary>
    /// <param name="sender">The application</param>
    /// <param name="e">The event arguments</param>
    private void App_OnExit(object sender, ExitEventArgs e)
    {
        Mediator.RemoveAllActions();

        if (_mainWindow?.DataContext is MainWindowViewModel viewModel)
            viewModel.CloseViewModel();

        _mainWindow?.CloseConnection();

        var duration = DateTime.Now - _startUp;
        Log.Information("Close application after {duration}", duration);
        Log.CloseAndFlush();
    }
}