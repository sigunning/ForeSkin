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
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Runtime.InteropServices;

namespace ForeScore.ViewModels
{
    class CourseViewModel : BaseViewModel
    {

        // service and property vars
        private AzureService azureService;

        private ObservableCollection<Course> _Courses;

        // course selected Command Interface called by control
        public ICommand SelectionCommand { private set; get; }
        public ICommand AddCommand { private set; get; }


        // works with the RefreshCommand and the IsRefreshing properties of the RefreshView.
        // Ensure that IsRefreshing is set to OneWay bind so it does not set the IsBusy flag.
        private Command refreshCommand;
        public Command RefreshCommand
        {
            get
            {
                return refreshCommand ?? (refreshCommand = new Command(async () => await ExecuteLoadCoursesCommand(), () =>
                {
                    return !IsBusy;
                }));
            }
        }

        


        // selected item 
        private Course _selection;
        public Course Selection
        {
            get { return _selection; }
            set { 
                _selection = value;
                OnPropertyChanged();
            }
        }

        // constructor
        public CourseViewModel()
        {

            azureService = DependencyService.Get<AzureService>();

            // implement the ICommands
            SelectionCommand = new Command<Course>(async ( item) =>
            {
                if (item == null)
                {
                    return;
                }
 
                IsBusy = true;
                Debug.WriteLine(string.Concat("Course selected is: ", item.CourseName) );
                await Shell.Current.Navigation.PushAsync(new CourseDetailPage(item));
                // un-select the list
                Selection = null;
                IsBusy = false;

            });

            AddCommand = new Command(async () =>
            {
                // create new course object with a guid id and pass to page
                Course course = new Course();
                course.CourseId = Guid.NewGuid().ToString();
                IsBusy = true;
                Debug.WriteLine("Add course screen... ");
                await Shell.Current.Navigation.PushAsync(new CourseDetailPage(course));
                IsBusy = false;

            });


            // subscribe to messaging so that updates in detail ViewModel can reflect back in the list
            SubscribeToMessageCenter();


        }

        private void SubscribeToMessageCenter()
        {
            MessagingCenter.Subscribe<Course>(this, "AddNew", async(course)  =>
            {
                Courses.Add(course);
                await azureService.SaveCourseAsync(course);
                Title = $"Courses ({Courses.Count})";
            });

            MessagingCenter.Subscribe<Course>(this, "Update", async (course) =>
            {
                await azureService.SaveCourseAsync(course);
            });
        }

        // Main source for the course list
        public ObservableCollection<Course> Courses
        {
            get { return _Courses; }
            set
            {
                _Courses = value;
                OnPropertyChanged();
                
            }
        }

        // Load the courses from data store. 
        private async Task ExecuteLoadCoursesCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            RefreshCommand.ChangeCanExecute();

            //DoStuff
            Courses = await azureService.GetCourses();
            Title = $"Courses ({Courses.Count})";

            IsBusy = false;
            RefreshCommand.ChangeCanExecute();
        }


        /*
        public async Task LoadCoursesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            //List<Course> lst = await azureService.GetCourses();
            Courses = await azureService.GetCourses();
            Title = $"Courses ({Courses.Count})";
            IsBusy = false;
        }
        */

        public async void OnDisplay()
        {
            base.OnAppearing();
            Debug.WriteLine("Loading courses...");
            await this.ExecuteLoadCoursesCommand();

        }

        

    }
}
