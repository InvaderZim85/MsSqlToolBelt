﻿using System.Windows.Controls;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.DefinitionExport;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for DefinitionExportControl.xaml
/// </summary>
public partial class DefinitionExportControl : UserControl, IUserControl
{
    /// <summary>
    /// Creates a new instance of the <see cref="DefinitionExportControl"/>
    /// </summary>
    public DefinitionExportControl()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    public void InitControl(SettingsManager manager)
    {
        if (DataContext is DefinitionExportControlViewModel viewModel)
            viewModel.InitViewModel(manager, GetText, SetText);

        CodeEditor.InitAvalonEditor(CodeType.None);
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is DefinitionExportControlViewModel viewModel)
            viewModel.SetConnection(dataSource, database);
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        if (DataContext is DefinitionExportControlViewModel viewModel)
            viewModel.CloseConnection();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    public void LoadData()
    {
        if (DataContext is DefinitionExportControlViewModel viewModel)
            viewModel.LoadData();
    }

    /// <summary>
    /// Gets the text of the editor
    /// </summary>
    /// <returns>The text</returns>
    private string GetText()
    {
        return CodeEditor.Text;
    }

    /// <summary>
    /// Sets the text of the editor
    /// </summary>
    /// <param name="text">The text</param>
    private void SetText(string text)
    {
        CodeEditor.Text = text;
    }

    /// <summary>
    /// Occurs when the text of the info text box was changed
    /// </summary>
    /// <param name="sender">The <see cref="TextBoxInfo"/></param>
    /// <param name="e">The event arguments</param>
    private void TextBoxInfo_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBoxInfo.ScrollToEnd();
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridObject"/></param>
    /// <param name="e">The event arguments</param>
    private void ObjectGrid_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridObject.CopyToClipboard<ObjectDto>();
    }

    /// <summary>
    /// Occurs when the text of the code editor was changed
    /// </summary>
    /// <param name="sender">The <see cref="CodeEditor"/></param>
    /// <param name="e">The event arguments</param>
    private void CodeEditor_OnTextChanged(object sender, System.EventArgs e)
    {
        CodeEditor.ScrollToEnd();
    }
}