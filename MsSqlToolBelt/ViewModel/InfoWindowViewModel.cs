using System;
using System.Collections.ObjectModel;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects;

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
            private set => SetField(ref _referenceList, value);
        }

        /// <summary>
        /// Loads and shows the data
        /// </summary>
        public async void Load()
        {
            try
            {
                var referenceList = PackageInfo.GetPackageInformation();

                ReferenceList = new ObservableCollection<ReferenceEntry>(referenceList);
            }
            catch (Exception ex)
            {
                await ShowMessage("Error",
                    $"An error has occurred while loading the reference information.\r\n\r\nMessage: {ex.Message}");
            }
        }
    }
}
