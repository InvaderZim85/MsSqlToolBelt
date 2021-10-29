using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for TableIndexWindow.xaml
    /// </summary>
    public partial class TableIndexWindow : MetroWindow
    {
        /// <summary>
        /// Contains the list with the indices
        /// </summary>
        private readonly List<TableIndex> _tableIndices;

        /// <summary>
        /// Creates a new instance of the <see cref="TableIndexWindow"/>
        /// </summary>
        /// <param name="tableIndices">The list with the table indices</param>
        public TableIndexWindow(List<TableIndex> tableIndices)
        {
            InitializeComponent();

            _tableIndices = tableIndices;
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        /// <param name="sender">The <see cref="TableIndexWindow"/></param>
        /// <param name="e">The event arguments</param>
        private void TableIndexWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TableIndexWindowViewModel viewModel)
                return;

            viewModel.InitViewModel(_tableIndices);
        }
    }
}
