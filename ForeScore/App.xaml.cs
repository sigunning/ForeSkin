using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ForeScore.LogOn;


[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ForeScore
{
    public partial class App : Application
    {

        public static string ApiEndpoint = "https://fabrikamb2chello.azurewebsites.net/hello";


        public App()
        {
            InitializeComponent();
            

            /* NOTE on Dependency Injection in Xamarin:
             * 
             * 'B2CAuthenticationService' implements the 'IAuthenticationService' interface. 
             * Using the DependencyService we can register the 'B2CAuthenticationService' such 
             * that when we ask for an instance of the 'IAuthenticationService' like this:
             * 
             *      var authenticationService = DependencyService.Get<IAuthenticationService>();
             * 
             * it allows us to grab the instance of the B2CAuthenticationService that we register in the line below:
             * 
             * */
            DependencyService.Register<B2CAuthenticationService>();


            // set up Navigation
            MainPage = new ForeScore.AppShell();
            //MainPage = new ForeScore.Views.MainPage();
           
        }
      

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        
    }
}
