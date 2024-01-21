using MahApps.Metro.Controls;
using MsSqlToolBelt.Ui.ViewModel.Windows;
using System.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for CustomColorWindow.xaml
/// </summary>
public partial class CustomColorWindow : MetroWindow
{
    /// <summary>
    /// Gets the name of the new color
    /// </summary>
    public string ColorName => DataContext is CustomColorWindowViewModel viewModel ? viewModel.Name : string.Empty;

    /// <summary>
    /// Creates a new instance of the <see cref="CustomColorWindow"/>
    /// </summary>
    public CustomColorWindow()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="CustomColorWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void CustomColorWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CustomColorWindowViewModel viewModel)
            viewModel.InitViewModel(result => DialogResult = result);
    }
}