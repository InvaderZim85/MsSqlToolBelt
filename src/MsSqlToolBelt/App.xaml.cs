using Microsoft.Data.Sqlite;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Ui.View.Windows;
using MsSqlToolBelt.Ui.ViewModel.Windows;
using Serilog;
using System.Text;
using System.Windows;

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
    /// The startup date
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

        try
        {
            Helper.InitHelper();

            _settingsManager ??= new SettingsManager();

            _mainWindow = new MainWindow(_startUp, _settingsManager);
            _mainWindow.Show();

            // Set the color theme
            Helper.SetColorTheme(
                await SettingsManager.LoadSettingsValueAsync(SettingsKey.ColorScheme, DefaultEntries.ColorScheme));

            Log.Information("Application startet.");
        }
        catch (SqliteException ex)
        {
            Log.Fatal(ex, "An fatal error has occurred.");

            var message = new StringBuilder()
                .AppendLine("An error has occurred within the settings database.")
                .AppendLine()
                .AppendLine("You can fix the error by removing the 'MsSqlToolBelt.Settings.db' file, which is located in the same folder as the application.")
                .AppendLine()
                .AppendLine("Note: Before you remove / delete the file, create a backup!");

            MessageBox.Show(message.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        Mediator.RemoveAll();

        if (_mainWindow?.DataContext is MainWindowViewModel viewModel)
            viewModel.CloseViewModel();

        _mainWindow?.CloseConnection();

        var duration = DateTime.Now - _startUp;

        SaveUpTime(duration);
        
        Log.Information("Close application after {duration}", duration);
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Saves the up time of the application
    /// </summary>
    /// <param name="duration">The duration</param>
    private async void SaveUpTime(TimeSpan duration)
    {
        if (_settingsManager == null)
            return;

        try
        {
            var upTime = await SettingsManager.LoadSettingsValueAsync(SettingsKey.UpTime, 0L);

            upTime += duration.Ticks;

            await SettingsManager.SaveSettingsValueAsync(SettingsKey.UpTime, upTime);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "An error has occurred while saving the up time.");
        }
    }
}