using System.Windows.Controls;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for SettingsControl.xaml
/// </summary>
public partial class SettingsControl : UserControl
{
    /// <summary>
    /// Creates a new instance of the <see cref="SettingsControl"/>
    /// </summary>
    public SettingsControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Init the control
    /// </summary>
    /// <param name="settingsManager">The instance for the interaction with the settings</param>
    public void InitControl(SettingsManager settingsManager)
    {
        if (DataContext is SettingsControlViewModel viewModel)
            viewModel.InitViewModel(settingsManager);
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridServer"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridServer_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridServer.CopyToClipboard<ServerEntry>();
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridIgnore"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridIgnore_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridIgnore.CopyToClipboard<FilterEntry>();
    }
}