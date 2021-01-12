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
            ReportsViewModel vm = new ReportsViewModel();
            BindingContext = vm;
        }

        private ReportsViewModel viewModel
        {
            get { return BindingContext as ReportsViewModel; }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
           // if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
           //     return;
            viewModel.LoadData();
           

        }

    }
}
