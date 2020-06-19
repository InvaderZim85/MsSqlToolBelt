using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : MetroWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="InfoWindow"/>
        /// </summary>
        public InfoWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        private void InfoWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InfoWindowViewModel viewModel)
                viewModel.Load();
        }

        /// <summary>
        /// Occurs when the user hits the link
        /// </summary>
        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }
}
