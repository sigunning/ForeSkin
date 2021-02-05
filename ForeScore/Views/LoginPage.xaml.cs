using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ForeScore.ViewModels;
using Xamarin.Essentials;

using System.Net.Http;
using Microsoft.Identity.Client;
using ForeScore.LogOn;
using ForeScore.Helpers;

namespace ForeScore.Views
{

	public partial class LoginPage : ContentPage
	{
        void Handle_Clicked(object sender, System.EventArgs e)
        {
           // Shell.CurrentShell.SendBackButtonPressed();
        }

        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel();
           
        }
        private LoginViewModel viewModel
        {
            get { return BindingContext as LoginViewModel; }
        }

        // override back button if not logged in
        protected override bool OnBackButtonPressed()
        {
            // can only go back to main page if authenticated and registered
            if (StaticHelpers.UserPlayer == null  || StaticHelpers.UserPlayer.PlayerId == null)
                return true;
            else
                return base.OnBackButtonPressed();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            //UpdateSignInState(StaticHelpers.UserPlayer);

            viewModel.Init();

            // check user loggedIn
            /*
            if (StaticHelpers.UserPlayer != null)
                UpdateSignInState(StaticHelpers.UserPlayer);
            else
                btnSignInSignOut.Text = "Sign in";
            */
        }


        async void OnSignInSignOut(object sender, EventArgs e)
        {
            try
            {
                if (btnSignInSignOut.Text == "Sign in")
                {
                    var userContext = await B2CAuthenticationService.Instance.SignInAsync();
                    UpdateSignInState(userContext);
                    UpdateUserInfo(userContext);
                    if (userContext.IsLoggedOn)
                    {

                        StaticHelpers.UserPlayer = userContext;
                        //await Shell.Current.Navigation.PopModalAsync();
                        await Shell.Current.GoToAsync($"//home");
                    }
                }
                else
                {
                    var userContext = await B2CAuthenticationService.Instance.SignOutAsync();
                    StaticHelpers.UserPlayer = null;
                    UpdateSignInState(userContext);
                    UpdateUserInfo(userContext);

                    
                }
            }
            catch (Exception ex)
            {
                // Checking the exception message 
                // should ONLY be done for B2C
                // reset and not any other error.
                if (ex.Message.Contains("AADB2C90118"))
                    OnPasswordReset();
                // Alert if any exception excluding user canceling sign-in dialog
                else if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
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
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
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
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
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
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        void UpdateSignInState(UserContext userContext)
        {
            bool isSignedIn;
            isSignedIn =  userContext==null ? false : userContext.IsLoggedOn ;

            btnSignInSignOut.Text = isSignedIn ? "Sign out" : "Sign in";
            btnCached.IsVisible = !isSignedIn;

            //Shell.SetNavBarIsVisible(this, isSignedIn);

            // Shell.SetFlyoutBehavior(this, isSignedIn ? FlyoutBehavior.Flyout : FlyoutBehavior.Disabled);
            /*
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