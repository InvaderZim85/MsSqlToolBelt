using System;
using System.Collections.ObjectModel;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic for the <see cref="View.InfoControl"/>
    /// </summary>
    internal sealed class InfoControlViewModel : ViewModelBase
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
        public async void InitViewModel()
        {
            try
            {
                var referenceList = PackageInfo.GetPackageInformation();

                ReferenceList = new ObservableCollection<ReferenceEntry>(referenceList);
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
        }
    }
}
