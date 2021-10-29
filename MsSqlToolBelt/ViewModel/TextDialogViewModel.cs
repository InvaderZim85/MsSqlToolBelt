using System;
using System.Windows;
using System.Windows.Input;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.Types;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic for the text dialog window
    /// </summary>
    internal sealed class TextDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets the text of the editor
        /// </summary>
        private Func<string> _getEditorText;

        /// <summary>
        /// Sets the text of the editor
        /// </summary>
        private Action<string> _setEditorText;

        /// <summary>
        /// The settings of the dialog
        /// </summary>
        private TextDialogSettings _settings;

        /// <summary>
        /// Backing field for <see cref="Caption"/>
        /// </summary>
        private string _caption;

        /// <summary>
        /// Gets or sets the caption
        /// </summary>
        public string Caption
        {
            get => _caption;
            private set => SetField(ref _caption, value);
        }

        /// <summary>
        /// Backing field for <see cref="CheckboxText"/>
        /// </summary>
        private string _checkboxText;

        /// <summary>
        /// Gets or sets the text of the checkbox
        /// </summary>
        public string CheckboxText
        {
            get => _checkboxText;
            set => SetField(ref _checkboxText, value);
        }

        /// <summary>
        /// Backing field for <see cref="OptionVisibility"/>
        /// </summary>
        private Visibility _optionVisibility = Visibility.Hidden;

        /// <summary>
        /// Gets or sets the value which indicates if the option check box should be shown
        /// </summary>
        public Visibility OptionVisibility
        {
            get => _optionVisibility;
            set => SetField(ref _optionVisibility, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowOption"/>
        /// </summary>
        private bool _showOption;

        /// <summary>
        /// Gets or sets the value which indicates if the optional text should be shown
        /// </summary>
        public bool ShowOption
        {
            get => _showOption;
            set
            {
                if (SetField(ref _showOption, value))
                    _setEditorText(value ? _settings.TextOption : _settings.Text);
            }
        }

        /// <summary>
        /// The command to copy the code
        /// </summary>
        public ICommand CopyCommand => new RelayCommand<CodeType>(Copy);

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="settings">The settings</param>
        /// <param name="getEditorText">The function to get the text of the editor</param>
        /// <param name="setEditorText">The action to set the text of the editor</param>
        public void InitViewModel(TextDialogSettings settings, Func<string> getEditorText, Action<string> setEditorText)
        {
            _settings = settings;

            Caption = settings.Caption;
            OptionVisibility = settings.ShowOption ? Visibility.Visible : Visibility.Hidden;
            _getEditorText = getEditorText;
            _setEditorText = setEditorText;
            CheckboxText = settings.CheckboxText;

            _setEditorText(_settings.Text);
        }

        /// <summary>
        /// Copies the code
        /// </summary>
        /// <param name="type">The desired type</param>
        private void Copy(CodeType type)
        {
            CopyToClipboard(_getEditorText());
        }
    }
}
