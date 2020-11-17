
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace ForeScore.ViewModels
{
    class CompetitionDetailViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;
        private Competition _competition;
        public bool IsNew;



        public ICommand SaveCommand { private set; get; }

        // constructor
        public CompetitionDetailViewModel()
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
                    await azureService.SaveCompetitionAsync(_competition);
                    if (IsNew)
                    {
                        MessagingCenter.Send(_competition, "AddNew");
                    }
                    else
                    {
                        MessagingCenter.Send(_competition, "Update");
                    }

                    RefreshCanExecutes();
                    Debug.WriteLine("Competition saved: ");
                    await Shell.Current.Navigation.PopAsync();
                    IsBusy = false;
                },
                 canExecute: () =>
                 {
                     return (Competition != null); 
                 }
            );
        }

        public Competition Competition
        {
            get { return _competition; }
            set
            {
                _competition = value;
                OnPropertyChanged();
                (SaveCommand as Command).ChangeCanExecute();
            }
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
            bool isValid = (Competition != null && _competition.CompetitionName != null && _competition.CompetitionDescription != null);
            ValidationText = (isValid ? string.Empty : "Please enter Name and Description");
            return isValid;
        }

    }


        
}
