using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for EditServerWindow.xaml
/// </summary>
public partial class EditServerWindow : MetroWindow
{
    /// <summary>
    /// Gets or sets the selected server
    /// </summary>
    public ServerEntry SelectedServer
    {
        get => DataContext is EditServerWindowViewModel viewModel ? viewModel.SelectedServer : new ServerEntry();
        set
        {
            if (DataContext is EditServerWindowViewModel viewModel)
                viewModel.SelectedServer = value;
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="EditServerWindow"/>
    /// </summary>
    public EditServerWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the dialog result and closed the window
    /// </summary>
    /// <param name="dialogResult">The desired result</param>
    private void CloseWindow(bool dialogResult)
    {
        DialogResult = dialogResult;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The window</param>
    /// <param name="e">The event arguments</param>
    private void EditServerWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditServerWindowViewModel viewModel)
            viewModel.InitViewModel(CloseWindow);
    }
}