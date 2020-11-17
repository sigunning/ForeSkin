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
    public partial class SocietyDetailPage : ContentPage
    {
        public SocietyDetailPage(Society society)
        {
            InitializeComponent();

            BindingContext = new SocietyDetailViewModel();

            // assign passed society item to viewmodel Course
            viewModel.Society = society;

            viewModel.IsNew = (society.SocietyName == null);
            Title = viewModel.IsNew ? "Add New Society" : "Edit Society";
        }

        private SocietyDetailViewModel viewModel
        {
            get { return BindingContext as SocietyDetailViewModel; }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (viewModel == null)
                return;

            await viewModel.ExecuteLoadLookupsCommand();

        }
    }
}