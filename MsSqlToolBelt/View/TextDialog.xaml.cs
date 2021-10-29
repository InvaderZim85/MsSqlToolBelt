using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.Types;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for TextDialog.xaml
    /// </summary>
    public partial class TextDialog : MetroWindow
    {
        /// <summary>
        /// Contains the settings of the dialog
        /// </summary>
        private readonly TextDialogSettings _settings;

        /// <summary>
        /// Creates a new instance of the <see cref="TextDialog"/>
        /// </summary>
        /// <param name="settings">The options of the dialog</param>
        public TextDialog(TextDialogSettings settings)
        {
            InitializeComponent();

            _settings = settings;
            Title = settings.Title;
        }

        /// <summary>
        /// Gets the text of the editor
        /// </summary>
        /// <returns>The text which should be returned</returns>
        private string GetEditorText()
        {
            return CodeEditor.Text;
        }

        /// <summary>
        /// Sets the text of the editor
        /// </summary>
        /// <param name="text">The text which should be shown</param>
        private void SetEditorText(string text)
        {
            CodeEditor.Text = text;
        }

        /// <summary>
        /// Sets the schema of the editor controls
        /// </summary>
        private void SetTheme()
        {
            // CSharp editor
            Helper.InitAvalonEditor(CodeEditor, CodeType.CSharp);
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        /// <param name="sender">The <see cref="TextDialog"/></param>
        /// <param name="e">The event arguments</param>
        private void TextDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TextDialogViewModel viewModel)
                viewModel.InitViewModel(_settings, GetEditorText, SetEditorText);

            CodeEditor.Text = _settings.Text;

            SetTheme();

            Helper.AddAction("SetTheme", SetTheme);
        }

        /// <summary>
        /// Occurs when the user hits the close button
        /// </summary>
        /// <param name="sender">The close button</param>
        /// <param name="e">The event arguments</param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
