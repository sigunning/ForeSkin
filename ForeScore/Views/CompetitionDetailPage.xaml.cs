using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ForeScore.Models;
using ForeScore.ViewModels;

namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CompetitionDetailPage : ContentPage
    {
        public CompetitionDetailPage(Competition competition)
        {
            InitializeComponent();

            BindingContext = new CompetitionDetailViewModel();

            // assign passed society item to viewmodel Course
            viewModel.Competition = competition;

            Title = viewModel.Competition.CompetitionName ?? "Add New Competition";

            viewModel.IsNew = (competition.CompetitionName == null);

        }

        private CompetitionDetailViewModel viewModel
        {
            get { return BindingContext as CompetitionDetailViewModel; }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (viewModel == null)
                return;

            
        }


    }
}