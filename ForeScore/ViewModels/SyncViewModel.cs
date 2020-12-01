using System.Text;
using ForeScore.Models;
using ForeScore.Views;
using ForeScore;

using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Essentials;
using ForeScore.Helpers;
using System.Threading.Tasks;

namespace ForeScore.ViewModels
{
    class SyncViewModel : BaseViewModel
    {

        // service and property vars
        private AzureService azureService;

        public ICommand SyncCommand { private set; get; }
        

        public SyncViewModel()
        {
            Title = "Sync Data to Cloud";

            azureService = DependencyService.Get<AzureService>();
            // init synch options object
            SyncOptionsObj = new SyncOptions();

            // set sync command
            SyncCommand = new Command(async () => await ExecuteSyncDataCommand());

        }

        async Task  ExecuteSyncDataCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            StatusMsg = "Synchronisation in progress...";
            // set offline mode off
            Preferences.Set("OfflineMode", false);

            await azureService.SyncAllData(SyncOptionsObj);

            Preferences.Set("OfflineMode", true);
            IsBusy = false;
            StatusMsg = "Synchronisation completed";
        }


        // data sync options
        private SyncOptions _syncOptions;
        public SyncOptions SyncOptionsObj
        {
            get { return _syncOptions; }
            set 
            {   
                SetProperty(ref _syncOptions, value);
                OnPropertyChanged();
            }
        }


        private string _statusMsg;
        public string StatusMsg
        {
            get { return _statusMsg; }
            set { SetProperty(ref _statusMsg, value); }
        }


        
    }
}
