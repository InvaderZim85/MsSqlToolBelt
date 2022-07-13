using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for TableIndexWindow.xaml
/// </summary>
public partial class TableIndexWindow : MetroWindow
{
    /// <summary>
    /// The table which should be shown
    /// </summary>
    private readonly TableEntry _table;

    /// <summary>
    /// Creates a new instance of the <see cref="TableIndexWindow"/>
    /// </summary>
    public TableIndexWindow(TableEntry table)
    {
        InitializeComponent();
        _table = table;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="TableIndexWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void TableIndexWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is TableIndexWindowViewModel viewModel)
            viewModel.InitViewModel(_table);
    }

    /// <summary>
    /// Occurs when the user hits the close button
    /// </summary>
    /// <param name="sender">The <see cref="ButtonClose"/></param>
    /// <param name="e">The event arguments</param>
    private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}