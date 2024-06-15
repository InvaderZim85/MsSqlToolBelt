using ICSharpCode.AvalonEdit.Search;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for ClassGenControl.xaml
/// </summary>
public partial class ClassGenControl : UserControl, IUserControl
{
    /// <summary>
    /// Occurs when the user wants to open the selected class in the search
    /// </summary>
    public event EventHandler<string>? OpenInSearch; 

    /// <summary>
    /// Gets or sets the preselection
    /// </summary>
    public string Preselection
    {
        get => DataContext is ClassGenControlViewModel viewModel ? viewModel.Preselection : string.Empty;
        set
        {
            if (DataContext is ClassGenControlViewModel viewModel)
                viewModel.Preselection = value;
        }
    }
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
    /// <param name="manager">The instance of the settings manager</param>
    public void InitControl(SettingsManager manager)
    {
        if (DataContext is ClassGenControlViewModel viewModel)
            viewModel.InitViewModel(manager, SetCode, SetColumnVisibility, RaiseOpenInSearch);

        ClassCodeEditor.InitAvalonEditor(CodeType.CSharp);
        SqlCodeEditor.InitAvalonEditor(CodeType.Sql);
    }

    /// <summary>
    /// Sets the connection
    /// </summary>
    /// <param name="dataSource">The data source</param>
    /// <param name="database">The database</param>
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is not ClassGenControlViewModel viewModel) 
            return;

        viewModel.SetConnection(dataSource, database);
        viewModel.Clear();
    }

    /// <summary>
    /// Closes the current connection
    /// </summary>
    public void CloseConnection()
    {
        if (DataContext is ClassGenControlViewModel viewModel)
            viewModel.CloseConnection();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    /// <param name="showProgress"><see langword="true"/> to show the progress bar, otherwise <see langword="false"/></param>
    public async void LoadData(bool showProgress)
    {
        if (DataContext is ClassGenControlViewModel viewModel)
            await viewModel.LoadDataAsync(showProgress);

        // Scroll to the preselection (if it's not empty)
        if (string.IsNullOrEmpty(Preselection))
            return;

        if (DataGridTables.SelectedItem != null)
            DataGridTables.ScrollIntoView(DataGridTables.SelectedItem);
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
    /// Sets the visibility of some columns
    /// </summary>
    /// <param name="visible">true to show the column, otherwise false</param>
    private void SetColumnVisibility(bool visible)
    {
        ColumnAlias.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Raises the <see cref="OpenInSearch"/> event
    /// </summary>
    /// <param name="value">The value which should be searched for</param>
    private void RaiseOpenInSearch(string value)
    {
        OpenInSearch?.Invoke(this, value);
    }

    /// <summary>
    /// Occurs when the user hits the link
    /// </summary>
    /// <param name="sender">The hyperlink</param>
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