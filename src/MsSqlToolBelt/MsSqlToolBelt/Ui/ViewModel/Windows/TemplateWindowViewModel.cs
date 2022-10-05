using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Templates;
using MsSqlToolBelt.Ui.Common;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.TemplateWindow"/>
/// </summary>
internal sealed class TemplateWindowViewModel : ViewModelBase
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
    /// Backing field for <see cref="Templates"/>
    /// </summary>
    private ObservableCollection<TemplateEntry> _templates = new();

    /// <summary>
    /// Gets or sets the list with the templates
    /// </summary>
    public ObservableCollection<TemplateEntry> Templates
    {
        get => _templates;
        set => SetField(ref _templates, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedTemplate"/>
    /// </summary>
    private TemplateEntry? _selectedTemplate;

    /// <summary>
    /// Gets or sets the selected template
    /// </summary>
    public TemplateEntry? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (SetField(ref _selectedTemplate, value) && value != null)
                _setCodeEditorText?.Invoke(value.Content);
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// The command occurs when the user hits the copy button
    /// </summary>
    public ICommand CopyCommand => new DelegateCommand(() => CopyToClipboard(_getCodeEditorText?.Invoke() ?? ""));

    /// <summary>
    /// The command occurs when the user hits the save button
    /// </summary>
    public ICommand SaveCommand => new DelegateCommand(SaveChanges);

    /// <summary>
    /// The command occurs when the user hits the backup button
    /// </summary>
    public ICommand BackupCommand => new DelegateCommand(CreateBackup);

    /// <summary>
    /// The command occurs when the user hits the load backup button
    /// </summary>
    public ICommand LoadBackupCommand => new DelegateCommand(LoadBackup);
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
    private async void SaveChanges()
    {
        if (SelectedTemplate == null)
            return;

        // Get the content
        SelectedTemplate.Content = _getCodeEditorText?.Invoke() ?? string.Empty;

        await ShowProgressAsync("Please wait", "Please wait while saving the changes...");

        try
        {
            _manager.UpdateTemplate(SelectedTemplate);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            await CloseProgressAsync();
        }
    }

    /// <summary>
    /// Creates a backup of the selected template
    /// </summary>
    private async void CreateBackup()
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

        await ShowProgressAsync("Please wait", "Please wait while saving the backup...");

        try
        {
            _manager.CreateBackup(SelectedTemplate, dialog.FileName);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            await CloseProgressAsync();
        }
    }

    /// <summary>
    /// Loads the backup and stores it into the selected template
    /// </summary>
    private async void LoadBackup()
    {
        if (SelectedTemplate == null)
            return;

        var dialog = new OpenFileDialog
        {
            Filter = "Template file (*.cgt)|*.cgt"
        };

        if (dialog.ShowDialog() != true)
            return;

        await ShowProgressAsync("Please wait", "Please wait while loading the backup...");

        try
        {
            _manager.LoadBackup(SelectedTemplate, dialog.FileName);

            _setCodeEditorText?.Invoke(SelectedTemplate.Content);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            await CloseProgressAsync();
        }
    }
}