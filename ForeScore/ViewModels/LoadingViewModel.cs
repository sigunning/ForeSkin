
using ForeScore.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;
using ForeScore.LogOn;
using ForeScore.Helpers;
using System;
using System.Diagnostics;
using ForeScore;
using ForeScore.Models;
using Xamarin.Essentials;

namespace ShellLogin.ViewModels
{
    class LoadingViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;

        public LoadingViewModel()
        {
            azureService = DependencyService.Get<AzureService>();

        }

        // Called by the views OnAppearing method
        public async void Init()
        {
            bool isAuthenticated=false ;
            bool isRegistered = false;
            try
            {
                // try to sign-in silently. Will error if not
                var userContext = await B2CAuthenticationService.Instance.AcquireTokenSilent();
                // so, we have signed in. Store user details to static object
                StaticHelpers.UserPlayer = userContext;
                isAuthenticated = true;

                // check if new user not yet registered. Will have a playerId if so.
                Player player = await azureService.GetPlayerByUserId(userContext.UserIdentifier);
                // store player id to static
                
                StaticHelpers.UserPlayer.PlayerId = player == null ? null : player.PlayerId;
                StaticHelpers.UserPlayer.AdminYN = player.AdminYN;
                // get home society
                var society = await azureService.GetHomeSociety(player.PlayerId);
                StaticHelpers.UserPlayer.HomeSocietyId = society.SocietyId;
                StaticHelpers.UserPlayer.DisplayName = player.PlayerName;
                isRegistered = (player.PlayerId != null);
                // store prefs
                Preferences.Set("UserId", userContext.UserIdentifier);
                Preferences.Set("PlayerId", player.PlayerId);
            }
            catch (Exception ex)
            { 
                //await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
                // Not authenticated
                Debug.WriteLine(ex.ToString());
            }

            


            //await Task.Delay(2000);
            if (isAuthenticated && isRegistered )
            {
                // all good, go to main page
                // await SetUserPlayer();
                await Shell.Current.GoToAsync($"//home");
            }
            else
            {
                // need to signin/up or register
                await Shell.Current.GoToAsync($"//login");
            }
        }
    }
}
