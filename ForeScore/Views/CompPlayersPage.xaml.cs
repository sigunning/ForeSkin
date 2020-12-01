using ForeScore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForeScore.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CompPlayersPage : ContentPage
    {
        public CompPlayersPage(Round round)
        {
            InitializeComponent();

            BindingContext = new CompPlayersViewModel();

            // assign passed society item to viewmodel Course
            viewModel.Round = round;

        }

        private CompPlayersViewModel viewModel
        {
            get { return BindingContext as CompPlayersViewModel; }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (viewModel == null)
                return;

            await viewModel.LoadData();

        }

    }
}