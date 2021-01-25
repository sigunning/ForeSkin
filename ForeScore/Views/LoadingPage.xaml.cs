using ShellLogin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPage : ContentPage
    {
        public LoadingPage()
        {
            InitializeComponent();
            BindingContext = new LoadingViewModel();
        }

        private LoadingViewModel viewModel
        {
            get { return BindingContext as LoadingViewModel; }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.Init();
		}
    }
}