using MahApps.Metro.Controls;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Windows;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for TableIndexWindow.xaml
/// </summary>
public partial class TableIndexWindow : MetroWindow
{
    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private readonly TableManager _tableManager;

    /// <summary>
    /// The table which should be shown
    /// </summary>
    private readonly TableEntry _table;

    /// <summary>
    /// Creates a new instance of the <see cref="TableIndexWindow"/>
    /// </summary>
    /// <param name="tableManager">The instance for the interaction with the tables</param>
    /// <param name="table">The selected table</param>
    public TableIndexWindow(TableManager tableManager, TableEntry table)
    {
        InitializeComponent();
        _tableManager = tableManager;
        _table = table;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="TableIndexWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void TableIndexWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is TableIndexWindowViewModel viewModel)
            viewModel.InitViewModel(_tableManager, _table);
    }

    /// <summary>
    /// Occurs when the user hits the close button
    /// </summary>
    /// <param name="sender">The close button</param>
    /// <param name="e">The event arguments</param>
    private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridIndexes"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridIndexes_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridIndexes.CopyToClipboard<IndexEntry>();
    }

    /// <summary>
    /// Occurs when the user clicks the hyperlink of the project file
    /// </summary>
    /// <param name="sender">The project link"/></param>
    /// <param name="e">The event arguments</param>
    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Helper.OpenLink(e.Uri.ToString());
    }
}