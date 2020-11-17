using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ForeScore.Models;
using ForeScore.ViewModels;

namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RoundDetailPage : ContentPage
    {
        public RoundDetailPage(Round round)
        {
            InitializeComponent();

            BindingContext = new RoundDetailViewModel();

            // assign passed society item to viewmodel Course
            viewModel.Round = round;

            viewModel.IsNew = (round.CourseId == null);

            Title = viewModel.IsNew ? "Add New Round" : "Edit Round";

        }

        private RoundDetailViewModel viewModel
        {
            get { return BindingContext as RoundDetailViewModel; }
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