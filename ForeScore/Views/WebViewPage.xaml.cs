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
    public partial class WebViewPage : ContentPage
    {
        string _url;

        public WebViewPage()
        {
            InitializeComponent();

        }

        void webOnNavigating(object sender, WebNavigatingEventArgs e)
        {
            progress.IsVisible = true;
            _url = e.Url;
        }
        void webOnEndNavigating(object sender, WebNavigatedEventArgs e)
        {
            progress.IsVisible = false;
            string[] aUrl = _url.Split('=');
            // enable when we get to course page
            btnApply.IsEnabled = (aUrl[0].ToLower().EndsWith("cid"));
        }

        private async void btnApply_Clicked(object sender, EventArgs e)
        {
            // get url and split. 
            try
            {
                // parse url to get CID id number
                string[] aUrl = _url.Split('=');

                if (aUrl[0].ToLower().EndsWith("cid"))
                {
                    // update course detail
                    await Shell.Current.Navigation.PopAsync();
                    MessagingCenter.Send(aUrl[1], "mscorecard");
                }
                else
                {
                    await DisplayAlert("Error", "This does not look like the correct page!", "Ok");
                }
            }
            catch
            {
                await DisplayAlert("Error", "Could not get course data id", "ok");
            }
        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopAsync();
        }
    }
}