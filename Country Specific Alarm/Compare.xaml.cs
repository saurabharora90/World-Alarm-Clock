using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using Coding4Fun.Phone.Controls;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.Globalization;

namespace Country_Specific_Alarm
{
    public partial class Compare : PhoneApplicationPage
    {
        public ObservableCollection<SavedTimeZones> existingZones { get; set; }
        List<TimeData> dataSource;
        PerformanceProgressBar progress;
        IsolatedStorageSettings settings;

        public Compare()
        {
            InitializeComponent();
            settings = IsolatedStorageSettings.ApplicationSettings;
            dataSource = new List<TimeData>();
            progress = new PerformanceProgressBar();
            //this.TitlePanel.Children.Add(progress);
            //callWebService();
            //timeZone.Text = TimeZoneInfo.Local.DisplayName;

            readXML("world-time.xml");

            existingZones = new ObservableCollection<SavedTimeZones>();
            MainListBox.ItemsSource = existingZones;
            load();
        }

        //load from isolated storage settings which conatins Key: city, value: offset
        private void load()
        {
            foreach (var item in settings)
            {
                string city = item.Key;
                double offset = Convert.ToDouble(item.Value);
                int hour = (int)offset;
                int minute = (int)((offset - (double)hour) * 60);
                TimeSpan baseOffset = TimeZoneInfo.Local.BaseUtcOffset;
                
                //Add 1 hour to offset if it is in daylight saving
                if (DateTime.Now.IsDaylightSavingTime())
                    baseOffset = baseOffset.Add(new TimeSpan(1, 0, 0));

                DateTime set = (DateTime.Now.Subtract(baseOffset)).Add(new TimeSpan(hour, minute, 0));
                existingZones.Add(new SavedTimeZones() { City = city, Date = set.ToShortDateString(), Offset = offset, Time = set.ToShortTimeString() });
            }
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            //Save to IslolatedStorage setting.
            List<TimeData> values = (List<TimeData>)TimeBox.ItemsSource;
            double offset = 0;
            string city = "";

            //Get the offset
            foreach (TimeData item in values)
            {
                if (item.ToString() == TimeBox.Text)
                {
                    offset = item.offset;
                    city = item.City;
                    break;
                }
            }
            try
            {
                settings.Add(city, offset);

                //Display added city's corresponding time
                int hour = (int)offset;
                int minute = (int)((offset - (double)hour) * 60);
                DateTime newDate = (DateTime)datePick.Value;
                newDate = newDate.Add(((DateTime)timePick.Value).TimeOfDay);
                TimeSpan baseOffset = TimeZoneInfo.Local.BaseUtcOffset;

                //Add 1 hour to offset if it is in daylight saving
                if (DateTime.Now.IsDaylightSavingTime())
                    baseOffset = baseOffset.Add(new TimeSpan(1, 0, 0));

                DateTime set = (newDate.Subtract(baseOffset)).Add(new TimeSpan(hour, minute, 0));
                existingZones.Add(new SavedTimeZones() { City = city, Date = set.ToShortDateString(), Offset = offset, Time = set.ToShortTimeString() });
            }

            catch
            {
                MessageBox.Show("Already exists!", "Error", MessageBoxButton.OK);
            }
            
            TimeBox.Text = "";
            add.IsEnabled = false;
        }

        private void datePick_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            DateTime newDate = (DateTime)e.NewDateTime;
            newDate = newDate.Add(((DateTime)timePick.Value).TimeOfDay);
            
            //Convert thisNewTime to GMT
            newDate = newDate.Subtract(TimeZoneInfo.Local.BaseUtcOffset);

            //Update all saved cities times
            foreach (var item in existingZones)
            {
                int hour = (int)item.Offset;
                int minute = (int)((item.Offset - (double)hour) * 60);

                DateTime temp = newDate.Add(new TimeSpan(hour, minute, 0));
                item.Date = temp.ToShortDateString();
                item.Time = temp.ToShortTimeString();
            }
        }

        private void timePick_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            DateTime newDate = (DateTime)e.NewDateTime;
            
            //Bug fix: When you change the date and then change the time, the comparision is done on the basis of the new time and the current date and not on the basis of the new date (which was changed before changing the time)

            TimeSpan diff = ((DateTime)(datePick.Value)) - DateTime.Now.Date;
            newDate = newDate.Add(diff);

            //Convert thisNewTime to GMT
            newDate = newDate.Subtract(TimeZoneInfo.Local.BaseUtcOffset);

            //Update all saved cities times
            foreach (var item in existingZones)
            {
                int hour = (int)item.Offset;
                int minute = (int)((item.Offset - (double)hour) * 60);

                DateTime temp = newDate.Add(new TimeSpan(hour, minute, 0));
                item.Date = temp.ToShortDateString();
                item.Time = temp.ToShortTimeString();
            }
        }

        bool search(string search, object value)
        {
            if (value != null)
            {
                TimeData datasourceValue = value as TimeData;
                string city = datasourceValue.City;
                string country = datasourceValue.Country;

                //To search with both city and country
                if (city.ToLower().StartsWith(search.ToLower()) || country.ToLower().StartsWith(search.ToLower()))
                    return true;
            }
            //... If no match, return false. 
            return false;
        }

        void callWebService()
        {
            var client = new WebClient();
            client.DownloadStringCompleted += downloadCompleted;
            client.DownloadStringAsync(new Uri("http://rss.timegenie.com/world-time.xml"));

            this.ContentPanel.Visibility = System.Windows.Visibility.Collapsed;
            progress.Foreground = new SolidColorBrush(Colors.Blue);
            progress.IsIndeterminate = true;
        }

        void readXML(string filepath)
        {
            //string result = ev.Result;
            //XDocument xdoc = XDocument.Parse(result);
            XDocument xdoc = XDocument.Load(filepath);
            var query = from p in xdoc.Elements("worldtime").Elements("data")
                        select p;
            foreach (var record in query)
            {
                dataSource.Add(new TimeData { City = record.Element("city").Value, Country = record.Element("country").Value, offset = Convert.ToDouble(record.Element("offset").Value, new CultureInfo("en-US")) });
            }

            //Populate the autocomplete box
            TimeBox.ItemsSource = dataSource;
            TimeBox.ItemFilter = search;
        }

        void downloadCompleted(object s, DownloadStringCompletedEventArgs ev)
        {
            if (ev.Error == null)
            {
                string result = ev.Result;
                XDocument xdoc = XDocument.Parse(result);
                var query = from p in xdoc.Elements("worldtime").Elements("data")
                            select p;
                foreach (var record in query)
                {
                    dataSource.Add(new TimeData { City = record.Element("city").Value, Country = record.Element("country").Value, offset = Convert.ToDouble(record.Element("offset").Value, new CultureInfo("en-US")) });
                }

                progress.IsIndeterminate = false;

                //Populate the autocomplete box
                TimeBox.ItemsSource = dataSource;
                TimeBox.ItemFilter = search;

                //show the content, show the appbar
                this.ContentPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                progress.IsIndeterminate = false;
                TimeBox.Text = "Check your data connection";
            }
        }

        private void TimeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            add.IsEnabled = true;
            this.Focus();
        }

        private void TimeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsContains(TimeBox.Text, TimeBox))
                add.IsEnabled = true;
            else
                add.IsEnabled = false;
        }

        private bool IsContains(string value, AutoCompleteBox autobox)
        {
            List<TimeData> values = (List<TimeData>)autobox.ItemsSource;

            //Bug fix: If internet connection has failed then value is empty
            if (values!=null)
            {
                foreach (TimeData item in values)
                {
                    if (item.ToString() == value)
                        return true;
                }
            }
            return false;
        }

        private void remove_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuItem).DataContext;
            SavedTimeZones select = data as SavedTimeZones;
            MessageBoxResult result = MessageBox.Show("Are you sure?", "Remove " + select.City, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                existingZones.Remove(select);
                //Remove from isolated storage settings
                settings.Remove(select.City);
            }
        }

        //protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    //timePick.Value = DateTime.Now;
        //    //datePick.Value = DateTime.Now;
        //    base.OnNavigatedTo(e);
        //}

    }
}