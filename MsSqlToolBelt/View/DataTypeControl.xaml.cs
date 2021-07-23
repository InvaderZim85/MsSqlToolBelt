using System.Windows.Controls;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for DataTypeControl.xaml
    /// </summary>
    public partial class DataTypeControl : UserControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DataTypeControl"/>
        /// </summary>
        public DataTypeControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Init the control
        /// </summary>
        public void InitControl()
        {
            if (DataContext is DataTypeControlViewModel viewModel)
                viewModel.InitViewModel();
        }
    }
}
