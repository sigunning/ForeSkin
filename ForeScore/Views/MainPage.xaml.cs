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
    public partial class MainPage : ContentPage
    {

        //private bool isSignedIn;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new BaseViewModel();

            // start in offline mode
            Preferences.Set("OfflineMode", true);
        }
        private BaseViewModel viewModel
        {
            get { return BindingContext as BaseViewModel; }
        }

        


        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await this.DisplayAlert("Alert!", "Exit Application?", "Yes", "No");
                if (result) await this.Navigation.PopAsync();
            });
            //return base.OnBackButtonPressed();
            return true;
        }


        // -------------------------------------------------------------------------
        //  TEMP HOME FOR SIGNUPIN
        // -------------------------------------------------------------------------

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //SignIn();
            // only open Login page when no valid login
            // btnPlay.IsVisible = isSignedIn;

            // check user loggedIn
            // viewModel.CheckSignIn();
        }

        
        

        
    }
}
