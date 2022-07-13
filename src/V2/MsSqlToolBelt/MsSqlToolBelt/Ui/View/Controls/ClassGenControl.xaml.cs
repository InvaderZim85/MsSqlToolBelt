using System.Windows.Controls;
using System.Windows.Navigation;
using ICSharpCode.AvalonEdit.Search;
using MsSqlToolBelt.Common.Enums;
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
        SearchPanel.Install(CodeEditor);
    }

    /// <summary>
    /// Init the control
    /// </summary>
    public void InitControl()
    {
        if (DataContext is ClassGenControlViewModel viewModel)
            viewModel.InitViewModel(SetCode);

        CodeEditor.InitAvalonEditor(CodeType.CSharp);
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is ClassGenControlViewModel viewModel)
            viewModel.SetConnection(dataSource, database);
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
    /// Sets the text of the code editor
    /// </summary>
    /// <param name="code">The code which should be set</param>
    private void SetCode(string code)
    {
        CodeEditor.Text = code;
        CodeEditor.ScrollToHome();
    }

    /// <summary>
    /// Occurs when the user hits the link
    /// </summary>
    /// <param name="sender">The hyper link</param>
    /// <param name="e">The event arguments</param>
    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        // TODO
    }
}