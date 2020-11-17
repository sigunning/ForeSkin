
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;
using Xamarin.Essentials;
using ForeScore.Common;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ForeScore.ViewModels
{
    class RoundDetailViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;
        private Round _round;
        public bool IsNew;


        public ICommand SaveCommand { private set; get; }

        public RoundDetailViewModel()
        {

            azureService = DependencyService.Get<AzureService>();

            // implement the ICommands
            SaveCommand = new Command(
                execute: async () =>

                {
                    IsBusy = true;

                    if (!Validate())
                        return;
                    // save record and then notify subscribers 
                    await azureService.SaveRoundAsync(_round);
                    if (IsNew)
                    {
                        MessagingCenter.Send(_round, "AddNew");
                    }
                    else
                    {
                        MessagingCenter.Send(_round, "Update");
                    }

                    RefreshCanExecutes();
                    Debug.WriteLine("Round saved: ");
                    await Shell.Current.Navigation.PopAsync();
                    IsBusy = false;
                },
                 canExecute: () =>
                 {
                     return (Round != null); 
                 }
            );
        }


        private void RefreshCanExecutes()
        {
            (SaveCommand as Command).ChangeCanExecute();
        }


        private string _validationText;
        public string ValidationText
        {
            get { return _validationText; }
            set
            {
                _validationText = value;
                OnPropertyChanged(); ;
            }
        }


        private bool Validate()
        {
            bool isValid = (Round != null && Round.RoundDate != null && Round.CourseId != null);
            ValidationText = (isValid ? string.Empty : "Please enter Course and Date");
            return isValid;
        }

        public Round Round
        {
            get { return _round; }
            set
            {
                _round = value;
                OnPropertyChanged();
                (SaveCommand as Command).ChangeCanExecute();
            }
        }

        // course lookup picker
        private List<CourseLookup> _coursePicker;
        public List<CourseLookup> CoursePicker
        {
            get { return _coursePicker; }
            set
            {
                _coursePicker = value;
                OnPropertyChanged();
            }
        }

        // course lookup picker
        private CourseLookup _selectedCourse;
        public CourseLookup SelectedCourse
        {
            get { return _selectedCourse; }
            set
            {
                _selectedCourse = value;
                Round.CourseId = value.CourseId;
                OnPropertyChanged();
            }
        }


        public async Task LoadData()
        {
           // load the lookups and picker lists
            await azureService.LoadCourseLookup();
            await azureService.LoadCompetitionLookup(Preferences.Get("SocietyId", string.Empty ));


            // set up  picker list
            CoursePicker = Common.Pickers.PickerCourse.ToList();
            // set course picker to current course, if editing
            if (Round.CourseId !=null )
                SelectedCourse = CoursePicker.FirstOrDefault(o => o.CourseId == Round.CourseId.ToString());
            
        }


    }
}
