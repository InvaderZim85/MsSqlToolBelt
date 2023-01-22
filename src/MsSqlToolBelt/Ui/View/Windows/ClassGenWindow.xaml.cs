using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for ClassGenWindow.xaml
/// </summary>
public partial class ClassGenWindow : MetroWindow
{
    /// <summary>
    /// The instance of the class generator
    /// </summary>
    private readonly ClassGenManager _manager;

    /// <summary>
    /// Creates a new instance of the <see cref="ClassGenWindow"/>
    /// </summary>
    /// <param name="classGenManager">The instance of the class generator</param>
    public ClassGenWindow(ClassGenManager classGenManager)
    {
        InitializeComponent();

        _manager = classGenManager;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="ClassGenWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void ClassGenWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ClassGenWindowViewModel viewModel)
            viewModel.InitViewModel(_manager);
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridColumns"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridColumns_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridColumns.CopyToClipboard<ColumnDto>();
    }

    /// <summary>
    /// Occurs when the user hits the link
    /// </summary>
    /// <param name="sender">The hyper link</param>
    /// <param name="e">The event arguments</param>
    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Helper.OpenLink(e.Uri.ToString());
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
}