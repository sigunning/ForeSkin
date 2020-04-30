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
        public ResultsPage(List<PlayerRoundScore> lstPlayerRoundScores)
        {
            InitializeComponent();
            ResultsViewModel vm = new ResultsViewModel(this.Navigation);

            // pass list to view model
            vm.PlayerRoundScores = lstPlayerRoundScores;

            BindingContext = vm;
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

            viewModel.OnAppearing();

        }

    }
}
