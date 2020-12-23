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
    public partial class PlayerCardPage : ContentPage
    {
        public PlayerCardPage(PlayerScore ps)
        {
            InitializeComponent();

            BindingContext = new PlayerCardViewModel();
            
            // assign PlayerScore property in viewmodel
            viewModel.PlayerScore = ps;

        }

        private PlayerCardViewModel viewModel
        {
            get { return BindingContext as PlayerCardViewModel; }
        }


        protected override async void OnAppearing()
        {
            Debug.WriteLine("PlayerCardPage OnAppearing");
            base.OnAppearing();
            if (viewModel == null)
                return;

            await viewModel.LoadData();
            Title = viewModel.Title;
        }

    }
}