using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.DataObjects.Search;
using ZimLabs.TableCreator;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic for the <see cref="View.TableIndexWindow"/>
    /// </summary>
    internal sealed class TableIndexWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Contains the name of the current table
        /// </summary>
        private string _tableName;

        /// <summary>
        /// Backing field for <see cref="WindowTitle"/>
        /// </summary>
        private string _windowTitle = "Table indices";

        /// <summary>
        /// Gets or sets the title of the window
        /// </summary>
        public string WindowTitle
        {
            get => _windowTitle;
            private set => SetField(ref _windowTitle, value);
        }

        /// <summary>
        /// Backing field for <see cref="TableIndices"/>
        /// </summary>
        private ObservableCollection<TableIndex> _tableIndices = new();

        /// <summary>
        /// Gets or sets the list with the indices
        /// </summary>
        public ObservableCollection<TableIndex> TableIndices
        {
            get => _tableIndices;
            private set => SetField(ref _tableIndices, value);
        }

        /// <summary>
        /// Backing field for <see cref="Info"/>
        /// </summary>
        private string _info = "";

        /// <summary>
        /// Gets or sets the info which is shown above the data grid
        /// </summary>
        public string Info
        {
            get => _info;
            private set => SetField(ref _info, value);
        }

        /// <summary>
        /// The command to save the table definition as table
        /// </summary>
        public ICommand SaveAsTableCommand => new DelegateCommand(() => SaveAsTable(OutputType.Default));

        /// <summary>
        /// The command to save the table definition as markdown table
        /// </summary>
        public ICommand SaveAsMdTableCommand => new DelegateCommand(() => SaveAsTable(OutputType.Markdown));

        /// <summary>
        /// The command to save the table definition as csv table
        /// </summary>
        public ICommand SaveAsCsvTableCommand => new DelegateCommand(() => SaveAsTable(OutputType.Csv));

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="tableIndices">The list with the table indices</param>
        public void InitViewModel(List<TableIndex> tableIndices)
        {
            _tableName = tableIndices.FirstOrDefault()?.Table;
            WindowTitle = string.IsNullOrEmpty(_tableName) ? "Table indices" : $"{_tableName} indices";
            TableIndices = new ObservableCollection<TableIndex>(tableIndices);
            Info =
                $"Table '{_tableName}' contains {(TableIndices.Count > 1 ? $"{tableIndices.Count} indices" : "one index")}";
        }

        /// <summary>
        /// Saves the table information as table
        /// </summary>
        private void SaveAsTable(OutputType outputType)
        {
            TableIndices.Export($"{_tableName}_Indices", outputType);
        }
    }
}
