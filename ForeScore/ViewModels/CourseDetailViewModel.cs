using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;

namespace ForeScore.ViewModels
{
    class CourseDetailViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;
        private Course _course;
        public bool IsNew;

        public ICommand SaveCommand { private set; get; }

        public List<int> ParList { get; set; }
        public List<int> SIList { get; set; }

        // constructor
        public CourseDetailViewModel()
        {
            //Title = Course.CourseName?? "Add New Course";
            azureService = DependencyService.Get<AzureService>();

            // implement the ICommands
            SaveCommand = new Command( async()  =>
            {
                IsBusy = true;
                if (IsNew)
                {
                    MessagingCenter.Send(_course, "AddNew");
                }
                else
                { 
                    MessagingCenter.Send(_course, "Update"); 
                }

                Debug.WriteLine("Course saved: ");
                await Shell.Current.Navigation.PopAsync();
                IsBusy = false;

            });

            // set up PAR picker list
            ParList= Common.Pickers.PickerPar.ToList();
            //ParList = new List<int>(new int[]  { 3, 4, 5 });
            // set up SI picker list
            //int[] sequence = Enumerable.Range(1, 18).ToArray();
            SIList = Common.Pickers.Picker18.ToList();
            //SIList = new List<int>(sequence);




        }

       


        public Course Course
        {
            get { return _course; }
            set
            {
                _course = value;
                OnPropertyChanged();
            }
        }
    }

   
}
