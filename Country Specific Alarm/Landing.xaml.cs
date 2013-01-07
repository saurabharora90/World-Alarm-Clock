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
using Microsoft.Phone.Scheduler;

namespace Country_Specific_Alarm
{
    public partial class Landing : PhoneApplicationPage
    {
        public Landing()
        {
            InitializeComponent();
            compareTimeTile.Title = "Compare\nTime";
        }

        private void suggestions_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //Replaced this with reminders.

            //Microsoft.Phone.Tasks.EmailComposeTask compose = new Microsoft.Phone.Tasks.EmailComposeTask();
            //compose.To = "saurabh_arora129@hotmail.com";
            //compose.Subject = "New feature request/Bug report";
            //compose.Show();

            NavigationService.Navigate(new Uri("/Reminders.xaml", UriKind.Relative));
        }

        private void rate_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Microsoft.Phone.Tasks.MarketplaceReviewTask review = new Microsoft.Phone.Tasks.MarketplaceReviewTask();
            review.Show();
        }

        private void systemTimeTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //freezeTile();
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        //private void freezeTile()
        //{
        //    HubTileService.FreezeGroup("main");
        //}

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            int count = ScheduledActionService.GetActions<ScheduledNotification>().Count<ScheduledNotification>();
            suggestionsTile.Title = "You have\n" + count.ToString() + "\nreminders";
            //suggestionsTile.DisplayNotification = true;
            suggestionsTile.Message = count.ToString();
            base.OnNavigatedTo(e);
        }

        private void aboutTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void worldAlarmTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Alarm.xaml", UriKind.Relative));
        }

        private void compareTimeTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Compare.xaml", UriKind.Relative));
        }

    }
}