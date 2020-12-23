using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForeScore.ViewModels;
using ForeScore.Models;
using Xamarin.Forms;

namespace ForeScore.Views
{
    public partial class ResultsPage : ContentPage
    {
        public ResultsPage()
        {
            InitializeComponent();
            BindingContext = new ResultsViewModel();

            // pass list to view model
            // vm.PlayerRoundScores = lstPlayerRoundScores;

            
        }

        private ResultsViewModel viewModel
        {
            get { return BindingContext as ResultsViewModel; }
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.LoadData();

        }

    }
}
