using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Search;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for SearchControl.xaml
/// </summary>
public partial class SearchControl : UserControl, IUserControl
{
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
            viewModel.InitViewModel(manager, SetSqlText, SetCmdText);

        SqlEditor.InitAvalonEditor(CodeType.Sql);
        CmdEditor.InitAvalonEditor(CodeType.Sql);
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is SearchControlViewModel viewModel)
            viewModel.SetConnection(dataSource, database);
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        if (DataContext is SearchControlViewModel viewModel)
            viewModel.CloseConnection();
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
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridTable"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridTable_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var items = new List<ColumnEntry>();

        foreach (var item in DataGridTable.SelectedItems)
        {
            if (item is not ColumnEntry entry)
                continue;

            items.Add(entry);
        }

        if (!items.Any())
            return;

        Clipboard.SetText(items.CreateTable(OutputType.Csv));
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridTable"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridJob_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var items = new List<JobStepEntry>();

        foreach (var item in DataGridJob.SelectedItems)
        {
            if (item is not JobStepEntry entry)
                continue;

            items.Add(entry);
        }

        if (!items.Any())
            return;

        Clipboard.SetText(items.OrderBy(o => o.Id).CreateTable(OutputType.Csv));
    }
}