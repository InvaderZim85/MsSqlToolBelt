using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects.TableType;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for ExportTableTypeDialog.xaml
    /// </summary>
    public partial class ExportTableTypeDialog : MetroWindow
    {
        /// <summary>
        /// Contains the list with the types
        /// </summary>
        private readonly List<TableType> _types;

        /// <summary>
        /// Creates a new instance of the <see cref="ExportTableTypeDialog"/>
        /// </summary>
        /// <param name="types"></param>
        public ExportTableTypeDialog(List<TableType> types)
        {
            InitializeComponent();

            _types = types;
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportTableTypeDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExportTableTypeViewModel viewModel)
                viewModel.InitViewModel(_types);
        }

        /// <summary>
        /// Occurs when the user hits the close button
        /// </summary>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
