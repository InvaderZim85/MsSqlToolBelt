using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.ClassGenerator;
using MsSqlToolBelt.DataObjects.Types;
using MsSqlToolBelt.View;
using ZimLabs.Database.MsSql;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic for the class generator control
    /// </summary>
    internal sealed class ClassGeneratorControlViewModel : ViewModelBase
    {
        /// <summary>
        /// The instance of the repo
        /// </summary>
        private GeneratorRepo _repo;

        /// <summary>
        /// Contains the value which indicates if the data was already loaded
        /// </summary>
        private bool _dataLoaded;

        /// <summary>
        /// Contains the class generator result
        /// </summary>
        private ClassGenResult _classGenResult;

        /// <summary>
        /// The method to set the code (sql, csharp)
        /// </summary>
        private Action<string, CodeType> _setCode;

        /// <summary>
        /// Contains the complete table list
        /// </summary>
        private List<Table> _originTableList;

        /// <summary>
        /// Backing field for <see cref="TableList"/>
        /// </summary>
        private ObservableCollection<Table> _tableList;

        /// <summary>
        /// Gets or sets the list with the tables
        /// </summary>
        public ObservableCollection<Table> TableList
        {
            get => _tableList;
            private set
            {
                SetField(ref _tableList, value);
                TableHeader = value != null ? $"{value.Count} tables" : "Tables";
            }
        }

        /// <summary>
        /// Backing field for <see cref="SelectedTable"/>
        /// </summary>
        private Table _selectedTable;

        /// <summary>
        /// Gets or sets the selected table
        /// </summary>
        public Table SelectedTable
        {
            get => _selectedTable;
            set
            {
                SetField(ref _selectedTable, value);
                Columns = value == null
                    ? new ObservableCollection<TableColumn>()
                    : new ObservableCollection<TableColumn>(value.Columns.OrderBy(o => o.ColumnPosition));
            }
        }

        /// <summary>
        /// Backing field for <see cref="Columns"/>
        /// </summary>
        private ObservableCollection<TableColumn> _columns;

        /// <summary>
        /// Gets or sets the columns of the selected table
        /// </summary>
        public ObservableCollection<TableColumn> Columns
        {
            get => _columns;
            private set
            {
                SetField(ref _columns, value);
                ColumnHeader = value != null ? $"{value.Count} columns" : "Columns";
            }
        }

        /// <summary>
        /// Gets the list with the class modifier
        /// </summary>
        public List<string> Modifier => new()
        {
            "public",
            "internal"
        };

        /// <summary>
        /// Backing field for <see cref="SelectedModifier"/>
        /// </summary>
        private string _selectedModifier;

        /// <summary>
        /// Gets or sets the selected modifier
        /// </summary>
        public string SelectedModifier
        {
            get => _selectedModifier;
            set => SetField(ref _selectedModifier, value);
        }

        /// <summary>
        /// Backing field for <see cref="MarkAsSealed"/>
        /// </summary>
        private bool _markAsSealed;

        /// <summary>
        /// Gets or sets the value wich indicates if the class should be marked as sealed
        /// </summary>
        public bool MarkAsSealed
        {
            get => _markAsSealed;
            set => SetField(ref _markAsSealed, value);
        }

        /// <summary>
        /// Backing field for <see cref="ClassName"/>
        /// </summary>
        private string _className;

        /// <summary>
        /// Gets or sets the class name
        /// </summary>
        public string ClassName
        {
            get => _className;
            set => SetField(ref _className, value);
        }

        /// <summary>
        /// Backing field for <see cref="CreateBackingField"/>
        /// </summary>
        private bool _createBackingField;

        /// <summary>
        /// Gets or sets the value which indicates if the CSharp class should created with backing field
        /// </summary>
        public bool CreateBackingField
        {
            get => _createBackingField;
            set => SetField(ref _createBackingField, value);
        }

        /// <summary>
        /// Backing field for <see cref="EfClass"/>
        /// </summary>
        private bool _efClass;

        /// <summary>
        /// Gets or sets the value which indicates if an entity framework class should be created
        /// </summary>
        public bool EfClass
        {
            get => _efClass;
            set => SetField(ref _efClass, value);
        }

        /// <summary>
        /// Backing field for <see cref="TableHeader"/>
        /// </summary>
        private string _tableHeader = "Tables";

        /// <summary>
        /// Gets or sets the table header
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
        /// Gets or sets the column header
        /// </summary>
        public string ColumnHeader
        {
            get => _columnHeader;
            private set => SetField(ref _columnHeader, value);
        }

        /// <summary>
        /// Backing field for <see cref="TableFilter"/>
        /// </summary>
        private string _tableFilter;

        /// <summary>
        /// Gets or sets the table filter
        /// </summary>
        public string TableFilter
        {
            get => _tableFilter;
            set => SetField(ref _tableFilter, value);
        }

        /// <summary>
        /// Backing field for <see cref="AddSummary"/>
        /// </summary>
        private bool _addSummary;

        /// <summary>
        /// Gets or sets the value which indicates if the user want's to add a summary
        /// </summary>
        public bool AddSummary
        {
            get => _addSummary;
            set => SetField(ref _addSummary, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowEfKeyCode"/>
        /// </summary>
        private bool _showEfKeyCode;

        /// <summary>
        /// Gets or sets the value which indicates if the ef key code should be shown
        /// </summary>
        public bool ShowEfKeyCode
        {
            get => _showEfKeyCode;
            set => SetField(ref _showEfKeyCode, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="setCode">The method to set the code</param>
        public void InitViewModel(Action<string, CodeType> setCode)
        {
            _setCode = setCode;
        }

        /// <summary>
        /// The command to generate the code
        /// </summary>
        public ICommand GenerateCommand => new DelegateCommand(GenerateCode);

        /// <summary>
        /// The command to show the query input dialog
        /// </summary>
        public ICommand GenerateFromQueryCommand => new DelegateCommand(GenerateCodeFromQuery);

        /// <summary>
        /// The command to clear the code
        /// </summary>
        public ICommand ClearCommand => new RelayCommand<CodeType>(Clear);

        /// <summary>
        /// The command to copy the code
        /// </summary>
        public ICommand CopyCommand => new RelayCommand<CodeType>(Copy);

        /// <summary>
        /// The command to filter the table list
        /// </summary>
        public ICommand FilterCommand => new DelegateCommand(FilterList);

        /// <summary>
        /// The command to set the column selection
        /// </summary>
        public ICommand SetSelectionCommand => new RelayCommand<SelectionType>(SetColumnSelection);

        /// <summary>
        /// The command to clear the alias values
        /// </summary>
        public ICommand ClearAliasCommand => new DelegateCommand(ClearAlias);

        /// <summary>
        /// The command to reload the tables
        /// </summary>
        public ICommand ReloadCommand => new DelegateCommand(() =>
        {
            _dataLoaded = false;
            LoadData();
        });

        /// <summary>
        /// The command to show the ef key code
        /// </summary>
        public ICommand ShowEfKeyCodeCommand => new DelegateCommand(() =>
        {
            var dialog = new TextDialog(new TextDialogSettings
            {
                Title = "Class generator",
                Caption = "Code to configure multiple columns as key",
                CheckboxText = "Without method",
                ShowOption = true,
                Text = _classGenResult.CodeEfKey,
                TextOption = _classGenResult.CodeEfKeyOption,
                CodeType = CodeType.CSharp
            })
            {
                Owner = Application.Current.MainWindow
            };

            dialog.ShowDialog();
        });

        /// <summary>
        /// Sets the connector
        /// </summary>
        /// <param name="connector">The connector</param>
        public void SetConnector(Connector connector)
        {
            _repo = new GeneratorRepo(connector);

            _dataLoaded = false;

            Clear();
        }

        /// <summary>
        /// Loads the data
        /// </summary>
        public void LoadData()
        {
            if (_dataLoaded)
                return;

            SelectedModifier = Modifier.FirstOrDefault();

            LoadTables();

            _dataLoaded = true;
        }

        /// <summary>
        /// Loads the tables
        /// </summary>
        private async void LoadTables()
        {
            var controller = await ShowProgress("Please wait", "Please wait while loading the tables...");

            try
            {
                var result = await _repo.LoadTables();

                _originTableList = result.OrderBy(o => o.Name).ToList();

                FilterList();
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Checks if the inserted class name is valid
        /// </summary>
        /// <returns>true when the class name is valid</returns>
        private bool ClassNameValid()
        {
            // Check if the class name starts with a number
            bool ClassNameStartsWithNumber()
            {
                if (string.IsNullOrWhiteSpace(ClassName))
                    return false;

                var firstChar = ClassName[0].ToString();
                return int.TryParse(firstChar, out _);
            }

            return !string.IsNullOrWhiteSpace(ClassName) && !ClassNameStartsWithNumber();
        }

        /// <summary>
        /// Generates the class
        /// </summary>
        private async void GenerateCode()
        {
            if (!ClassNameValid())
            {
                await ShowMessage("Class generator",
                    "Please enter a valid class name.\r\n\r\nHint: Must not start with a number and must not be empty");
                return;
            }

            // Check if a column is selected
            if (Columns.All(a => !a.Use))
            {
                await ShowMessage("Class generator", "You have to select at least one column to generate a class.");
                return;
            }

            var controller = await ShowProgress("Please wait", "Please wait while generating the class...");

            try
            {
                _classGenResult = await Task.Run(() =>
                    ClassGenerator.Generate(new ClassGenSettingsDto
                    {
                        Table = SelectedTable,
                        Modifier = SelectedModifier,
                        MarkAsSealed = MarkAsSealed,
                        ClassName = ClassName,
                        BackingField = CreateBackingField,
                        EfClass = EfClass,
                        AddSummary = AddSummary
                    }));

                if (_classGenResult == null)
                    return;

                _setCode(_classGenResult.Code, CodeType.CSharp);
                _setCode(_classGenResult.Sql, CodeType.Sql);

                ShowEfKeyCode = !string.IsNullOrEmpty(_classGenResult.CodeEfKey);
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Generates a class from the inserted query
        /// </summary>
        private async void GenerateCodeFromQuery()
        {
            if (!ClassNameValid())
            {
                await ShowMessage("Class generator",
                    "Please enter a valid class name.\r\n\r\nHint: Must not start with a number and must not be empty");
                return;
            }

            var dialog = new TextDialog(new TextDialogSettings
            {
                Title = "SQL Query",
                Caption = "Insert the SQL query to generate a class from it. Hint: Add a 'TOP (1)' to your query to reduce the load of the SQL server",
                ShowOption = false,
                ShowValidateButton = true,
                ValidationFunc = QueryHelper.ValidateSql,
                CodeType = CodeType.Sql
            })
            {
                Owner = Application.Current.MainWindow
            };

            dialog.ShowDialog();

            if (string.IsNullOrEmpty(dialog.Code))
                return;

            var controller = await ShowProgress("Please wait", "Please wait while generating the class...");

            try
            {
                _classGenResult = await ClassGenerator.GenerateFromQueryAsync(_repo, new ClassGenSettingsDto
                {
                    Table = SelectedTable,
                    Modifier = SelectedModifier,
                    MarkAsSealed = MarkAsSealed,
                    ClassName = ClassName,
                    BackingField = CreateBackingField,
                    EfClass = EfClass,
                    AddSummary = AddSummary,
                    SqlQuery = dialog.Code
                });

                if (_classGenResult == null)
                    return;

                _setCode(_classGenResult.Code, CodeType.CSharp);
                _setCode(_classGenResult.Sql, CodeType.Sql);

                ShowEfKeyCode = !string.IsNullOrEmpty(_classGenResult.CodeEfKey);
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Filters the list of tables according to the given filter string
        /// </summary>
        private void FilterList()
        {
            TableList = new ObservableCollection<Table>(string.IsNullOrEmpty(TableFilter)
                ? _originTableList
                : _originTableList.Where(w => w.Name.ContainsIgnoreCase(TableFilter)));

            SelectedTable = TableList.FirstOrDefault();
        }

        /// <summary>
        /// Sets the selection of the column entries
        /// </summary>
        /// <param name="type">The desired selection type</param>
        private void SetColumnSelection(SelectionType type)
        {
            if (Columns == null || !Columns.Any())
                return;

            foreach (var entry in Columns)
            {
                entry.Use = type == SelectionType.All;
            }
        }

        /// <summary>
        /// Clears the alias values
        /// </summary>
        private void ClearAlias()
        {
            if (Columns == null || !Columns.Any())
                return;

            foreach (var entry in Columns)
            {
                entry.Alias = "";
            }
        }

        /// <summary>
        /// Clears the editor
        /// </summary>
        /// <param name="type">The desired type</param>
        private void Clear(CodeType type)
        {
            _setCode("", type);
        }

        /// <summary>
        /// Copies the code
        /// </summary>
        /// <param name="type">The desired type</param>
        private void Copy(CodeType type)
        {
            CopyToClipboard(type == CodeType.CSharp ? _classGenResult.Code : _classGenResult.Sql);
        }

        /// <summary>
        /// Clears the content of the control
        /// </summary>
        public void Clear()
        {
            TableList = new ObservableCollection<Table>();
            Columns = new ObservableCollection<TableColumn>();
            TableHeader = "Tables";
            ColumnHeader = "Columns";
            _setCode("", CodeType.CSharp);
            _setCode("", CodeType.Sql);
        }
    }
}
