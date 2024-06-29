using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;
using System.Windows.Controls;
using System.Windows.Input;
using MsSqlToolBelt.DataObjects.Table;

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
    /// Init the control
    /// </summary>
    public void InitControl()
    {
        if (DataContext is ReplicationControlViewModel viewModel)
            viewModel.InitViewModel(SetSqlText);

        SqlEditor.InitAvalonEditor(CodeType.Sql);
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
    /// Sets the text of the sql editor
    /// </summary>
    /// <param name="text">The text which should be set</param>
    private void SetSqlText(string text)
    {
        SqlEditor.Text = text;
        SqlEditor.ScrollToHome();
    }

    /// <summary>
    /// Occurs when the user hits CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridIndexes"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridIndexes_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridIndexes.CopyToClipboard<IndexEntry>();
    }

    /// <summary>
    /// Occurs when the user hits CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridColumns"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridColumns_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridColumns.CopyToClipboard<ColumnEntry>();
    }

    /// <summary>
    /// Occurs when the user hits CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridTables"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridTables_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridTables.CopyToClipboard<TableEntry>();
    }

    /// <summary>
    /// Occurs when the user hits CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridRepArticles"/></param>
    /// <param name="e">The event arguments</param>
    private void DateGridRepArticles_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridRepArticles.CopyToClipboard<ReplicationArticle>();
    }
}