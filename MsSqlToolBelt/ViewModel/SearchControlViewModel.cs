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
using MsSqlToolBelt.DataObjects.Search;
using ZimLabs.Database.MsSql;
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
            set => SetField(ref _typeList, value);
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
        /// Init the view model
        /// </summary>
        /// <param name="setSqlText">The method to set the sql text</param>
        public void InitViewModel(Action<string> setSqlText)
        {
            _setSqlText = setSqlText;
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
        /// Performs the search for the given value
        /// </summary>
        private async void PerformSearch()
        {
            if (string.IsNullOrEmpty(Search))
                return;
            SearchEnabled = false;
            var controller = await ShowProgress("Search", "Please wait while searching...");

            try
            {
                _originResult = await Task.Run(() => _repo.Search(Search, MatchWholeWord));

                var typeList = new List<string> {"All"};
                typeList.AddRange(_originResult.Select(s => s.Type).Distinct());
                TypeList = new ObservableCollection<string>(typeList);
                SelectedType = typeList.FirstOrDefault(f => f.Equals("All"));

                FilterResultList();
            }
            catch (Exception ex)
            {
                await ShowError(ex);
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

            ResultInfo = $"Result - Total: {result.Count} // {string.Join("//", info)}";

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
                await ShowProgress("Export", "Please wait while exporting the data...");

            try
            {
                await Task.Run(() => ExportHelper.ExportSearchResult(exportDir, Result.Where(w => w.Export).ToList()));
            }
            catch (Exception ex)
            {
                await ShowMessage("Error", $"An error has occured: {ex.Message}");
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Browses for an export directory
        /// </summary>
        private string Browse()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select the export directory"
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return "";

            return dialog.FileName;
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
    }
}
