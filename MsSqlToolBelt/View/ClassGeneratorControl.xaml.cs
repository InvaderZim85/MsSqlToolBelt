using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;
using MsSqlToolBelt.DataObjects.Types;
using MsSqlToolBelt.ViewModel;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for ClassGeneratorControl.xaml
    /// </summary>
    public partial class ClassGeneratorControl : UserControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ClassGeneratorControl"/>
        /// </summary>
        public ClassGeneratorControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the connector of the view model
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        public void SetConnector(Connector connector)
        {
            if (DataContext is ClassGeneratorControlViewModel viewModel)
                viewModel.SetConnector(connector);
        }

        /// <summary>
        /// Sets the text of one of the editors
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="type">The desired control</param>
        private void SetCode(string text, CodeType type)
        {
            switch (type)
            {
                case CodeType.CSharp:
                    CodeEditor.Text = text;
                    break;
                case CodeType.Sql:
                    SqlEditor.Text = text;
                    break;
            }
        }

        /// <summary>
        /// Sets the schema of the editor controls
        /// </summary>
        private void SetSchema()
        {
            // Sql editor
            Helper.InitAvalonEditor(SqlEditor, CodeType.Sql);

            // CSharp editor
            Helper.InitAvalonEditor(CodeEditor, CodeType.CSharp);
        }

        /// <summary>
        /// Init the control
        /// </summary>
        public void InitControl()
        {
            if (DataContext is not ClassGeneratorControlViewModel viewModel)
                return;

            viewModel.InitViewModel(SetCode);
            SetSchema();
        }

        /// <summary>
        /// Loads the data
        /// </summary>
        public void LoadData()
        {
            if (DataContext is ClassGeneratorControlViewModel viewModel)
                viewModel.LoadData();
        }

        /// <summary>
        /// Clears the content of the control
        /// </summary>
        public void Clear()
        {
            if (DataContext is ClassGeneratorControlViewModel viewModel)
                viewModel.Clear();
        }

        /// <summary>
        /// Occurs when the user hits the link
        /// </summary>
        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }
}
