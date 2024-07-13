
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Input;
using ForeScore.Models;
using ForeScore.Views;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Collections.Generic;

namespace ForeScore.ViewModels
{
    class CompetitionDetailViewModel : BaseViewModel
    {
        // service and property vars
        private AzureService azureService;
        private Competition _competition;
        public bool IsNew;

        

        public ICommand SaveCommand { private set; get; }
        public ICommand EditSocietyCommand { private set; get; }

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

            EditSocietyCommand = new Command(async () =>
            {
                if (Common.Pickers.PickerSociety.Count > 0)
                {
                    SocietiesPicker = Common.Pickers.PickerSociety.ToList();
                    // set society pref on dropdown  
                    string societyId = Preferences.Get("SocietyId", null);
                    if (societyId != null)
                    {
                        var society = SocietiesPicker.FirstOrDefault(o => o.SocietyId == societyId);
                        if (society != null)
                        {
                            SelectedSociety = society;
                            Debug.WriteLine("Set SelectedSociety in ExecuteLoadSocietiesCommand: " + SelectedSociety.SocietyName);
                        }
                    }
                    IsEditSociety = true;
                }
            });


        }

        private bool _isEditSociety;
        public bool IsEditSociety 
        {
            get { return _isEditSociety; }
            set
            {
                _isEditSociety = value;
                OnPropertyChanged();
            }
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


        // private backing vars for lists
        private List<Society> _societiesPicker;
        // Main source for the societies list
        public List<Society> SocietiesPicker
        {
            get { return _societiesPicker; }
            set
            {
                _societiesPicker = value;
                OnPropertyChanged();
            }
        }

        // property bound to picker selecteitem
        private Society _selectedSociety;
        public Society SelectedSociety
        {
            get { return _selectedSociety; }
            set
            {
                _selectedSociety = value;
                OnPropertyChanged();
                // update on Comp model
                Competition.SocietyId = value.SocietyId;
            }
        }
    }



}
