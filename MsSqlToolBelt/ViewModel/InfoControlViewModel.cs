using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects;
using ZimLabs.TableCreator;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic for the <see cref="View.InfoControl"/>
    /// </summary>
    internal sealed class InfoControlViewModel : ViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="LogDir"/>
        /// </summary>
        private string _logDir;

        /// <summary>
        /// Gets or sets the path of the log directory
        /// </summary>
        public string LogDir
        {
            get => _logDir;
            set => SetField(ref _logDir, value);
        }

        /// <summary>
        /// Backing field for <see cref="ReferenceList"/>
        /// </summary>
        private ObservableCollection<ReferenceEntry> _referenceList;

        /// <summary>
        /// Contains the list with the reference data
        /// </summary>
        public ObservableCollection<ReferenceEntry> ReferenceList
        {
            get => _referenceList;
            private set => SetField(ref _referenceList, value);
        }

        /// <summary>
        /// The command to open the log directory
        /// </summary>
        public ICommand OpenLogDirCommand => new DelegateCommand(OpenLogDirectory);

        /// <summary>
        /// The command to copy the table to the clipboard
        /// </summary>
        public ICommand CopyAsCommand => new DelegateCommand(CopyAs);

        /// <summary>
        /// The command to save the table definition as table
        /// </summary>
        public ICommand SaveAsCommand => new DelegateCommand(SaveAs);

        /// <summary>
        /// Loads and shows the data
        /// </summary>
        public async void InitViewModel()
        {
            try
            {
                var referenceList = PackageInfo.GetPackageInformation();

                ReferenceList = new ObservableCollection<ReferenceEntry>(referenceList);

                LogDir = Path.Combine(Helper.GetBaseFolder(), "logs");

                InitSaveCopyTypes();
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
        }

        /// <summary>
        /// Navigates to the log directory in the explorer
        /// </summary>
        private void OpenLogDirectory()
        {
            if (string.IsNullOrEmpty(LogDir))
                return;

            Helper.ShowInExplorer(LogDir);
        }

        /// <summary>
        /// Copies the table to the clipboard
        /// </summary>
        private void CopyAs()
        {
            if (SelectedCopyType is null)
                return;

            var type = (OutputType)SelectedCopyType.Id;

            CopyValues(type, ReferenceList);
        }

        /// <summary>
        /// Saves the table information as table
        /// </summary>
        private void SaveAs()
        {
            if (SelectedSaveType is null)
                return;

            SaveValues((OutputType)SelectedSaveType.Id, ReferenceList, "NugetPackages");
        }
    }
}
