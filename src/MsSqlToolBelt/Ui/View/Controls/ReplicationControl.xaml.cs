using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for ReplicationControl.xaml
/// </summary>
public partial class ReplicationControl : UserControl
{
    /// <summary>
    /// Creates a new instance of the <see cref="ReplicationControl"/>
    /// </summary>
    public ReplicationControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the connection
    /// </summary>
    /// <param name="dataSource">The data source</param>
    /// <param name="database">The database</param>
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is ReplicationControlViewModel viewModel)
            viewModel.SetConnection(dataSource, database);
    }

    /// <summary>
    /// Closes the current connection
    /// </summary>
    public void CloseConnection()
    {
        if (DataContext is ReplicationControlViewModel viewModel)
            viewModel.CloseConnection();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    /// <param name="showProgress"><see langword="true"/> to show the progress bar, otherwise <see langword="false"/></param>
    public void LoadData(bool showProgress)
    {
        if (DataContext is ReplicationControlViewModel viewModel)
            viewModel.LoadData(showProgress);
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
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridColumns"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridColumns_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridColumns.CopyToClipboard<ColumnEntry>();
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridTables"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridTables_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridTables.CopyToClipboard<TableEntry>();
    }
}