using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KeyVaultPage : ContentPage
    {
        public KeyVaultPage()
        {
            InitializeComponent();
        }

        private async void GetValueButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var token = await SecureStorage.GetAsync(KeyName.Text.ToLower());
                KeyValue.Text = token;
                if (token == null)
                    lblMsg.Text = "Key not found!";
                else
                    lblMsg.Text = "Value Retrieved";
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error! "+ ex.InnerException;
            }
        }

        private async void StoreValueButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (KeyValue.Text != string.Empty)
                {
                    await SecureStorage.SetAsync(KeyName.Text.ToLower(), KeyValue.Text);
                    lblMsg.Text = "Value Stored";
                }
                else
                {
                    lblMsg.Text = "No Value Entered";
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error! " + ex.InnerException;
            }
        }

        private void RemoveKeyButton_Clicked(object sender, EventArgs e)
        {
            SecureStorage.Remove(KeyName.Text.ToLower());
            lblMsg.Text = "Key Removed";
            
        }
    }
}