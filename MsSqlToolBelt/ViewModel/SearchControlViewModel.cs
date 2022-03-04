using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.View;
using ZimLabs.Database.MsSql;
using ZimLabs.TableCreator;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic of the search control
    /// </summary>
    internal sealed class SearchControlViewModel : ViewModelBase
    {
        /// <summary>
        /// Contains the instance of the search repo
        /// </summary>
        private SearchRepo _repo;

        /// <summary>
        /// The method to set the sql text
        /// </summary>
        private Action<string> _setSqlText;

        /// <summary>
        /// Contains the sql text
        /// </summary>
        private string _sqlQuery;

        /// <summary>
        /// Backing field for <see cref="Search"/>
        /// </summary>
        private string _search;

        /// <summary>
        /// Gets or sets the search value
        /// </summary>
        public string Search
        {
            get => _search;
            set => SetField(ref _search, value);
        }

        /// <summary>
        /// Backing field for <see cref="MatchWholeWord"/>
        /// </summary>
        private bool _matchWholeWord;

        /// <summary>
        /// Gets or sets the value which indicates if the whole word should match
        /// </summary>
        public bool MatchWholeWord
        {
            get => _matchWholeWord;
            set => SetField(ref _matchWholeWord, value);
        }

        /// <summary>
        /// Contains the original result list
        /// </summary>
        private List<SearchResult> _originResult;

        /// <summary>
        /// Backing field for <see cref="Result"/>
        /// </summary>
        private ObservableCollection<SearchResult> _result;

        /// <summary>
        /// Gets or sets the search result
        /// </summary>
        public ObservableCollection<SearchResult> Result
        {
            get => _result;
            private set => SetField(ref _result, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedResult"/>
        /// </summary>
        private SearchResult _selectedResult;

        /// <summary>
        /// Gets or sets the selected result
        /// </summary>
        public SearchResult SelectedResult
        {
            get => _selectedResult;
            set
            {
                SetField(ref _selectedResult, value);
                _sqlQuery = value?.Definition ?? "";
                _setSqlText(value?.Definition ?? "");

                if (value == null)
                {
                    ShowIndices = false;
                    ShowTable = false;
                    ShowDefinition = true;
                    TableColumns = new ObservableCollection<TableColumn>();
                    return;
                }

                if (value.IsTable)
                {
                    ShowIndices = value.Indices.Any();
                    ShowTable = true;
                    ShowDefinition = false;
                    TableColumns = new ObservableCollection<TableColumn>(value.Columns.OrderBy(o => o.ColumnPosition));
                }
                else
                {
                    ShowIndices = false;
                    ShowTable = false;
                    ShowDefinition = true;
                    TableColumns = new ObservableCollection<TableColumn>();
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="TypeList"/>
        /// </summary>
        private ObservableCollection<string> _typeList;

        /// <summary>
        /// Gets or sets the list with the types
        /// </summary>
        public ObservableCollection<string> TypeList
        {
            get => _typeList;
            private set => SetField(ref _typeList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedType"/>
        /// </summary>
        private string _selectedType;

        /// <summary>
        /// Gets or sets the selected type
        /// </summary>
        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetField(ref _selectedType, value) && !string.IsNullOrEmpty(value))
                    FilterResultList();
            }
        }

        /// <summary>
        /// Backing field for <see cref="SearchEnabled"/>
        /// </summary>
        private bool _searchEnabled = true;

        /// <summary>
        /// Gets or sets the value which indicates if the search is currently enabled
        /// </summary>
        public bool SearchEnabled
        {
            get => _searchEnabled;
            set => SetField(ref _searchEnabled, value);
        }

        /// <summary>
        /// Backing field for <see cref="ResultInfo"/>
        /// </summary>
        private string _resultInfo = "Result";

        /// <summary>
        /// Gets or sets the result info
        /// </summary>
        public string ResultInfo
        {
            get => _resultInfo;
            private set => SetField(ref _resultInfo, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowTable"/>
        /// </summary>
        private bool _showTable;

        /// <summary>
        /// Gets or sets the value which indicates if the selected entry is a "table" and the columns should be shown
        /// </summary>
        public bool ShowTable
        {
            get => _showTable;
            set => SetField(ref _showTable, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowDefinition"/>
        /// </summary>
        private bool _showDefinition = true;

        /// <summary>
        /// Gets or sets the value which indicates if the definition should be shown
        /// </summary>
        public bool ShowDefinition
        {
            get => _showDefinition;
            set => SetField(ref _showDefinition, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowIndices"/>
        /// </summary>
        private bool _showIndices;

        /// <summary>
        /// Gets or sets the value which indicates if the "show indices" button should be active
        /// </summary>
        public bool ShowIndices
        {
            get => _showIndices;
            set => SetField(ref _showIndices, value);
        }

        /// <summary>
        /// Backing field for <see cref="TableColumns"/>
        /// </summary>
        private ObservableCollection<TableColumn> _tableColumns = new();

        /// <summary>
        /// Gets or sets the columns of a table
        /// </summary>
        public ObservableCollection<TableColumn> TableColumns
        {
            get => _tableColumns;
            private set => SetField(ref _tableColumns, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedSaveAsTypeSearchResult"/>
        /// </summary>
        private TextValueItem _selectedSaveAsTypeSearchResult;

        /// <summary>
        /// Gets or sets the selected save as type for the export of the search result
        /// </summary>
        public TextValueItem SelectedSaveAsTypeSearchResult
        {
            get => _selectedSaveAsTypeSearchResult;
            set => SetField(ref _selectedSaveAsTypeSearchResult, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedCopyAsTypeSearchResult"/>
        /// </summary>
        private TextValueItem _selectedCopyAsTypeSearchResult;

        /// <summary>
        /// Gets or sets the selected copy as type for the export of the search result
        /// </summary>
        public TextValueItem SelectedCopyAsTypeSearchResult
        {
            get => _selectedCopyAsTypeSearchResult;
            set => SetField(ref _selectedCopyAsTypeSearchResult, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="setSqlText">The method to set the sql text</param>
        public void InitViewModel(Action<string> setSqlText)
        {
            _setSqlText = setSqlText;

            InitSaveCopyTypes();
        }

        /// <summary>
        /// Sets the connector
        /// </summary>
        /// <param name="connector">The connector</param>
        public void SetConnector(Connector connector)
        {
            _repo = new SearchRepo(connector);

            Clear();
        }

        /// <summary>
        /// The command to export the result
        /// </summary>
        public ICommand ExportCommand => new DelegateCommand(Export);

        /// <summary>
        /// The command to copy the preview to the clipboard
        /// </summary>
        public ICommand CopyCommand => new DelegateCommand(() =>
        {
            if (string.IsNullOrEmpty(_sqlQuery))
                return;

            Clipboard.SetText(_sqlQuery, TextDataFormat.Text);
        });

        /// <summary>
        /// The command to start the search
        /// </summary>
        public ICommand SearchCommand => new DelegateCommand(PerformSearch);

        /// <summary>
        /// The command to select all entries
        /// </summary>
        public ICommand SelectAllCommand => new DelegateCommand(() => SetSelection(true));

        /// <summary>
        /// The command to deselect all entries
        /// </summary>
        public ICommand DeselectAllCommand => new DelegateCommand(() => SetSelection(false));

        /// <summary>
        /// The command to save the table definition as table
        /// </summary>
        public ICommand SaveAsCommand => new DelegateCommand(SaveAs);

        /// <summary>
        /// The command to save the search result as table
        /// </summary>
        public ICommand SaveAsResultCommand => new DelegateCommand(SaveAsSearchResult);

        /// <summary>
        /// The command to copy the table to the clipboard
        /// </summary>
        public ICommand CopyAsCommand => new DelegateCommand(CopyAs);

        /// <summary>
        /// The command to copy the search result to the clipboard
        /// </summary>
        public ICommand CopyAsResultCommand => new DelegateCommand(CopyAsSearchResult);

        /// <summary>
        /// The command to show the list with the table indices
        /// </summary>
        public ICommand ShowTableIndicesCommand => new DelegateCommand(ShowTableIndices);

        /// <summary>
        /// Performs the search for the given value
        /// </summary>
        private async void PerformSearch()
        {
            if (string.IsNullOrEmpty(Search))
                return;

            SearchEnabled = false;
            var controller = await ShowProgressAsync("Search", $"Please wait while searching for '{Search}'...");

            try
            {
                _originResult = await _repo.SearchAsync(Search, MatchWholeWord);

                var typeList = new List<string> {"All"};
                typeList.AddRange(_originResult.Select(s => s.Type).Distinct());
                TypeList = new ObservableCollection<string>(typeList);
                SelectedType = typeList.FirstOrDefault(f => f.Equals("All"));

                FilterResultList();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex);
                ResultInfo = "Result - Error";
            }
            finally
            {
                SearchEnabled = true;
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Filters the result list
        /// </summary>
        private void FilterResultList()
        {
            var result = SelectedType.Equals("All")
                ? _originResult
                : _originResult.Where(w => w.Type.Equals(SelectedType)).ToList();

            var info = result.GroupBy(g => g.Type).Select(s => new {Type = s.Key, Count = s.Count()})
                .Select(s => $"{s.Type}: {s.Count}");

            ResultInfo = $"Result - Total: {result.Count} // {string.Join(" // ", info)}";

            Result = new ObservableCollection<SearchResult>(result.OrderByDescending(o => o.Type).ThenBy(t => t.Name));
        }

        /// <summary>
        /// Exports the result
        /// </summary>
        private async void Export()
        {
            if (Result == null || !Result.Any())
                return;

            var exportDir = Browse();
            if (string.IsNullOrEmpty(exportDir))
                return;

            var controller =
                await ShowProgressAsync("Export", "Please wait while exporting the data...");

            try
            {
                await Task.Run(() => ExportHelper.ExportSearchResult(exportDir, Result.Where(w => w.Export).ToList()));
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex);
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Browses for an export directory
        /// </summary>
        private static string Browse()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select the export directory"
            };

            return dialog.ShowDialog() != CommonFileDialogResult.Ok ? "" : dialog.FileName;
        }

        /// <summary>
        /// Saves the table information as table
        /// </summary>
        private void SaveAs()
        {
            if (SelectedSaveType is null)
                return;

            SaveValues((OutputType) SelectedSaveType.Id, TableColumns, $"{SelectedResult.Name}Table");
        }

        /// <summary>
        /// Saves the search result as formatted string
        /// </summary>
        private void SaveAsSearchResult()
        {
            if (_selectedSaveAsTypeSearchResult is null)
                return;

            SaveValues((OutputType) SelectedSaveAsTypeSearchResult.Id, Result.Where(w => w.Export),
                $"SearchResult_{Search.Replace(" ", "")}");
        }

        /// <summary>
        /// Copies the table to the clipboard
        /// </summary>
        private void CopyAs()
        {
            if (SelectedCopyType is null)
                return;

            var type = (OutputType)SelectedCopyType.Id;

            CopyValues(type, TableColumns);
        }

        /// <summary>
        /// Copies the search result as formatted string to the clipboard
        /// </summary>
        private void CopyAsSearchResult()
        {
            if (SelectedCopyAsTypeSearchResult is null)
                return;

            CopyValues((OutputType)SelectedCopyAsTypeSearchResult.Id, Result.Where(w => w.Export));
        }

        /// <summary>
        /// Selects all entries
        /// </summary>
        /// <param name="selected">true to select all entries, otherwise false</param>
        private void SetSelection(bool selected)
        {
            if (Result == null || !Result.Any())
                return;

            foreach (var entry in Result)
            {
                entry.Export = selected;
            }
        }

        /// <summary>
        /// Clears the content of the control
        /// </summary>
        public void Clear()
        {
            Result = new ObservableCollection<SearchResult>();
            SelectedResult = null;
            _setSqlText("");
        }

        /// <summary>
        /// Shows the list with the table indices
        /// </summary>
        private void ShowTableIndices()
        {
            if (SelectedResult == null || !SelectedResult.Indices.Any())
                return;

            var dialog = new TableIndexWindow(SelectedResult.Indices) {Owner = Application.Current.MainWindow};
            dialog.Show();
        }
    }
}
