using System;
using System.Windows;
using System.Windows.Input;
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
        /// Backing field for <see cref="Caption"/>
        /// </summary>
        private string _caption;

        /// <summary>
        /// Gets or sets the caption
        /// </summary>
        public string Caption
        {
            get => _caption;
            set => SetField(ref _caption, value);
        }

        /// <summary>
        /// The command to copy the code
        /// </summary>
        public ICommand CopyCommand => new RelayCommand<CodeType>(Copy);

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="caption">The caption which should be shown</param>
        /// <param name="getEditorText">Function to get the text of the editor control</param>
        public void InitViewModel(string caption, Func<string> getEditorText)
        {
            Caption = caption;
            _getEditorText = getEditorText;
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
