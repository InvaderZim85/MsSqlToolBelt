using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.ExportWindow"/>
/// </summary>
internal class ExportWindowViewModel : ViewModelBase
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
    /// Backing field for <see cref="ExportTypes"/>
    /// </summary>
    private ObservableCollection<IdTextEntry> _exportTypes = new();

    /// <summary>
    /// Gets or sets the list with the export types
    /// </summary>
    public ObservableCollection<IdTextEntry> ExportTypes
    {
        get => _exportTypes;
        private set => SetProperty(ref _exportTypes, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedExportType"/>
    /// </summary>
    private IdTextEntry? _selectedExportType;

    /// <summary>
    /// Gets or sets the selected export type
    /// </summary>
    public IdTextEntry? SelectedExportType
    {
        get => _selectedExportType;
        set => SetProperty(ref _selectedExportType, value);
    }

    /// <summary>
    /// The command to export the data
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(ExportData);

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
    private async void ExportData()
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

            _closeWindow?.Invoke(true, true);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }
}