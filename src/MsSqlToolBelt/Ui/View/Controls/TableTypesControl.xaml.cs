using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.TableType;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for TableTypesControl.xaml
/// </summary>
public partial class TableTypesControl : UserControl, IConnection
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableTypesControl"/>
    /// </summary>
    public TableTypesControl()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is TableTypesControlViewModel viewModel)
            viewModel.SetConnection(dataSource, database);
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        if (DataContext is TableTypesControlViewModel viewModel)
            viewModel.CloseConnection();
    }

    /// <inheritdoc />
    public void LoadData(bool showProgress)
    {
        if (DataContext is TableTypesControlViewModel viewModel)
            viewModel.LoadData(showProgress);
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridTypes"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridTypes_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridTypes.CopyToClipboard<TableTypeEntry>();
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
}