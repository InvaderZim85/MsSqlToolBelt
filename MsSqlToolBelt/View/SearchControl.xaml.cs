using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.ViewModel;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchControl : UserControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SearchControl"/>
        /// </summary>
        public SearchControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the connector of the view model
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        public void SetConnector(Connector connector)
        {
            if (DataContext is SearchControlViewModel viewModel)
                viewModel.SetConnector(connector);
        }

        /// <summary>
        /// Sets the text of the avalon editor
        /// </summary>
        /// <param name="text">The desired text</param>
        private void SetSqlText(string text)
        {
            SqlEditor.Text = text;
        }

        /// <summary>
        /// Sets the sql schema of the editor window
        /// </summary>
        private void SetSqlSchema()
        {
            //var dark = Properties.Settings.Default.Theme.ContainsIgnoreCase("dark");
            //var dark = true;

            SqlEditor.Options.HighlightCurrentLine = true;
            SqlEditor.SyntaxHighlighting = LoadSqlSchema(true);
            //SqlEditor.Foreground = new SolidColorBrush(dark ? Colors.White : Colors.Black);
            SqlEditor.Foreground = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// Loads the highlight definition for the avalon editor
        /// </summary>
        /// <returns>The definition</returns>
        private static IHighlightingDefinition LoadSqlSchema(bool dark)
        {
            var fileName = dark ? "AvalonSqlSchema_Dark.xml" : "AvalonSqlSchema.xml";
            var file = Path.Combine(ZimLabs.CoreLib.Core.GetBaseFolder(), "SqlSchema", fileName);

            using (var reader = File.Open(file, FileMode.Open))
            {
                using (var xmlReader = new XmlTextReader(reader))
                {
                    return HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
                }
            }
        }

        /// <summary>
        /// Init the control
        /// </summary>
        public void InitControl()
        {
            if (!(DataContext is SearchControlViewModel viewModel))
                return;

            viewModel.InitViewModel(SetSqlText);
            SetSqlSchema();
        }

        /// <summary>
        /// Occurs when the user hits the CTRL + C while the data grid has the focus
        /// </summary>
        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(ResultGrid.SelectedItem is SearchResult result))
                return;

            Clipboard.SetText(result.Name);
        }
    }
}
