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
   
    public partial class DiscardPage : ContentPage
    {
        public DiscardPage(Competition _competition)
        {
            InitializeComponent();

            ResultsViewModel vm = new ResultsViewModel();
            BindingContext = vm;

            viewModel.Competition = _competition;
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