using System;
using System.Collections.ObjectModel;
using System.IO;
using ZimLabs.CoreLib;
using ZimLabs.CoreLib.NuGet;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic of the info window
    /// </summary>
    internal sealed class InfoWindowViewModel : ViewModelBase
    {
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
            set => SetField(ref _referenceList, value);
        }

        /// <summary>
        /// Loads and shows the data
        /// </summary>
        public async void Load()
        {
            try
            {
                var referenceList = NuGetHelper.GetPackageInformation(Path.Combine(Core.GetBaseFolder(), "packages.config"));

                ReferenceList = new ObservableCollection<ReferenceEntry>(referenceList);
            }
            catch (Exception ex)
            {
                await ShowMessage("Error",
                    $"An error has occured while loading the reference information.\r\n\r\nMessage: {ex.Message}");
            }
        }
    }
}
