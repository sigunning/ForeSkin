using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ForeScore.Models;
using ForeScore.ViewModels;

namespace ForeScore.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayerDetailPage : ContentPage
    {
        public PlayerDetailPage(Player player)
        {
            InitializeComponent();
            BindingContext = new PlayerDetailViewModel();

            // assign passed course item to viewmodel Course
            viewModel.Player = player;

            //Title = viewModel.Course.CourseName;
            Title = viewModel.Player.PlayerName ?? "Add New Player";

            viewModel.IsNew = (player.PlayerName == null);

        }

        private PlayerDetailViewModel viewModel
        {
            get { return BindingContext as PlayerDetailViewModel; }
        }

    }
}