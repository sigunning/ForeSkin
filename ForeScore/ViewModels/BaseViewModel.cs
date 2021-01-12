using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ForeScore.Helpers;
using ForeScore.Views;
using ForeScore.Models;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace ForeScore.ViewModels
{
    class BaseViewModel : INotifyPropertyChanged
    {
        // service and property vars
        private AzureService azureService;

        public ICommand PlayCommand { private set; get; }

        public BaseViewModel()
        {
            //SetMode();
            azureService = DependencyService.Get<AzureService>();

            // #### temp user setting
            // PlayerId='867DC91B-BFBE-48BC-9A8D-1F133ADD4A6D'
            Preferences.Set("UserId", "867DC91B-BFBE-48BC-9A8D-1F133ADD4A6D");
            Preferences.Set("PlayerId", "867DC91B-BFBE-48BC-9A8D-1F133ADD4A6D");
            

            // set current player
            SetUserPlayer();

            // Go for it!
            PlayCommand = new Command(async () =>
            {
                IsBusy = true;
                await Shell.Current.Navigation.PushAsync(new SocietyCompPage());
                IsBusy = false;

            });
        }

        private string title = string.Empty;
        public const string TitlePropertyName = "Title";


        /// <summary>
        /// Gets or sets the "Title" property
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get { return title; }
            set
            {
                // check connectivity status and display
                //string sConnect = ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode)) ? " *" : "" ;
                //SetProperty(ref title, string.Concat(value, sConnect) );
                SetProperty(ref title, value);
            }
        }

        


        //---------------------------------------------------------------------------------------
        private string connectedIcon = null;
        /// Gets or sets the "Connected Icon" of the viewmodel
        public string ConnectedIcon
        {
            get { return connectedIcon; }
            set { SetProperty(ref connectedIcon, value); }
        }

        private bool connectedMode = false;
        /// Gets or sets the "Connected Icon" of the viewmodel
        public bool ConnectedMode
        {
            get { return connectedMode; }
            set { SetProperty(ref connectedMode, value); }
        }
        private string _connectedMsg;
        public string ConnectedMsg
        {
            get { return _connectedMsg; }
            set { SetProperty(ref _connectedMsg, value); }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(
            ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {


            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;

            if (onChanged != null)
                onChanged();

            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        protected virtual void OnAppearing()
        {
            // set connectivity icon property for view binding
            SetMode();
            

        }

        //---------------------------------------------------------------------------------------
        // Get the current user Player details
        //---------------------------------------------------------------------------------------
        private Player _userPlayer;
        /// Gets or sets the current user Player
        public Player UserPlayer
        {
            get { return _userPlayer; }
            set 
            { 
                _userPlayer = value;
                OnPropertyChanged(nameof(UserPlayer));
            }
        }

        private async Task SetUserPlayer()
        {
            UserPlayer = await azureService.GetPlayer(Preferences.Get("PlayerId", null));
        }
        //---------------------------------------------------------------------------------------

        public void SetMode()
        {
            //we may be in offline mode by design...or have no signal

            
            ConnectedIcon = (Connectivity.NetworkAccess == NetworkAccess.None) ? MaterialIcon.CloudOff : MaterialIcon.CloudDone;
            ConnectedMode = (Connectivity.NetworkAccess == NetworkAccess.None) ? false : true;
            ConnectedMsg = (Connectivity.NetworkAccess == NetworkAccess.None) ? "No Connection!" : "Connected";

            /*
            if (Settings.OfflineMode)
                ConnectedIcon = "ic_action_cloud_off.png";
            else
                ConnectedIcon = ((!CrossConnectivity.Current.IsConnected)) ? "ic_action_signal_cellular_connected_no_internet_4_bar.png" : "ic_action_cloud_done.png";

            ConnectedMode = ((!CrossConnectivity.Current.IsConnected) || (Settings.OfflineMode)) ? "Offline" : "Online";
            */
        }


        private bool _isOfflineMode = false;
        public bool IsOfflineMode
        {
            get { return _isOfflineMode; }
            set
            {
                _isOfflineMode = value;
                OnPropertyChanged(nameof(IsOfflineMode));
                Preferences.Set("OfflineMode", _isOfflineMode);
            }
        }



        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>

        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy == value)
                    return;

                if (SetProperty(ref isBusy, value))
                {
                    OnPropertyChanged("IsBusy");
                    IsNotBusy = !isBusy;
                }
            }
        }


        private bool canLoadMore = true;
        /// <summary>
        /// Gets or sets if we can load more.
        /// </summary>
        public const string CanLoadMorePropertyName = "CanLoadMore";
        public bool CanLoadMore
        {
            get { return canLoadMore; }
            set { SetProperty(ref canLoadMore, value); }
        }


        // activity indicator message
        public string ActivityMessage
        {
            get { return "Please Wait..."; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is not busy.
        /// </summary>
        /// <value><c>true</c> if this instance is not busy; otherwise, <c>false</c>.</value>

        bool isNotBusy = true;
        public bool IsNotBusy
        {
            get { return isNotBusy; }
            private set { SetProperty(ref isNotBusy, value); }
        }

        public ImageSource SplashSource
        {
            get
            {
                //string resource = "ForeScore.Resources.pinscribe-splash-gn.png";
                string resource = "ForeScore.Resources.splash.jpg";
                return ImageSource.FromResource(resource);
            }
        }
        public ImageSource LogoSource
        {
            get
            {
                string resource = "ForeScore.Resources.logo.png";
                return ImageSource.FromResource(resource);
            }
        }

        // sync data
        /*
        public Command SynchroniseCommand
        {
            get
            {
                return new Command(async (sender) =>
                {
                    
                            IsBusy = true;
                            await azureService.SyncAllData();
                            IsBusy = false;
                       
                });
            }
        }
        */

        // sync data
        public Command ToggleOfflineModeCommand
        {
            get
            {
                return new Command( (sender) =>
                {
                    Preferences.Set("ResetData", false);
                    IsOfflineMode = (!IsOfflineMode);
                    Preferences.Set("OfflineMode", false);
                });
            }
        }


        

    }
}
