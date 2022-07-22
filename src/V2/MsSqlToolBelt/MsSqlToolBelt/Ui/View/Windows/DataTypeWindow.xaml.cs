using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for DataTypeWindow.xaml
/// </summary>
public partial class DataTypeWindow : MetroWindow
{
    /// <summary>
    /// The instance of the class generator
    /// </summary>
    private readonly ClassGenManager? _manager;

    /// <summary>
    /// Creates a new instance of the <see cref="DataTypeWindow"/>
    /// </summary>
    public DataTypeWindow(ClassGenManager? manager)
    {
        InitializeComponent();

        _manager = manager;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="DataTypeWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void DataTypeWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DataTypeWindowViewModel viewModel) 
            return;

        viewModel.InitViewModel(_manager);
        viewModel.LoadData();
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