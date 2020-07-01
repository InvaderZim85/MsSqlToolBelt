using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SettingsWindow"/>
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsWindowViewModel viewModel)
                viewModel.InitViewModel();
        }
    }
}
