using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using ForeScore.Models;
using ForeScore.Views;
using ForeScore;

using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ForeScore.ViewModels
{
    class CourseViewModel : BaseViewModel
    {

        // service and property vars
        private AzureService azureService;

        private ObservableCollection<Course> _Courses;


        // constructor
        public CourseViewModel()
        {
            Title = "Golf Courses";
            azureService = DependencyService.Get<AzureService>();
        }

        public ObservableCollection<Course> Courses
        {
            get { return _Courses; }
            set
            {
                _Courses = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadCoursesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            //List<Course> lst = await azureService.GetCourses();
            Courses = await azureService.GetCourses();
            Title = $"Courses ({Courses.Count})";
            IsBusy = false;
        }

        public async void OnDisplay()
        {
            base.OnAppearing();
            Debug.WriteLine("Loading courses...");
            await this.LoadCoursesAsync();

        }

        public async Task OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e == null) return; // has been set to null, do not 'process' tapped event
            //await Shell.Current.GoToAsync($"//player");


            // _navigation.PushAsync(new EditCoursePage((Course)e.SelectedItem));
            Debug.WriteLine("Course selected...");
        }

        public Command SelectionChangedCommand
        {
            get
            {
                return new Command(async (sender) =>
                {

                    IsBusy = true;
                    await Shell.Current.GoToAsync($"//player");
                    IsBusy = false;

                });
            }
        }
    }
}
