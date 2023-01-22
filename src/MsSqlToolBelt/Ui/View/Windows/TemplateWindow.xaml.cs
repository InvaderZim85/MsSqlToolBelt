using System.Windows;
using ICSharpCode.AvalonEdit.Search;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for TemplateWindow.xaml
/// </summary>
public partial class TemplateWindow : MetroWindow
{
    /// <summary>
    /// Creates a new instance of the <see cref="TemplateWindow"/>
    /// </summary>
    public TemplateWindow()
    {
        InitializeComponent();

        SearchPanel.Install(CodeEditor);
    }

    /// <summary>
    /// Sets the text of the editor
    /// </summary>
    /// <param name="text">The text which should be set</param>
    private void SetEditorText(string text)
    {
        CodeEditor.Text = text;
        CodeEditor.ScrollToHome();
    }

    /// <summary>
    /// Gets the text of the editor
    /// </summary>
    /// <returns>The text</returns>
    private string GetEditorText()
    {
        return CodeEditor.Text;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="TemplateWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void TemplateWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        CodeEditor.InitAvalonEditor(CodeType.CSharp);

        if (DataContext is TemplateWindowViewModel viewModel)
            viewModel.InitViewModel(SetEditorText, GetEditorText);
    }

    /// <summary>
    /// Occurs when the user hits the close button
    /// </summary>
    /// <param name="sender">The close button</param>
    /// <param name="e">The event arguments</param>
    private void ButtonClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}