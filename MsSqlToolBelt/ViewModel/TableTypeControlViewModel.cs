using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.TableType;
using MsSqlToolBelt.View;
using ZimLabs.Database.MsSql;
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
            set => SetField(ref _tableTypes, value);
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
                    : new ObservableCollection<TableTypeColumn>(value.Columns);

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
            set => SetField(ref _columns, value);
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
            set => SetField(ref _tableHeader, value);
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
            set => SetField(ref _columnHeader, value);
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
        }

        /// <summary>
        /// The command to open the export command
        /// </summary>
        public ICommand ExportCommand => new DelegateCommand(Export);

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
        /// Opens the export window
        /// </summary>
        private void Export()
        {
            var exportDialog = new ExportTableTypeDialog(TableTypes.ToList()) {Owner = Application.Current.MainWindow};
            exportDialog.ShowDialog();
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
    }
}
