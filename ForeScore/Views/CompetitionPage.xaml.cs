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
    public partial class CompetitionPage : ContentPage
    {
        public CompetitionPage()
        {
            InitializeComponent();
            BindingContext = new CompetitionViewModel();

        }

        private CompetitionViewModel viewModel
        {
            get { return BindingContext as CompetitionViewModel; }
        }


        protected override async void OnAppearing()
        {
            Debug.WriteLine("CompetitionPage OnAppearing");
            base.OnAppearing();
            if (viewModel == null)
                return;

            await viewModel.LoadData();

        }
    }
}