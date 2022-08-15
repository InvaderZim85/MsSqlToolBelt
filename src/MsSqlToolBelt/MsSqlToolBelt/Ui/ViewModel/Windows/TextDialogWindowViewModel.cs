﻿using System;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.DataObjects.Common;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

internal class TextDialogWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the text of the editor control
    /// </summary>
    private Func<string>? _getEditorText;

    /// <summary>
    /// Sets the text of the editor control
    /// </summary>
    private Action<string>? _setEditorText;

    /// <summary>
    /// The action to close the window
    /// </summary>
    private Action? _closeWindow;

    /// <summary>
    /// Backing field for <see cref="Settings"/>
    /// </summary>
    private TextDialogSettings _settings = new();

    /// <summary>
    /// Gets or sets the settings
    /// </summary>
    public TextDialogSettings Settings
    {
        get => _settings;
        set => SetField(ref _settings, value);
    }

    /// <summary>
    /// Backing field for <see cref="ShowValidationInfo"/>
    /// </summary>
    private bool _showValidationInfo;

    /// <summary>
    /// Gets or sets the value which indicates if the validation info label should be shown
    /// </summary>
    public bool ShowValidationInfo
    {
        get => _showValidationInfo;
        set => SetField(ref _showValidationInfo, value);
    }

    private bool _codeValid;

    /// <summary>
    /// Gets or sets the value which indicates if the inserted code is valid
    /// </summary>
    public bool CodeValid
    {
        get => _codeValid;
        set
        {
            _codeValid = value;
            ShowValidationInfo = !value && _settings.ShowValidateButton && _settings.ValidationFunc != null;
        }
    }

    /// <summary>
    /// Backing field for <see cref="ValidateButtonVisible"/>
    /// </summary>
    private bool _validateButtonVisible;

    /// <summary>
    /// Gets or sets the value which indicates if the validate button is visible
    /// </summary>
    public bool ValidateButtonVisible
    {
        get => _validateButtonVisible;
        set => SetField(ref _validateButtonVisible, value);
    }

    /// <summary>
    /// The command to copy the code
    /// </summary>
    public ICommand CopyCommand => new DelegateCommand(Copy);

    /// <summary>
    /// The command to validate the inserted text
    /// </summary>
    public ICommand ValidateCommand => new DelegateCommand(ExecuteValidation);

    /// <summary>
    /// The command to close the window
    /// </summary>
    public ICommand CloseCommand => new DelegateCommand(CloseWindow);

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="settings">The settings</param>
    /// <param name="setEditorText">The action to set the text of the editor control</param>
    /// <param name="getEditorText">The function to get the text of the editor control</param>
    /// <param name="closeWindow">The action to close the window</param>
    public void InitViewModel(TextDialogSettings settings, Action<string> setEditorText, Func<string> getEditorText, Action closeWindow)
    {
        Settings = settings;
        _getEditorText = getEditorText;
        _setEditorText = setEditorText;
        _closeWindow = closeWindow;

        ValidateButtonVisible = settings.ShowValidateButton && settings.ValidationFunc != null;

        setEditorText(settings.Text);

        CodeValid = true;
    }

    /// <summary>
    /// Copies the code
    /// </summary>
    private void Copy()
    {
        CopyToClipboard(_getEditorText?.Invoke() ?? string.Empty);
    }

    /// <summary>
    /// Executes the validation
    /// </summary>
    private async void ExecuteValidation()
    {
        if (_settings.ValidationFunc == null)
            return;

        var text = _getEditorText?.Invoke();

        if (string.IsNullOrEmpty(text))
            return;

        await ShowProgressAsync("Please wait", "Please wait while validating the query...");

        try
        {
            var (valid, message) = await _settings.ValidationFunc(text);

            if (valid)
            {
                CodeValid = true;
                await ShowMessageAsync("Validation", "Inserted SQL query is valid." +
                                                     "\r\n\r\nNote: Even if the SQL query has been validated successfully, " +
                                                     "it may not be possible to execute the query if, for example, " +
                                                     "a column or table does not exist or is misspelled.");
            }
            else
            {
                CodeValid = false;
                await ShowMessageAsync("Validation", $"The inserted SQL query is not valid:\r\n{message}");
            }
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
    /// Closes the window if everything is okay (code validation)
    /// </summary>
    private async void CloseWindow()
    {
        if (!_settings.ShowValidateButton || _settings.ValidationFunc == null)
            _closeWindow?.Invoke();

        if (CodeValid)
            _closeWindow?.Invoke();

        var result = await ShowQuestionAsync("Validation",
            "The code has been changed and needs to be validated again. If you close the window anyway, the generation of the class will be aborted." +
            "\r\n\r\nClose the window anyway?",
            "Yes", "No");

        if (result == MessageDialogResult.Affirmative)
            _closeWindow?.Invoke();
    }
}