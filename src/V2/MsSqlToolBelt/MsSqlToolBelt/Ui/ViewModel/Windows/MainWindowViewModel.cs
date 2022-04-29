using System;
using System.Windows.Input;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Windows
{
    internal class MainWindowViewModel : ViewModelBase
    {
        #region Actions

        /// <summary>
        /// The action to initialize the specified fly out
        /// </summary>
        private Action<FlyOutType>? _initFlyOut;

        /// <summary>
        /// The action to set the repo
        /// </summary>
        private Action<BaseRepo>? _setRepo;

        #endregion

        /// <summary>
        /// Backing field for <see cref="SettingsOpen"/>
        /// </summary>
        private bool _settingsOpen;

        /// <summary>
        /// Gets or sets the value which indicates if the settings fly out is open
        /// </summary>
        public bool SettingsOpen
        {
            get => _settingsOpen;
            set
            {
                SetField(ref _settingsOpen, value);

                switch (value)
                {
                    case true:
                        _initFlyOut?.Invoke(FlyOutType.Settings);
                        break;
                    case false:
                        //LoadServerList();
                        break;
                }
            }
        }

        /// <summary>
        /// The command to open the settings control
        /// </summary>
        public ICommand OpenSettingsCommand => new DelegateCommand(() => SettingsOpen = !SettingsOpen);

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="initFlyOut"></param>
        public void InitViewModel(Action<FlyOutType> initFlyOut)
        {
            _initFlyOut = initFlyOut;
        }
    }
}
