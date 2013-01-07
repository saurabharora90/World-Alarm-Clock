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
using Microsoft.Phone.Shell;

namespace Country_Specific_Alarm
{
    public partial class Reminders : PhoneApplicationPage
    {
        IEnumerable<ScheduledNotification> notifications;
        
        public Reminders()
        {
            InitializeComponent();
        }

        private void ResetItemsList()
        {
            // Use GetActions to retrieve all of the scheduled actions
            // stored for this application. The type <Reminder> is specified
            // to retrieve only Reminder objects.
            //reminders = ScheduledActionService.GetActions<Reminder>();
            RemoveNotScheduled();
            notifications = ScheduledActionService.GetActions<ScheduledNotification>();

            // If there are 1 or more reminders, hide the "no reminders"
            // TextBlock. IF there are zero reminders, show the TextBlock.
            //if (reminders.Count<Reminder>() > 0)
            if (notifications.Count<ScheduledNotification>() > 0)
            {
                EmptyTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyTextBlock.Visibility = Visibility.Visible;
            }

            // Update the ReminderListBox with the list of reminders.
            // A full MVVM implementation can automate this step.
            NotificationListBox.ItemsSource = notifications;
        }

        //To only display the ones which are scheduled and delete the rest from the list
        void RemoveNotScheduled()
        {
            notifications = ScheduledActionService.GetActions<ScheduledNotification>();
            foreach (var item in notifications)
            {
                if (item.IsScheduled == false)
                {
                    ScheduledActionService.Remove(item.Name);

                    //Also remove the tile for this if any
                    string val = "DefualtTitle=" + item.Content;
                    ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(val));

                    // If the Tile was found, then delete it.
                    if (TileToFind != null)
                    {
                        TileToFind.Delete();
                    }
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //Reset the ReminderListBox items when the page is navigated to.
            ResetItemsList();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            // The scheduled action name is stored in the Tag property
            // of the delete button for each reminder.
            //string name = (string)((Button)sender).Tag;
            string name = ((sender as MenuItem).Tag).ToString();

            // Call Remove to unregister the scheduled action with the service.
            ScheduledActionService.Remove(name);

            // Reset the ReminderListBox items
            ResetItemsList();

            //Delete tile for it
            ScheduledNotification temp = (ScheduledNotification)((sender as MenuItem).DataContext);
            string val = "DefualtTitle=" + temp.Content;
            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(val));

            // If the Tile was found, then delete it.
            if (TileToFind != null)
            {
                TileToFind.Delete();
            }
        }

        private void ApplicationBarAddButton_Click(object sender, EventArgs e)
        {
            // Navigate to the AddReminder page when the add button is clicked.
            NavigationService.Navigate(new Uri("/Alarm.xaml", UriKind.RelativeOrAbsolute));
        }

        private void tile_Click(object sender, RoutedEventArgs e)
        {
            ScheduledNotification temp= (ScheduledNotification)((sender as MenuItem).DataContext);

            StandardTileData NewTileData = new StandardTileData
            {
                Title = temp.Content,
                BackTitle = temp.Content,
                BackContent = "Alarm set for " + temp.BeginTime.ToShortTimeString() + " on " + temp.BeginTime.Date.ToShortDateString(),
            };

            // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
            string val = "DefualtTitle=" + temp.Content;
            string uri = "/Reminders.xaml?DefualtTitle=" + temp.Content;
            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(val));
            if (TileToFind!=null)
            {
                MessageBox.Show("Tile already exists", "Error", MessageBoxButton.OK);
            }
            else
                ShellTile.Create(new Uri(uri, UriKind.Relative), NewTileData);
        }
    }
}