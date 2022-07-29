using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using ICSharpCode.AvalonEdit.Search;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for ClassGenControl.xaml
/// </summary>
public partial class ClassGenControl : UserControl, IConnection
{
    /// <summary>
    /// Creates a new instance of the <see cref="ClassGenControl"/>
    /// </summary>
    public ClassGenControl()
    {
        InitializeComponent();
        SearchPanel.Install(ClassCodeEditor);
        SearchPanel.Install(SqlCodeEditor);
    }

    /// <summary>
    /// Init the control
    /// </summary>
    public void InitControl()
    {
        if (DataContext is ClassGenControlViewModel viewModel)
            viewModel.InitViewModel(SetCode);

        ClassCodeEditor.InitAvalonEditor(CodeType.CSharp);
        SqlCodeEditor.InitAvalonEditor(CodeType.Sql);
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is not ClassGenControlViewModel viewModel) 
            return;

        viewModel.SetConnection(dataSource, database);
        viewModel.Clear();
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        if (DataContext is ClassGenControlViewModel viewModel)
            viewModel.CloseConnection();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    public void LoadData()
    {
        if (DataContext is ClassGenControlViewModel viewModel)
            viewModel.LoadData();
    }

    /// <summary>
    /// Sets the code of the CSharp class and the SQL query
    /// </summary>
    /// <param name="result">The result of the class generator</param>
    private void SetCode(ClassGenResult result)
    {
        ClassCodeEditor.Text = result.ClassCode;
        ClassCodeEditor.ScrollToHome();

        SqlCodeEditor.Text = result.SqlQuery;
        SqlCodeEditor.ScrollToHome();
    }

    /// <summary>
    /// Occurs when the user hits the link
    /// </summary>
    /// <param name="sender">The hyper link</param>
    /// <param name="e">The event arguments</param>
    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Helper.OpenLink(e.Uri.ToString());
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridTables"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridTables_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridTables.CopyToClipboard<TableDto>();
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridColumns"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridColumns_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridColumns.CopyToClipboard<ColumnDto>();
    }
}