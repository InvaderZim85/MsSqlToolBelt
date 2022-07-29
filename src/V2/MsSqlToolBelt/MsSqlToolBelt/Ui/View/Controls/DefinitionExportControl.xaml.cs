using System.Windows.Controls;
using System.Windows.Input;
using MsSqlToolBelt.DataObjects.DefinitionExport;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for DefinitionExportControl.xaml
/// </summary>
public partial class DefinitionExportControl : UserControl, IConnection
{
    /// <summary>
    /// Creates a new instance of the <see cref="DefinitionExportControl"/>
    /// </summary>
    public DefinitionExportControl()
    {
        InitializeComponent();
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
}