using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for SearchHistoryWindow.xaml
/// </summary>
public partial class SearchHistoryWindow : MetroWindow
{
    /// <summary>
    /// The instance for the interaction with the search history
    /// </summary>
    private readonly SearchHistoryManager _manager;

    /// <summary>
    /// Gets the selected entry
    /// </summary>
    public string SelectedEntry { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a new instance of the <see cref="SearchHistoryWindow"/>
    /// </summary>
    /// <param name="manager">The search history manager</param>
    public SearchHistoryWindow(SearchHistoryManager manager)
    {
        InitializeComponent();

        _manager = manager;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="SearchHistoryWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void SearchHistoryWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SearchHistoryWindowViewModel viewModel)
            return;

        viewModel.InitViewModel(_manager, SetSelectedEntry);
        viewModel.LoadData();
    }

    /// <summary>
    /// Sets the selected entry
    /// </summary>
    /// <param name="entry">The selected entry</param>
    private void SetSelectedEntry(string entry)
    {
        SelectedEntry = entry;
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