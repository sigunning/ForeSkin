using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ForeScore.Models;
using ForeScore.ViewModels;
using System.Diagnostics;

namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SocietyCompPage : ContentPage
    {
        public SocietyCompPage()
        {
            InitializeComponent();
            BindingContext = new SocietyCompViewModel();

        }

        private SocietyCompViewModel viewModel
        {
            get { return BindingContext as SocietyCompViewModel; }
        }
  
        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CompPlayersPage());
        }

        protected override async void OnAppearing()
        {
            Debug.WriteLine("SocietyCompPage OnAppearing");
            base.OnAppearing();
            if (viewModel == null)
                return;
        
            await viewModel.LoadData();

        }
    }
}