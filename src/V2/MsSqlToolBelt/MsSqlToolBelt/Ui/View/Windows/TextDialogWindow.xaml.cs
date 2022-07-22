using System;
using System.Windows;
using ICSharpCode.AvalonEdit.Search;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for TextDialogWindow.xaml
/// </summary>
public partial class TextDialogWindow : MetroWindow
{
    /// <summary>
    /// Contains the settings of the dialog
    /// </summary>
    private readonly TextDialogSettings _settings;

    /// <summary>
    /// Gets the inserted code (when the code is not valid, this value will be an empty string)
    /// </summary>
    public string Code =>
        DataContext is TextDialogWindowViewModel { CodeValid: true } ? CodeEditor.Text : "";

    /// <summary>
    /// Creates a new instance of the <see cref="TextDialogWindow"/>
    /// </summary>
    /// <param name="settings">The settings</param>
    public TextDialogWindow(TextDialogSettings settings)
    {
        InitializeComponent();

        _settings = settings;

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
    /// Occurs when the user changes the code
    /// </summary>
    /// <param name="sender">The <see cref="CodeEditor"/></param>
    /// <param name="e">The event arguments</param>
    private void CodeEditor_OnTextChanged(object? sender, EventArgs e)
    {
        if (DataContext is TextDialogWindowViewModel viewModel)
            viewModel.CodeValid = false;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="TextDialogWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void TextDialogWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        CodeEditor.InitAvalonEditor(_settings.CodeType);

        if (DataContext is TextDialogWindowViewModel viewModel)
            viewModel.InitViewModel(_settings, SetEditorText, GetEditorText, Close);
    }
}