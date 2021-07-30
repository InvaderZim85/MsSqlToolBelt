using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.DataObjects;
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
        }

        /// <summary>
        /// The command to export the data type list as a text file
        /// </summary>
        public ICommand ExportTextCommand => new DelegateCommand(() => ExportAs(OutputType.Default));

        /// <summary>
        /// The command to export the data type list as a CSV file
        /// </summary>
        public ICommand ExportCsvCommand => new DelegateCommand(() => ExportAs(OutputType.Csv));

        /// <summary>
        /// The command to export the data type list as a markdown file
        /// </summary>
        public ICommand ExportMdCommand => new DelegateCommand(() => ExportAs(OutputType.Markdown));

        /// <summary>
        /// Exports the data type list as a csv file
        /// </summary>
        /// <param name="outputType">The desired output type</param>
        private void ExportAs(OutputType outputType)
        {
            TypeList.Export("DataTypes", outputType);
        }
    }
}
