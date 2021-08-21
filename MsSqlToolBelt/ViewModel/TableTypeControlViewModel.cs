using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.TableType;
using ZimLabs.Database.MsSql;
using ZimLabs.TableCreator;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic of the table type control
    /// </summary>
    internal sealed class TableTypeControlViewModel : ViewModelBase
    {
        /// <summary>
        /// The instance for the interaction with the database
        /// </summary>
        private TableTypeRepo _repo;

        /// <summary>
        /// Contains the value which indicates if the data was already loaded
        /// </summary>
        private bool _dataLoaded;

        /// <summary>
        /// Backing field for <see cref="TableTypes"/>
        /// </summary>
        private ObservableCollection<TableType> _tableTypes;

        /// <summary>
        /// Gets or sets the list with the table types
        /// </summary>
        public ObservableCollection<TableType> TableTypes
        {
            get => _tableTypes;
            private set => SetField(ref _tableTypes, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedTableType"/>
        /// </summary>
        private TableType _selectedTableType;

        /// <summary>
        /// Gets or sets the selected table type
        /// </summary>
        public TableType SelectedTableType
        {
            get => _selectedTableType;
            set
            {
                SetField(ref _selectedTableType, value);
                Columns = value == null
                    ? new ObservableCollection<TableTypeColumn>()
                    : new ObservableCollection<TableTypeColumn>(value.Columns.OrderBy(o => o.ColumnId));

                ColumnHeader = $"{value?.Columns?.Count ?? 0} column(s)";
            }
        }

        /// <summary>
        /// Backing field for <see cref="Columns"/>
        /// </summary>
        private ObservableCollection<TableTypeColumn> _columns;

        /// <summary>
        /// Gets or sets the columns of the selected table type
        /// </summary>
        public ObservableCollection<TableTypeColumn> Columns
        {
            get => _columns;
            private set => SetField(ref _columns, value);
        }

        /// <summary>
        /// Backing field for <see cref="TableHeader"/>
        /// </summary>
        private string _tableHeader = "Table types";

        /// <summary>
        /// Gets or sets the header for the table grid
        /// </summary>
        public string TableHeader
        {
            get => _tableHeader;
            private set => SetField(ref _tableHeader, value);
        }

        /// <summary>
        /// Backing field for <see cref="ColumnHeader"/>
        /// </summary>
        private string _columnHeader = "Columns";

        /// <summary>
        /// Gets or sets the header for the column grid
        /// </summary>
        public string ColumnHeader
        {
            get => _columnHeader;
            private set => SetField(ref _columnHeader, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedSaveTypeTable"/>
        /// </summary>
        private TextValueItem _selectedSaveTypeTable;

        /// <summary>
        /// Gets or sets the selected save type
        /// </summary>
        public TextValueItem SelectedSaveTypeTable
        {
            get => _selectedSaveTypeTable;
            set => SetField(ref _selectedSaveTypeTable, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedCopyTypeTable"/>
        /// </summary>
        private TextValueItem _selectedCopyTypeTable;

        /// <summary>
        /// Gets or sets the selected copy type
        /// </summary>
        public TextValueItem SelectedCopyTypeTable
        {
            get => _selectedCopyTypeTable;
            set => SetField(ref _selectedCopyTypeTable, value);
        }

        /// <summary>
        /// Sets the connector
        /// </summary>
        /// <param name="connector">The connector</param>
        public void SetConnector(Connector connector)
        {
            _repo = new TableTypeRepo(connector);

            _dataLoaded = false;
        }

        /// <summary>
        /// Loads the data
        /// </summary>
        public void LoadData()
        {
            if (_dataLoaded)
                return;

            LoadTableTypes();

            _dataLoaded = true;

            InitSaveCopyTypes();
        }

        /// <summary>
        /// The command to reload the data
        /// </summary>
        public ICommand ReloadCommand => new DelegateCommand(() =>
        {
            _dataLoaded = false;
            LoadData();
        });

        /// <summary>
        /// The command to copy the table to the clipboard
        /// </summary>
        public ICommand CopyAsCommand => new DelegateCommand(CopyAs);

        /// <summary>
        /// The command to save the table definition as table
        /// </summary>
        public ICommand SaveAsCommand => new DelegateCommand(SaveAs);

        /// <summary>
        /// The command to copy the table to the clipboard
        /// </summary>
        public ICommand CopyAsTableCommand => new DelegateCommand(CopyAsTable);

        /// <summary>
        /// The command to save the table definition as table
        /// </summary>
        public ICommand SaveAsTableCommand => new DelegateCommand(SaveAsTable);

        /// <summary>
        /// Loads the table types
        /// </summary>
        private async void LoadTableTypes()
        {
            var controller = await ShowProgress("Please wait",
                "Please wait while loading the table types...");

            try
            {
                var result = await Task.Run(() => _repo.LoadTableTypes());

                TableTypes = new ObservableCollection<TableType>(result.OrderBy(o => o.Name));
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
            finally
            {
                await controller.CloseAsync();
                TableHeader = $"{TableTypes?.Count ?? 0} table type(s)";
            }
        }

        /// <summary>
        /// Clears the content of the control
        /// </summary>
        public void Clear()
        {
            TableTypes = new ObservableCollection<TableType>();
            TableHeader = "Table types";
            ColumnHeader = "Columns";
        }

        /// <summary>
        /// Copies the table to the clipboard
        /// </summary>
        private void CopyAs()
        {
            if (SelectedCopyType is null)
                return;

            CopyValues((OutputType)SelectedCopyType.Id, TableTypes);
        }

        /// <summary>
        /// Saves the table information as table
        /// </summary>
        private void SaveAs()
        {
            if (SelectedSaveType is null)
                return;

            SaveValues((OutputType)SelectedSaveType.Id, TableTypes, "TableTypes");
        }

        /// <summary>
        /// Copies the table to the clipboard
        /// </summary>
        private void CopyAsTable()
        {
            if (SelectedCopyTypeTable is null)
                return;

            var type = (OutputType)SelectedCopyTypeTable.Id;

            CopyValues(type, Columns);
        }

        /// <summary>
        /// Saves the table information as table
        /// </summary>
        private void SaveAsTable()
        {
            if (SelectedSaveTypeTable is null)
                return;

            SaveValues((OutputType)SelectedSaveTypeTable.Id, Columns, $"{SelectedTableType.Name}_Columns");
        }
    }
}
