using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Country_Specific_Alarm
{
//    public class SavedTimeZones
//    {
//        public string City { get; set; }

//        public string Date { get; set; }

//        public string Time { get; set; }

//        public double Offset { get; set; }
//    }

    public class SavedTimeZones : INotifyPropertyChanged
    {
        private string _city;
        public string City
        {
            get
            {
                return _city;
            }
            set
            {
                _city = value;
            }
        }

        private double _offset;
        public double Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
            }
        }

        private string _date;
        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                NotifyPropertyChanged("Date");
            }
        }

        private string _time;
        public string Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
                NotifyPropertyChanged("Time");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
