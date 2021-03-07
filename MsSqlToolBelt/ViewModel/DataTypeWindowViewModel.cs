using System.Collections.ObjectModel;
using MsSqlToolBelt.DataObjects;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic of the data type info window
    /// </summary>
    internal sealed class DataTypeWindowViewModel : ViewModelBase
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
        }
    }
}
