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
            string ftpMsg = string.Empty;

            // set offline mode off
            Preferences.Set("OfflineMode", false);

            // sync to SQL
            await azureService.SyncAllData(SyncOptionsObj);

            if (SyncOptionsObj.Upload)
            {
                string host = "waws-prod-sn1-043.ftp.azurewebsites.windows.net";
                //host = "104.214.114.166";
                string user = await SecureStorage.GetAsync("ftpuser");
                //string user = @"breadmakersgolf\$breadmakersgolf";
                string password = await SecureStorage.GetAsync("ftppassword");
                //string password = "1PLYdfcahrjsmqXjjlJTTJz9iwkWc92pBaZb8GWGQ7GJYAZecRl4F4jmL8Nv";
                string source = azureService.DBPath;
                // "/data/user/0/com.sprinklerhead.foreskin/files/forescore.db"
                string dest = "/site/wwwroot/App_Data/forescore.db";
                FTP ftp = new FTP();
                FluentFTP.FtpStatus ftpStatus = await ftp.UploadFile(host, user, password, source, dest);
                ftpMsg = ftpStatus == FluentFTP.FtpStatus.Success ? " - Upload Ok" : " - Upload Failed";

            }

            Preferences.Set("OfflineMode", true);
            IsBusy = false;
            StatusMsg = "Synchronisation completed" + ftpMsg;
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
