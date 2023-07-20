using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.DataObjects.Common;
using System;
using System.Threading.Tasks;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

internal partial class TextDialogWindowViewModel : ViewModelBase
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
    /// The settings
    /// </summary>
    [ObservableProperty]
    private TextDialogSettings _settings = new();

    /// <summary>
    /// The value which indicates if the validation info label should be shown
    /// </summary>
    [ObservableProperty]
    private bool _showValidationInfo;

    /// <summary>
    /// Backing field for <see cref="CodeValid"/>
    /// </summary>
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
            ShowValidationInfo = !value && Settings.ShowValidateButton && Settings.ValidationFunc != null;
        }
    }

    /// <summary>
    /// The value which indicates if the validate button is visible
    /// </summary>
    [ObservableProperty]
    private bool _validateButtonVisible;

    /// <summary>
    /// Backing field for <see cref="ShowOptionalText"/>
    /// </summary>
    private bool _showOptionalText;

    /// <summary>
    /// Gets or sets the value which indicates if the optional text should be shown
    /// </summary>
    public bool ShowOptionalText
    {
        get => _showOptionalText;
        set
        {
            SetProperty(ref _showOptionalText, value);
            _setEditorText?.Invoke(value ? Settings.TextOption : Settings.Text);
        }
    }

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
    [RelayCommand]
    private void CopyContent()
    {
        CopyToClipboard(_getEditorText?.Invoke() ?? string.Empty);
    }

    /// <summary>
    /// Executes the validation
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ExecuteValidationAsync()
    {
        if (Settings.ValidationFunc == null)
            return;

        var text = _getEditorText?.Invoke();

        if (string.IsNullOrEmpty(text))
            return;

        var controller = await ShowProgressAsync("Please wait", "Please wait while validating the query...");

        try
        {
            var (valid, message) = await Settings.ValidationFunc(text);

            if (valid)
            {
                CodeValid = true;
                await ShowMessageAsync(MessageHelper.ValidationValidNote);
            }
            else
            {
                CodeValid = false;
                await ShowMessageAsync(new MessageEntry("Validation",
                    "The inserted SQL query / statement is not valid.", "", $"Message: {message}"));
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Complex);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Closes the window if everything is okay (code validation)
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task CloseWindowAsync()
    {
        if (!Settings.ShowValidateButton || Settings.ValidationFunc == null)
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