using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForeScore.ViewModels;
using Xamarin.Forms;

namespace ForeScore.Views
{
    public partial class ReportsPage : ContentPage
    {
        public ReportsPage()
        {
            InitializeComponent();
            ReportsViewModel vm = new ReportsViewModel(this.Navigation);
            BindingContext = vm;
        }

        private ReportsViewModel viewModel
        {
            get { return BindingContext as ReportsViewModel; }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.OnAppearing();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            viewModel.OnDisappearing();

        }


        void OnTournamentSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.OnTournamentSelected(sender, e);
        }

        void OnRoundSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            //viewModel.OnRoundSelected(sender, e);
            viewModel.RoundSelected(sender, e);
        }

        void OnPlayerSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            viewModel.PlayerSelected(sender, e);

        }
    }
}
