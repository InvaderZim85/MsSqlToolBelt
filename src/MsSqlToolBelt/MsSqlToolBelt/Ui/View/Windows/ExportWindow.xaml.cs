using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for ExportWindow.xaml
/// </summary>
public partial class ExportWindow : MetroWindow
{
    /// <summary>
    /// Gets the value which indicates if the user want's to export the data (<see langword="true"/> = export, <see langword="false"/> = copy)
    /// </summary>
    public bool Export { get; private set; }

    /// <summary>
    /// Gets the desired filename (only set when <see cref="Export"/> is <see langword="true"/>)
    /// </summary>
    public string FileName => DataContext is ExportWindowViewModel viewModel ? viewModel.Filename : string.Empty;

    /// <summary>
    /// Gets the export type
    /// </summary>
    public ExportType ExportType => DataContext is ExportWindowViewModel viewModel
        ? (ExportType) (viewModel.SelectedExportType?.Id ?? 0)
        : ExportType.Ascii;

    /// <summary>
    /// The default file name
    /// </summary>
    private readonly string _defaultName;

    /// <summary>
    /// The desired export type
    /// </summary>
    private readonly ExportDataType _type;

    /// <summary>
    /// Creates a new instance of the <see cref="ExportWindow"/>
    /// </summary>
    /// <param name="defaultName">The default file name</param>
    /// <param name="type">The export type</param>
    public ExportWindow(string defaultName, ExportDataType type)
    {
        InitializeComponent();

        _defaultName = defaultName;
        _type = type;
    }

    /// <summary>
    /// The action to close the window
    /// </summary>
    /// <param name="result">The desired result</param>
    /// <param name="export">The value which indicates if the user want's to export the data</param>
    private void CloseWindow(bool result, bool export)
    {
        Export = export;
        DialogResult = result;
    }

    /// <summary>
    /// Occurs when the form was loaded
    /// </summary>
    /// <param name="sender">The <see cref="ExportWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void ExportWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ExportWindowViewModel viewModel)
            viewModel.InitViewModel(_defaultName, _type, CloseWindow);
    }

    /// <summary>
    /// Occurs when the user hits the close button
    /// </summary>
    /// <param name="sender">The <see cref="ButtonClose"/></param>
    /// <param name="e">The event arguments</param>
    private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
    {
        CloseWindow(false, false);
    }

    /// <summary>
    /// Occurs when the user hits the copy button
    /// </summary>
    /// <param name="sender">The <see cref="ButtonCopy"/></param>
    /// <param name="e">The event arguments</param>
    private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
    {
        CloseWindow(true, false);
    }
}