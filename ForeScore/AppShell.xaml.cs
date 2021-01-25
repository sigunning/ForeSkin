using System;
using System.Collections.Generic;
using System.Windows.Input;
using ForeScore.ViewModels;
using ForeScore.Views;
using Xamarin.Forms;

namespace ForeScore
{
    public partial class AppShell : Xamarin.Forms.Shell
    {

        

        public AppShell()
        {
            InitializeComponent();

            BindingContext = new BaseViewModel();

            RegisterRoutes();
        }

        private BaseViewModel viewModel
        {
            get { return BindingContext as BaseViewModel; }
        }

        private void RegisterRoutes()
        {
           //Routing.RegisterRoute("login", typeof(LoginPage));
           Routing.RegisterRoute($"//login", typeof(LoginPage));
 

        }

        
        // handle the hardware Back button
        protected override bool OnBackButtonPressed()
        {
            // if we are the Home page, allow exit
            string route = Shell.Current.CurrentItem.Route;
            if (route == "home") 
                return base.OnBackButtonPressed();

            // otherwise, either pop page if navigation stack is >1 or  or go to home page and stay in app
            Device.BeginInvokeOnMainThread(async () =>
            {
                // var result = await this.DisplayAlert("Alert!", "Exit Application?", "Yes", "No");
                if (Shell.Current.Navigation.NavigationStack.Count >1)
                    await Shell.Current.Navigation.PopAsync();
                else
                    await Shell.Current.GoToAsync($"//home");
            });

            return true;

        }
        

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//login");
            Shell.Current.FlyoutIsPresented = false;
            //await Shell.Current.Navigation.PushModalAsync(new LoginPage());
        }

    }
}
