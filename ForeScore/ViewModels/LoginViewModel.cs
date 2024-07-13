using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Microsoft.Identity.Client;
using ForeScore.LogOn;
using ForeScore.Helpers;
using System.Diagnostics;
using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ForeScore.ViewModels
{
    class LoginViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;

        public ICommand SignInOutCommand {  set; get; }
        public ICommand SignInCachedCommand { set; get; }
        public ICommand RegisterCommand { set; get; }
        public ICommand EditProfileCommand { set; get; }


        public LoginViewModel()
        {

            azureService = DependencyService.Get<AzureService>();

            // for button SignInSignOut
            SignInOutCommand = new Command(async () =>
            {
               
                IsBusy = true;
                Debug.WriteLine("Sign In/out... ");
                await SignInSignOut();
                IsBusy = false;

            });

            // for button cached sign on
            SignInCachedCommand = new Command(async () =>
            {

                IsBusy = true;
                Debug.WriteLine("Sign In cached... ");
                UserContext userContext = new UserContext()
                {
                    IsLoggedOn = true,
                    UserIdentifier = Preferences.Get("UserId",null),
                    GivenName = Preferences.Get("GivenName", null)
                };
                UpdateSignInState(userContext);
                await Shell.Current.GoToAsync($"//home");
                IsBusy = false;

            });


            // for button SignInSignOut
            RegisterCommand = new Command<String>(async (string registrationCode) =>
            {

                IsBusy = true;
                Debug.WriteLine("Sign In/out... ");
                await Register(registrationCode);
                IsBusy = false;

            });

            EditProfileCommand = new Command(async () =>
            {

                IsBusy = true;
                Debug.WriteLine("Profile ");
                await EditProfile();
                // re-run signin
                IsSignedIn = false;
                await SignInSignOut();
                IsBusy = false;

            });

            // set connection mode
            SetMode();
        }

        public void Init()
        {
            // we could be in any of these states:
            // # Signed in and registered - arrived from Logout option [UserPlayer not null && UserPlayer.PlayerId not null]
            // # Signed in but not registered [UserPlayer not Null && UserPlayer.PlayerId = null]
            // # Not signed in or registered [UserPlayer = Null]

            UpdateSignInState(StaticHelpers.UserPlayer);
        }

        // binding properties

        // Signed in with B2C
        private bool _isSignedIn;
        public bool IsSignedIn { get => _isSignedIn; set => SetProperty(ref _isSignedIn, value) ; }

       
        private bool _isRegistered;
        public bool IsRegistered { get => _isRegistered; set => SetProperty(ref _isRegistered, value); }

        private string _signInSignOutText;
        public string SignInSignOutText { get => _signInSignOutText; set => SetProperty(ref _signInSignOutText, value); }

        // Registered as a player
        private bool _isNewPlayer;
        public bool IsNewPlayer { get => _isNewPlayer; set => SetProperty(ref _isNewPlayer, value); }

        private string _validationText;
        public string ValidationText { get => _validationText; set => SetProperty(ref _validationText, value); }


        private string _playerName;
        public string PlayerName { get => _playerName; set => SetProperty(ref _playerName, value); }


        // -------------------------------------------------------------------------

        private async Task<bool> Register(string playerId)
        {

            // download players
            SyncOptions sync = new SyncOptions() { Players = true };
            await azureService.SyncAllData(sync);

            // find the player code 
            Player player = await azureService.GetPlayer(playerId);

            // if re-installed app, player may be registered already
            if (player.RegisteredYN)
            {
                ValidationText = "You are already Registered!";
                StaticHelpers.UserPlayer.DisplayName = player.PlayerName;
                StaticHelpers.UserPlayer.UserIdentifier = player.userId;
                StaticHelpers.UserPlayer.AdminYN = player.AdminYN;
                // store prefs
                Preferences.Set("UserId", player.userId);
                Preferences.Set("PlayerId", player.PlayerId);

            }
            else
            {
                // if found and userid is empty, we can update
                if (player == null || player.userId != null)
                {
                    ValidationText = "Invalid Registration Code! Try Again";
                    return false;
                }

                // set playerid
                StaticHelpers.UserPlayer.PlayerId = player.PlayerId;

                IsBusy = true;

                // update
                ValidationText = "Registration Successful!";
                player.userId = StaticHelpers.UserPlayer.UserIdentifier;
                player.EmailAddress = StaticHelpers.UserPlayer.EmailAddress;
                player.PlayerName = StaticHelpers.UserPlayer.DisplayName;
                player.FirstName = StaticHelpers.UserPlayer.GivenName;
                player.LastName = StaticHelpers.UserPlayer.FamilyName;
                await azureService.SavePlayerAsync(player);



                // create home society
                Society society = new Society();
                society.SocietyId = Guid.NewGuid().ToString();
                society.SocietyName = StaticHelpers.UserPlayer.DisplayName;
                society.SocietyDescription = "General Play";
                society.CreatedByPlayerId = playerId;
                society.CreatedDate = DateTime.Now;

                // save record and then notify subscribers 
                await azureService.SaveSocietyAsync(society);


                // add current player as a member of society
                SocietyPlayer societyPlayer = new SocietyPlayer();
                societyPlayer.PlayerId = playerId;
                societyPlayer.SocietyId = society.SocietyId;
                societyPlayer.SocietyAdmin = true;
                societyPlayer.HomeYN = true;
                societyPlayer.JoinedDate = DateTime.Now;
                await azureService.SaveSocietyPlayerAsync(societyPlayer);

                // set prefs
                Preferences.Set("SocietyId", society.SocietyId);
                Preferences.Set("PlayerId", player.PlayerId);
                Preferences.Set("UserId", player.userId);

            }
            UpdateSignInState(StaticHelpers.UserPlayer);

            // go to main page
            await Shell.Current.GoToAsync($"//home");

            IsBusy = false;
            return true;
        }

        private async Task<bool> SignInSignOut()
        {
            IsBusy = true;
            try
            {
                if (!IsSignedIn)
                {
                    var userContext = await B2CAuthenticationService.Instance.SignInAsync();
                    
                    if (userContext.IsLoggedOn)
                    {
                        if ( await SetUserPreferences(userContext) )
                        {
                            // await SetUserPlayer();
                            // go to main page
                            await Shell.Current.GoToAsync($"//home");
                        }
                        else
                        {   
                            // must get reg code
                            UpdateSignInState(userContext);
                            
                        }

                    }
                }
                else
                {
                    var userContext = await B2CAuthenticationService.Instance.SignOutAsync();
                    // clear user object
                    StaticHelpers.UserPlayer = null;
                    UpdateSignInState(StaticHelpers.UserPlayer);
                }
                IsBusy = false;
                return true;
            }
            catch (Exception ex)
            {
                // Checking the exception message 
                // should ONLY be done for B2C
                // reset and not any other error.
                if (ex.Message.Contains("AADB2C90118"))
                {
                    OnPasswordReset();
                    
                }
                // Alert if any exception excluding user canceling sign-in dialog
                else if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                {
                    //await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
                    Debug.WriteLine(ex.ToString());
                    
                }
                IsBusy = false;
                return false;

            }
        }

        private async Task<bool> SetUserPreferences(UserContext userContext)
        {
            // maybe different user, so clear all preferences
            Preferences.Clear();

            StaticHelpers.UserPlayer = userContext;
            // store for cached login
            Preferences.Set("UserId", userContext.UserIdentifier);
            Preferences.Set("PlayerName", userContext.GivenName);
            // get PlayerId - if none, ask for code
            Player player = await azureService.GetPlayerByUserId(userContext.UserIdentifier);
            if (player != null)
            {
                // get home society
                var society = await azureService.GetHomeSociety(player.PlayerId);
                Preferences.Set("SocietyId", society.SocietyId);
                StaticHelpers.UserPlayer.HomeSocietyId = society.SocietyId;
                StaticHelpers.UserPlayer.PlayerId = player.PlayerId;
                StaticHelpers.UserPlayer.AdminYN = player.AdminYN;
                StaticHelpers.UserPlayer.DisplayName = player.PlayerName;
                Preferences.Set("PlayerId", player.PlayerId);
                return true;
            }
            return false;
        }

        /*
        async void OnCallApi(object sender, EventArgs e)
        {
            try
            {
                lblApi.Text = $"Calling API {App.ApiEndpoint}";
                var userContext = await B2CAuthenticationService.Instance.SignInAsync();
                var token = userContext.AccessToken;

                // Get data from API
                HttpClient client = new HttpClient();
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, App.ApiEndpoint);
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(message);
                string responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    lblApi.Text = $"Response from API {App.ApiEndpoint} | {responseString}";
                }
                else
                {
                    lblApi.Text = $"Error calling API {App.ApiEndpoint} | {responseString}";
                }
            }
            catch (MsalUiRequiredException ex)
            {
                await DisplayAlert($"Session has expired, please sign out and back in.", ex.ToString(), "Dismiss");
            }
            catch (Exception ex)
            {
                await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }
        */

        async Task<bool> EditProfile()
        {
            try
            {
                var userContext = await B2CAuthenticationService.Instance.EditProfileAsync();
                UpdateSignInState(userContext);
                UpdateUserInfo(userContext);
                return true;
            }
            catch (Exception ex)
            {
                // Alert if any exception excluding user canceling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    //await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
                    Debug.WriteLine(ex.ToString());
                return false;
            }
        }
        async void OnResetPassword(object sender, EventArgs e)
        {
            try
            {
                var userContext = await B2CAuthenticationService.Instance.ResetPasswordAsync();
                UpdateSignInState(userContext);
                UpdateUserInfo(userContext);
            }
            catch (Exception ex)
            {
                // Alert if any exception excluding user canceling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    //await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
                    Debug.WriteLine(ex.ToString());
            }
        }
        async void OnPasswordReset()
        {
            try
            {
                var userContext = await B2CAuthenticationService.Instance.ResetPasswordAsync();
                UpdateSignInState(userContext);
                UpdateUserInfo(userContext);
            }
            catch (Exception ex)
            {
                // Alert if any exception excluding user canceling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    //await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
                    Debug.WriteLine(ex.ToString());
            }
        }

        void UpdateSignInState(UserContext userContext)
        {
            // userContext is Static userPlayer
            // we could be in any of these states:
            // # Signed in and registered - arrived from Logout option [UserPlayer not null && UserPlayer.PlayerId not null]
            // # Signed in but not registered [UserPlayer not Null && UserPlayer.PlayerId = null]
            // # Not signed in or registered [UserPlayer = Null]


            IsSignedIn = userContext != null;

            IsNewPlayer = (IsSignedIn &&  userContext.PlayerId == null);

            SignInSignOutText = IsSignedIn ? "Sign out" : "Sign in";

            PlayerName = userContext==null ? "NOT SIGNED IN" : userContext.DisplayName?? "HELLO";

            ValidationText = string.Empty;

            
        }
        public void UpdateUserInfo(UserContext userContext)
        {
            
            

            /*
            lblName.Text = userContext.Name;
            lblGivenName.Text = userContext.GivenName;
            lblFamilyName.Text = userContext.FamilyName;
            lblUserID.Text = userContext.UserIdentifier;
            lblEmail.Text = userContext.EmailAddress;
            lblNewUser.Text = userContext.IsNewUser.ToString();
            */
        }

    }
}
