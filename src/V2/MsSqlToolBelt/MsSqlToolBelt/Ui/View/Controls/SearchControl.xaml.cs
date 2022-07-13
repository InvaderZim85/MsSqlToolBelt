using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Search;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;

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
}