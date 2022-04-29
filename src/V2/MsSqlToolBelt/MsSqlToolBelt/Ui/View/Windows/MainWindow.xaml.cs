using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
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
                //case FlyOutType.DataTypes:
                //    DataTypeControl.InitControl();
                //    break;
                //case FlyOutType.Info:
                //    InfoControl.InitControl();
                //    break;
            }
        }

        /// <summary>
        /// Occurs when the fly out was closed
        /// </summary>
        /// <param name="sender">The settings fly out</param>
        /// <param name="e">The event arguments</param>
        private void Flyout_OnClosingFinished(object sender, RoutedEventArgs e)
        {
            // TODO: Set the new color theme
        }

        /// <summary>
        /// Occurs when the main window was loaded
        /// </summary>
        /// <param name="sender">The main window</param>
        /// <param name="e">The event arguments</param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.InitViewModel(InitFlyOut);
        }
    }
}
