
using ForeScore.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;
using ForeScore.LogOn;
using ForeScore.Helpers;
using System;
using System.Diagnostics;

namespace ShellLogin.ViewModels
{
    class LoadingViewModel : BaseViewModel
    {
        

        public LoadingViewModel()
        {
           //
        }

        // Called by the views OnAppearing method
        public async void Init()
        {
            bool isAuthenticated=false ;
            try
            {
                var userContext = await B2CAuthenticationService.Instance.AcquireTokenSilent();
                StaticHelpers.UserPlayer = userContext;
                isAuthenticated = true;
            }
            catch (Exception ex)
            { 
                //await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
                Debug.WriteLine(ex.ToString());
            }



            
            //await Task.Delay(2000);
            if (isAuthenticated)
            {
                //await this.routingService.NavigateTo("///main");
                await Shell.Current.GoToAsync($"//home");
            }
            else
            {
                //await this.routingService.NavigateTo("///login");
                await Shell.Current.GoToAsync($"//login");
            }
        }
    }
}
