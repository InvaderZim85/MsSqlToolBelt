using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Templates;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.TemplateWindow"/>
/// </summary>
internal partial class TemplateWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the templates
    /// </summary>
    private readonly TemplateManager _manager = new();

    /// <summary>
    /// Sets the text of the code editor
    /// </summary>
    private Action<string>? _setCodeEditorText;

    /// <summary>
    /// Gets the text of the code editor
    /// </summary>
    private Func<string>? _getCodeEditorText;

    #region View properties

    /// <summary>
    /// The list with the templates
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TemplateEntry> _templates = new();

    /// <summary>
    /// Backing field for <see cref="SelectedTemplate"/>
    /// </summary>
    [ObservableProperty]
    private TemplateEntry? _selectedTemplate;

    #endregion

    #region Commands

    /// <summary>
    /// The command occurs when the user hits the copy button
    /// </summary>
    public ICommand CopyCommand => new RelayCommand(() => CopyToClipboard(_getCodeEditorText?.Invoke() ?? ""));
    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="setEditorText">The action to set the editor text</param>
    /// <param name="getEditorText">The function to get the editor text</param>
    public void InitViewModel(Action<string> setEditorText, Func<string> getEditorText)
    {
        _setCodeEditorText = setEditorText;
        _getCodeEditorText = getEditorText;
        _manager.LoadTemplates();

        Templates = _manager.Templates.ToObservableCollection();
    }

    /// <summary>
    /// Saves the changes of the current template
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (SelectedTemplate == null)
            return;

        // Get the content
        SelectedTemplate.Content = _getCodeEditorText?.Invoke() ?? string.Empty;

        var controller = await ShowProgressAsync("Please wait", "Please wait while saving the changes...");

        try
        {
            TemplateManager.UpdateTemplate(SelectedTemplate);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Creates a backup of the selected template
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        if (SelectedTemplate == null)
            return;

        var dialog = new SaveFileDialog
        {
            FileName = SelectedTemplate.FileName,
            Filter = "Template file (*.cgt)|*.cgt"
        };

        if (dialog.ShowDialog() != true)
            return;

        var controller = await ShowProgressAsync("Please wait", "Please wait while saving the backup...");

        try
        {
            TemplateManager.CreateBackup(SelectedTemplate, dialog.FileName);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Loads the backup and stores it into the selected template
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task LoadBackupAsync()
    {
        if (SelectedTemplate == null)
            return;

        var dialog = new OpenFileDialog
        {
            Filter = "Template file (*.cgt)|*.cgt"
        };

        if (dialog.ShowDialog() != true)
            return;

        var controller = await ShowProgressAsync("Please wait", "Please wait while loading the backup...");

        try
        {
            TemplateManager.LoadBackup(SelectedTemplate, dialog.FileName);

            _setCodeEditorText?.Invoke(SelectedTemplate.Content);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Occurs when the user selects another template
    /// </summary>
    /// <param name="value">The selected template</param>
    partial void OnSelectedTemplateChanged(TemplateEntry? value)
    {
        if (value == null)
            return;

        _setCodeEditorText?.Invoke(value.Content);
    }
}