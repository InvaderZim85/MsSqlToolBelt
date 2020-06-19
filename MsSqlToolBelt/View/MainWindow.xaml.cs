using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.ViewModel;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MainWindow"/>
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the connector of the search and table type control
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        private void SetConnector(Connector connector)
        {
            _searchControl.SetConnector(connector);
            _tableTypeControl.SetConnector(connector);
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.InitViewModel(SetConnector);

            _searchControl.InitControl();
        }
    }
}
