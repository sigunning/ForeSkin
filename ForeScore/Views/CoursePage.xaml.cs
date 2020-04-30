using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ForeScore.ViewModels;

namespace ForeScore.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CoursePage : ContentPage
	{
		public CoursePage ()
		{
			InitializeComponent ();
            BindingContext = new CourseViewModel();
        }

        private CourseViewModel viewModel
        {
            get { return BindingContext as CourseViewModel; }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (viewModel == null)
                return;

            viewModel.OnDisplay();

        }

        // search for string - convert to lowercase
        void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e) 
        {
            //    lstCourses.BeginRefresh();
           

                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                    lstCourses.ItemsSource = viewModel.Courses;
                else
                    lstCourses.ItemsSource = viewModel.Courses.Where(i => i.CourseName.ToLower().Contains(e.NewTextValue.ToLower()));

                //   lstCourses.EndRefresh();
          
        }

        async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

            if (viewModel == null || !viewModel.CanLoadMore || viewModel.IsBusy)
                return;

            await Navigation.PushAsync(new PlayerPage());
            //await viewModel.OnItemSelected(sender, e);

        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        async void courseview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.CurrentSelection;
            await Navigation.PushAsync(new PlayerPage());
        }
    }
}