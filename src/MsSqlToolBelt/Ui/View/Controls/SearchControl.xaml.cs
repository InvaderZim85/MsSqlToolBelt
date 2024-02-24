using ICSharpCode.AvalonEdit.Search;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for SearchControl.xaml
/// </summary>
public partial class SearchControl : UserControl, IUserControl
{
    /// <summary>
    /// Occurs when the user wants to open the current search result in the class generator
    /// </summary>
    public event EventHandler<SearchResult>? OpenInClassGenerator;

    /// <summary>
    /// Creates a new instance of the <see cref="SearchControl"/>
    /// </summary>
    public SearchControl()
    {
        InitializeComponent();
        SearchPanel.Install(SqlEditor);
    }

    /// <inheritdoc />
    public void InitControl(SettingsManager manager)
    {
        if (DataContext is SearchControlViewModel viewModel)
            viewModel.InitViewModel(manager, SetSqlText, SetCmdText, SetTableDefinitionText, RaiseOpenInClassGenerator);

        SqlEditor.InitAvalonEditor(CodeType.Sql);
        CmdEditor.InitAvalonEditor(CodeType.Sql);
        TableDefinitionEditor.InitAvalonEditor(CodeType.Sql);
    }

    /// <summary>
    /// Sets the connection
    /// </summary>
    /// <param name="dataSource">The data source</param>
    /// <param name="database">The database</param>
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is SearchControlViewModel viewModel)
            viewModel.SetConnection(dataSource, database);
    }

    /// <summary>
    /// Closes the connection
    /// </summary>
    public void CloseConnection()
    {
        if (DataContext is SearchControlViewModel viewModel)
            viewModel.CloseConnection();
    }

    /// <summary>
    /// Executes the search with the given value
    /// </summary>
    /// <param name="value">The value to search for</param>
    /// <returns>The awaitable task</returns>
    public Task ExecuteSearchAsync(string value)
    {
        if (DataContext is SearchControlViewModel viewModel)
            return viewModel.ExecuteSearchAsync(value);
        
        return Task.CompletedTask;
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
    /// Sets the command text
    /// </summary>
    /// <param name="text">The text which should be set</param>
    private void SetCmdText(string text)
    {
        CmdEditor.Text = text;
        CmdEditor.ScrollToHome();
    }

    /// <summary>
    /// Sets the table definition
    /// </summary>
    /// <param name="text">The text which should be set</param>
    private void SetTableDefinitionText(string text)
    {
        TableDefinitionEditor.Text = text;
        TableDefinitionEditor.ScrollToHome();
    }

    /// <summary>
    /// Raises the <see cref="OpenInClassGenerator"/> event
    /// </summary>
    /// <param name="result">The search result</param>
    private void RaiseOpenInClassGenerator(SearchResult result)
    {
        OpenInClassGenerator?.Invoke(this, result);
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridTable"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridTable_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridTable.CopyToClipboard<ColumnEntry>();
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridTable"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridJob_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridJob.CopyToClipboard<JobStepEntry, int>(entry => entry.Id);
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridResult"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridResult_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridResult.CopyToClipboard<SearchResult>();
    }
}