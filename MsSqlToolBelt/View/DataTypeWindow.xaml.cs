using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for DataTypeWindow.xaml
    /// </summary>
    public partial class DataTypeWindow : MetroWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DataTypeWindow"/>
        /// </summary>
        public DataTypeWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        /// <param name="sender">The <see cref="DataTypeWindow"/></param>
        /// <param name="e">The event arguments</param>
        private void DataTypeWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DataTypeWindowViewModel viewModel)
                viewModel.InitViewModel();
        }

        /// <summary>
        /// Occurs when the user hits the close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
