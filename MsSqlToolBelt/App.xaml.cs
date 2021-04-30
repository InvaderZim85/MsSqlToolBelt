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
        /// THe instance of the main window
        /// </summary>
        private MainWindow _application;

        /// <summary>
        /// Occurs when the app is starting
        /// </summary>
        /// <param name="sender">The application</param>
        /// <param name="e">The event arguments</param>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Helper.InitLogger();

            try
            {
                Log.Information("Start application {name}", Helper.GetFullVersionName());

                _application = new MainWindow(e.Args);
                _application.Show();
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
                viewModel.StopTimer();

            Log.Information("Close application.");
            Log.CloseAndFlush();
        }
    }
}
