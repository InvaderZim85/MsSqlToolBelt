using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using System.Collections.ObjectModel;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.ExportWindow"/>
/// </summary>
internal partial class ExportWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Contains the default name
    /// </summary>
    private string _defaultName = string.Empty;

    /// <summary>
    /// The action to close the window
    /// </summary>
    private Action<bool, bool>? _closeWindow;

    /// <summary>
    /// Gets the filename
    /// </summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// The list with the export types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IdTextEntry> _exportTypes = [];

    /// <summary>
    /// The selected export type
    /// </summary>
    [ObservableProperty]
    private IdTextEntry? _selectedExportType;

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="defaultName">The default name</param>
    /// <param name="type">The desired export type</param>
    /// <param name="closeWindow">The action to close the window</param>
    public void InitViewModel(string defaultName, ExportDataType type, Action<bool, bool> closeWindow)
    {
        _defaultName = defaultName;
        _closeWindow = closeWindow;

        ExportTypes = new ObservableCollection<IdTextEntry>(Helper.CreateExportTypeList(type));
        SelectedExportType = ExportTypes.FirstOrDefault();
    }

    /// <summary>
    /// Exports the data
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ExportDataAsync()
    {
        if (SelectedExportType is not {BoundItem: ExportTypeDescriptionAttribute attribute})
            return;

        try
        {
            var dialog = new SaveFileDialog
            {
                FileName = _defaultName,
                DefaultExt = attribute.Extension,
                Filter = $"{attribute.Description} (*.{attribute.Extension})|*.{attribute.Extension}"
            };

            if (dialog.ShowDialog() != true)
                return;

            Filename = dialog.FileName;

            _closeWindow?.Invoke(true, true);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }
}