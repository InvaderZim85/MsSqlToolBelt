using System.Collections.ObjectModel;
using System.Windows.Input;
using MsSqlToolBelt.DataObjects.Types;
using ZimLabs.TableCreator;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic for the <see cref="View.DataTypeControl"/>
    /// </summary>
    internal sealed class DataTypeControlViewModel : ViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="TypeList"/>
        /// </summary>
        private ObservableCollection<DataType> _typeList;

        /// <summary>
        /// Gets or sets the list with the data types
        /// </summary>
        public ObservableCollection<DataType> TypeList
        {
            get => _typeList;
            private set => SetField(ref _typeList, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        public void InitViewModel()
        {
            TypeList = new ObservableCollection<DataType>(Helper.DataTypes);

            InitSaveCopyTypes();
        }

        /// <summary>
        /// The command to copy the table to the clipboard
        /// </summary>
        public ICommand CopyAsCommand => new DelegateCommand(CopyAs);

        /// <summary>
        /// The command to save the table definition as table
        /// </summary>
        public ICommand SaveAsCommand => new DelegateCommand(SaveAs);

        /// <summary>
        /// Copies the table to the clipboard
        /// </summary>
        private void CopyAs()
        {
            if (SelectedCopyType is null)
                return;

            var type = (OutputType)SelectedCopyType.Id;

            CopyValues(type, TypeList);
        }

        /// <summary>
        /// Saves the table information as table
        /// </summary>
        private void SaveAs()
        {
            if (SelectedSaveType is null)
                return;

            SaveValues((OutputType)SelectedSaveType.Id, TypeList, "DataTypes");
        }
    }
}
