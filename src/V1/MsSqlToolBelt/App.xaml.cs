using System;
using System.Windows;
using MsSqlToolBelt.View;
using MsSqlToolBelt.ViewModel;
using Serilog;

namespace MsSqlToolBelt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The instance of the main window
        /// </summary>
        private MainWindow _application;

        /// <summary>
        /// Contains the startup time
        /// </summary>
        private DateTime _startUp;

        /// <summary>
        /// Occurs when the app is starting
        /// </summary>
        /// <param name="sender">The application</param>
        /// <param name="e">The event arguments</param>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _startUp = DateTime.Now;
            Helper.InitLogger();

            try
            {
                Log.Information("Start application {name}", Helper.GetFullVersionName());

                _application = new MainWindow(e.Args);
                _application.Show();

                // Set the color theme
                Helper.SetColorTheme();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An error has occurred in the method '{method}'", nameof(App_OnStartup));
            }
        }

        /// <summary>
        /// Occurs when the app is closed
        /// </summary>
        /// <param name="sender">The application</param>
        /// <param name="e">The event arguments</param>
        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (_application.DataContext is MainWindowViewModel viewModel)
            {
                // Stop the timer
                viewModel.StopTimer();

                // Kill the database connection
                viewModel.DisposeConnection();
            }

            // Remove all actions
            Helper.RemoveAllActions();

            var duration = DateTime.Now - _startUp;
            Log.Information("Close application after {duration}", duration);
            Log.CloseAndFlush();
        }
    }
}
