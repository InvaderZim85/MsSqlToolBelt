using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for EditServerWindow.xaml
/// </summary>
public partial class EditServerWindow : MetroWindow
{
    /// <summary>
    /// Contains the selected server
    /// </summary>
    private readonly string _selectedServer;

    /// <summary>
    /// Gets the selected server
    /// </summary>
    public string SelectedServer =>
        DataContext is EditServerWindowViewModel viewModel ? viewModel.Server ?? string.Empty : string.Empty;

    /// <summary>
    /// Gets the selected database
    /// </summary>
    public string SelectedDatabase
    {
        get
        {
            if (DataContext is not EditServerWindowViewModel viewModel)
                return string.Empty;

            var selectedDatabase = viewModel.SelectedDatabase ?? string.Empty;
            // The arrow is a indicator for an "empty" selection
            return selectedDatabase.StartsWith("<") ? string.Empty : selectedDatabase;
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="EditServerWindow"/>
    /// </summary>
    /// <param name="selectedServer">The selected server</param>
    public EditServerWindow(string selectedServer = "")
    {
        InitializeComponent();

        _selectedServer = selectedServer;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The window</param>
    /// <param name="e">The event arguments</param>
    private void EditServerWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditServerWindowViewModel viewModel)
            viewModel.InitViewModel(_selectedServer);
    }

    /// <summary>
    /// Occurs when the user hits the select button
    /// </summary>
    /// <param name="sender">The select button</param>
    /// <param name="e">The event arguments</param>
    private void ButtonSelect_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SelectedServer))
            DialogResult = false;

        DialogResult = true;
    }

    /// <summary>
    /// Occurs when the user hits the close button
    /// </summary>
    /// <param name="sender">The close button</param>
    /// <param name="e">The event arguments</param>
    private void ButtonClose_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}