using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.DataObjects;
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
        /// The command to export the data type list as a CSV file
        /// </summary>
        public ICommand ExportCommand => new DelegateCommand(ExportAsCsv);

        /// <summary>
        /// Exports the data type list as a csv file
        /// </summary>
        private void ExportAsCsv()
        {
            var dialog = new CommonSaveFileDialog
            {
                Title = "Selected the desired destination",
                DefaultFileName = "DataTypes",
                DefaultExtension = ".csv",
                Filters = {new CommonFileDialogFilter("CSV file", "*.csv")}
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            var content = TableCreator.CreateTable(TypeList, OutputType.Csv);

            File.WriteAllText(dialog.FileName, content, Encoding.UTF8);
        }
    }
}
