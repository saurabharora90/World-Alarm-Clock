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
using System.Xml.Linq;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using System.Globalization;

namespace Country_Specific_Alarm
{
    public partial class Alarm : PhoneApplicationPage
    {

        List<TimeData> dataSource;
        PerformanceProgressBar progress;
        DateTime finalAlarm;

        public Alarm()
        {
            InitializeComponent();
            progress = new PerformanceProgressBar();
            this.TitlePanel.Children.Add(progress);

            dataSource = new List<TimeData>();
            
            //call webservice to download data
            callWebService();
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
                var messagePrompt = new MessagePrompt
                {
                    Title = "Connection Failure",
                    Body = new TextBlock { Text = "Please check if you are connected to the Internet", Foreground = new SolidColorBrush(Colors.Red), FontSize = 30.0, TextWrapping = TextWrapping.Wrap },
                    IsAppBarVisible = false
                };
            messagePrompt.Show();
            messagePrompt.Completed += new EventHandler<PopUpEventArgs<string, PopUpResult>>(messagePrompt_Completed);
            }
        }

        void messagePrompt_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            NavigationService.GoBack();
        }

        private void set_Click(object sender, EventArgs e)
        {
            List<TimeData> values = (List<TimeData>)TimeBox.ItemsSource;
            double offset = 0;
            
            //Get the offset
            foreach (TimeData item in values)
            {
                if (item.ToString() == TimeBox.Text)
                {
                    offset = item.offset;
                    break;
                }
            }
            //Get alarm time
            DateTime alarmTime = (DateTime)timepick.Value;
            DateTime alarmDate = (DateTime)datepick.Value;
            DateTime finalAlarmTime = alarmDate + alarmTime.TimeOfDay;

            //Get time difference
            int hour = (int)offset;
            int minute = (int)((offset - (double)hour) * 60);
            TimeSpan diff = new TimeSpan(hour, minute, 0);

            //Convert alarmTime to GMT
            DateTime alarmTimeInGMT = finalAlarmTime.Subtract(diff);

            //Get local timedifference from GMT
            TimeSpan local = TimeZoneInfo.Local.BaseUtcOffset;

            //Add 1 hour to localoffset if it is in daylight saving
            if (DateTime.Now.IsDaylightSavingTime())
                local = local.Add(new TimeSpan(1, 0, 0));

            finalAlarm = alarmTimeInGMT.Add(local);
            //if (finalAlarm.CompareTo(DateTime.Now) < 0)
            //    finalAlarm = finalAlarm.AddDays(1);
            
            //Confirm from user if he wants to set the alarm
            progress.IsIndeterminate = false;
            string confirmation = "Set alarm for " + finalAlarm.ToString() + " local time?"; 
            var messagePrompt = new MessagePrompt
            {
                Title = "Local Alarm time",
                
                Body = new TextBlock { Text = confirmation, Foreground = new SolidColorBrush(Colors.Red), FontSize = 30.0, TextWrapping = TextWrapping.Wrap },
                IsAppBarVisible = false,
                IsCancelVisible = true
            };
            messagePrompt.Show();
            messagePrompt.Completed += new EventHandler<PopUpEventArgs<string, PopUpResult>>(messagePrompt1_Completed);
        }

        void messagePrompt1_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult == PopUpResult.Ok)
            {
                //set alarm
                var alarm = new Microsoft.Phone.Scheduler.Alarm(System.Guid.NewGuid().ToString())
                {
                    Content = desc.Text,
                    BeginTime = finalAlarm,
                    Sound = new Uri("lovely_alarm.mp3", UriKind.Relative)
                    //ExpirationTime = dueTime.AddSeconds(3),
                    //RecurrenceType = RecurrenceInterval.None
                };
                try
                {
                    ScheduledActionService.Add(alarm);
                    //Display success
                    var messagePrompt = new MessagePrompt
                    {
                        Title = "Alarm set",
                        IsAppBarVisible = false
                    };
                    messagePrompt.Show();
                }
                catch
                {
                    var messagePrompt = new MessagePrompt
                    {
                        Title = "Error",
                        Body = new TextBlock { Text = "Internal Error! Alarm time might have already passed. Please try again", Foreground = new SolidColorBrush(Colors.Red), FontSize = 30.0, TextWrapping = TextWrapping.Wrap },
                        IsAppBarVisible = false
                    };
                    messagePrompt.Show();
                }
            }
        }

        private void clear_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void TimeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsContains(TimeBox.Text, TimeBox))
                (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
            else
                (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
        }

        private bool IsContains(string value, AutoCompleteBox autobox)
        {
            List<TimeData> values = (List<TimeData>)autobox.ItemsSource;
            foreach (TimeData item in values)
            {
                if (item.ToString() == value)
                    return true;
            }
            return false;
        }

        private void TimeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
            this.Focus();
        }

    }
}