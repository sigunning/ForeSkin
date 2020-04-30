using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ForeScore.ViewModels;
using Xamarin.Essentials;

namespace ForeScore.Views
{
    public partial class MainPage : ContentPage
    {

        

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new BaseViewModel();
            
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

    }
}
