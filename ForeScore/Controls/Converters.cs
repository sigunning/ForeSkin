using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Runtime.CompilerServices;
using MultiBinding;
using System.Collections;
using ForeScore.Helpers;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using ForeScore.Models;

namespace ForeScore.Common
{

    public static class Lookups
    {
        public static Dictionary<string, string> _dictCourses = new Dictionary<string, string>();
        public static Dictionary<string, string> _dictCompetitions = new Dictionary<string, string>();
        public static Dictionary<string, string> _dictSocieties = new Dictionary<string, string>();
        public static Dictionary<string, string> _dictTournaments = new Dictionary<string, string>();
        public static Dictionary<string, string> _dictRounds = new Dictionary<string, string>();
        public static Dictionary<string, string> _dictPlayers = new Dictionary<string, string>();

        public static string GetLookup(string lookupType, string value)
        {
            try
            {
                string retval = string.Empty;
                lookupType = lookupType.ToLower();
              
                switch (lookupType)
                {
                    case "courses":
                        retval = _dictCourses[value];
                        break;
                    case "competitions":
                        retval = _dictCompetitions[value];
                        break;



                }

                return retval;
            }
            catch
            {
                return string.Empty;
            }

        }

    }

    

    public static class Pickers
    {
        // dropdown values
        public static ObservableCollection<int> Picker18 = new ObservableCollection<int>()
            {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18 };

        public static ObservableCollection<int> PickerScore = new ObservableCollection<int>()
            {1,2,3,4,5,6,7,8,9,10 };

        public static ObservableCollection<int> PickerPar = new ObservableCollection<int>()
            {3,4,5 };


        // cut down lists for picker source
        public static List<Society> PickerSociety = new List<Society>();
        public static List<CourseLookup> PickerCourse = new List<CourseLookup>();
        public static List<Competition> PickerCompetition = new List<Competition>();

    }

    public class MyNumericColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string htmlColorCode = value.ToString();

            Xamarin.Forms.Color col = Xamarin.Forms.Color.FromHex(htmlColorCode);
            return col;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }



    public class CourseNameConverter : IValueConverter
    {
        

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string courseName;
            try
            {
                courseName = Lookups._dictCourses[value.ToString()];
            }
            catch
            {
                courseName = "Not Found";
            }

            return courseName;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class PlayerNameConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string playerName;
            try
            {
                playerName = Lookups._dictPlayers[value.ToString()];
            }
            catch
            {
                playerName = "Not Found";
            }

            return playerName;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class MarkerConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool IsMarker;
            try
            {
                IsMarker = ( value.Equals(Preferences.Get("UserId", string.Empty)) );
            }
            catch
            {
                IsMarker = false;
            }

            return IsMarker;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string marker_id;
            try
            {
                marker_id = ( (bool)value==true ? Preferences.Get("UserId", string.Empty) : string.Empty);
            }
            catch
            {
                marker_id = null;
            }

            return marker_id;
        }

    }



    public class DictionaryItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null && values.Length >= 2)
            {
                var myDict = values[0] as IDictionary;
                if (values[1] is string)
                {
                    var myKey = values[1] as string;
                    if (myDict != null && myKey != null)
                    {
                        //the automatic conversion from Uri to string doesn't work
                        //return myDict[myKey];
                        return myDict[myKey].ToString();
                    }
                }
                else
                {
                    long? myKey = values[1] as long?;
                    if (myDict != null && myKey != null)
                    {
                        //the automatic conversion from Uri to string doesn't work
                        //return myDict[myKey];
                        return myDict[myKey].ToString();
                    }
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public interface IMultiValueConverter
    {
        object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
    }


    [ContentProperty(nameof(Bindings))]
    public class MultiBinding : IMarkupExtension<Binding>
    {
        

        private BindableObject _target;
        private readonly InternalValue _internalValue = new InternalValue();
        private readonly IList<BindableProperty> _properties = new List<BindableProperty>();

        public IList<Binding> Bindings { get; } = new List<Binding>();

        public string StringFormat { get; set; }

        public IMultiValueConverter Converter { get; set; }

        public object ConverterParameter { get; set; }

        public Binding ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(StringFormat) && Converter == null)
                throw new InvalidOperationException($"{nameof(MultiBinding)} requires a {nameof(Converter)} or {nameof(StringFormat)}");

            //Get the object that the markup extension is being applied to
            var provideValueTarget = (IProvideValueTarget)serviceProvider?.GetService(typeof(IProvideValueTarget));
            _target = provideValueTarget?.TargetObject as BindableObject;

            if (_target == null) return null;

            foreach (Binding b in Bindings)
            {
                var property = BindableProperty.Create($"Property-{Guid.NewGuid().ToString("N")}", typeof(object),
                    typeof(MultiBinding), default(object), propertyChanged: (_, o, n) => SetValue());
                _properties.Add(property);
                _target.SetBinding(property, b);
            }
            SetValue();

            var binding = new Binding
            {
                Path = nameof(InternalValue.Value),
                Converter = new MultiValueConverterWrapper(Converter, StringFormat),
                ConverterParameter = ConverterParameter,
                Source = _internalValue
            };

            return binding;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }

        private void SetValue()
        {
            if (_target == null) return;
            _internalValue.Value = _properties.Select(_target.GetValue).ToArray();
        }

        private sealed class InternalValue : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private object _value;
            public object Value
            {
                get { return _value; }
                set
                {
                    if (!Equals(_value, value))
                    {
                        _value = value;
                        OnPropertyChanged();
                    }
                }
            }

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private sealed class MultiValueConverterWrapper : IValueConverter
        {
            private readonly IMultiValueConverter _multiValueConverter;
            private readonly string _stringFormat;

            public MultiValueConverterWrapper(IMultiValueConverter multiValueConverter, string stringFormat)
            {
                _multiValueConverter = multiValueConverter;
                _stringFormat = stringFormat;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (_multiValueConverter != null)
                {
                    value = _multiValueConverter.Convert(value as object[], targetType, parameter, culture);
                }
                if (!string.IsNullOrWhiteSpace(_stringFormat))
                {
                    var array = value as object[];
                    // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                    if (array != null)
                    {
                        value = string.Format(_stringFormat, array);
                    }
                    else
                    {
                        value = string.Format(_stringFormat, value);
                    }
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
