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

namespace ForeScore.ViewModels
{
    class LoginViewModel : BaseViewModel
    {

        public ICommand SignInCommand {  set; get; }
        public ICommand LogoutCommand  {  set; get; }


        public LoginViewModel()
        {
            // constructor
            SignInCommand = new Command(async () =>
            {
               
                IsBusy = true;
                Debug.WriteLine("Sign In... ");
                await SignInSignOut(true);
                IsBusy = false;

            });

            LogoutCommand = new Command(async () =>
            {

                IsBusy = true;
                Debug.WriteLine("Sign Out... ");
                await SignInSignOut(false);
                IsBusy = false;

            });


            SetMode();
        }

        public async Task ExecuteLogout()
        {
            await SignInSignOut(false);
            
        }

        private bool _isSignedIn;
        public bool IsSignedIn { get => _isSignedIn; set => SetProperty(ref _isSignedIn, value) ; }



        private UserContext _userContext;
        public  UserContext UserContext
        {
            get { return _userContext; }
            set
            {
                _userContext = value;
                OnPropertyChanged();
                // trigger bindings in UI
                IsSignedIn = _userContext == null ? false : _userContext.IsLoggedOn;
                SetShell(IsSignedIn);
            }
        }

        private string _signInSignOutText="Sign in";
        public string SignInSignOutText { get => _signInSignOutText; set => SetProperty(ref _signInSignOutText, value); }

        private string _flyoutBehaviour;
        public string FlyoutBehaviour { get => _flyoutBehaviour; set => SetProperty(ref _flyoutBehaviour, value); }

        private string _greeting = "Flyout";
        public string Greeting { get => _greeting; set => SetProperty(ref _greeting, value); }
        

        private void SetShell(bool isSignedIn)
        {
            IsSignedIn = isSignedIn;
            SignInSignOutText = isSignedIn ? "Sign out" : "Sign in";
            FlyoutBehaviour = isSignedIn ? "Flyout" : "Disabled";
            Greeting = string.Concat("Hi ", isSignedIn ? UserContext.Name : "Guest" );
            
        }
        
        // -------------------------------------------------------------------------


        public async void CheckSignIn()
        {
            // if we have no connection, use local copy

            if (ConnectedMode)
            {
                // if UserContext not already set...
                if (UserContext == null)
                    UserContext = StaticHelpers.UserPlayer;
            }
            else
            {
                // use local context

            }



        }

        private async Task<bool> SignInSignOut(bool blnSignIn)
        {
            IsBusy = true;
            try
            {
                if (blnSignIn)
                {
                    var userContext = await B2CAuthenticationService.Instance.SignInAsync();
                    UpdateSignInState(userContext);
                    UpdateUserInfo(userContext);
                  
                }
                else
                {
                    var userContext = await B2CAuthenticationService.Instance.SignOutAsync();
                    UpdateSignInState(userContext);
                    UpdateUserInfo(userContext);
                    
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

        async void OnEditProfile(object sender, EventArgs e)
        {
            try
            {
                var userContext = await B2CAuthenticationService.Instance.EditProfileAsync();
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
            UserContext = userContext;
            StaticHelpers.UserPlayer = userContext;

            /*
            var isSignedIn = userContext == null ? false : userContext.IsLoggedOn;

            SignInSignOutText = isSignedIn ? "Sign out" : "Sign in";

            SignInSignOutText.IsVisible = !isSignedIn;
            btnPlay.IsVisible = isSignedIn;
            Shell.SetNavBarIsVisible(this, isSignedIn);

            Shell.SetFlyoutBehavior(this, isSignedIn ? FlyoutBehavior.Flyout : FlyoutBehavior.Disabled);
            
            btnEditProfile.IsVisible = isSignedIn;
            btnCallApi.IsVisible = isSignedIn;
            slUser.IsVisible = isSignedIn;
            lblApi.Text = "";
            */
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
