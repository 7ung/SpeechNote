using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SpeechNote.Models;
using System.IO;
using Windows.Storage;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Microsoft.Phone.Scheduler;

namespace SpeechNote.Views
{
    public partial class CreateEventPage : PhoneApplicationPage
    {
        private EventInfo _eventInfo = null;
        private string _oldReminderName = String.Empty;
        private int _currentIndex = 0;

        public CreateEventPage()
        {
            InitializeComponent();

            int index = (int)PhoneApplicationService.Current.State["selectedIndex"];
            if (index == -1)
            {
                _eventInfo = new EventInfo("", "", DateTime.Now, new TimeSpan(2, 0, 0), false);
                ContentPanel.DataContext = _eventInfo;
            }
            else
            {
                ContentPanel.DataContext = App.EventList[index];
                _oldReminderName = App.EventList[index].Name;
                _currentIndex = 3;
            }

            UpdateUI(_currentIndex);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            statusPanel.Visibility = Visibility.Collapsed;
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            if(_currentIndex < 3)
            {
                _currentIndex++;
                UpdateUI(_currentIndex);

                if (_currentIndex == 3)
                    (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
            }
        }


        private void UpdateUI(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        namePanel.Visibility = Visibility.Visible;
                        datePanel.Visibility = Visibility.Collapsed;
                        detailPanel.Visibility = Visibility.Collapsed;
                        break;
                    }
                case 1:
                    {
                        namePanel.Visibility = Visibility.Collapsed;
                        datePanel.Visibility = Visibility.Visible;
                        detailPanel.Visibility = Visibility.Collapsed;
                        break;
                    }
                case 2:
                    {
                        namePanel.Visibility = Visibility.Collapsed;
                        datePanel.Visibility = Visibility.Collapsed;
                        detailPanel.Visibility = Visibility.Visible;
                        break;
                    }
                case 3:
                    {
                        namePanel.Visibility = Visibility.Visible;
                        datePanel.Visibility = Visibility.Visible;
                        detailPanel.Visibility = Visibility.Visible;
                        break;
                    }
                default:
                    break;
            }
        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void listenBtn_Click(object sender, EventArgs e)
        {
            statusPanel.Visibility = Visibility.Visible;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            // focus vô cái khác để các UI đang edit nó notify cho model update
            this.Focus();

            if (_eventInfo != null)
                App.EventList.Add(_eventInfo);
            
            SetRemider();

            SaveEventList();
        }
        
        private async void SaveEventList()
        {
            try
            {
                using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("events-data.xml", CreationCollisionOption.ReplaceExisting))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<EventInfo>));
                    xmlSerializer.Serialize(stream, App.EventList);
                    
                    stream.Close();

                    this.NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
            
        }

        private void SetRemider()
        {
            var data = ContentPanel.DataContext as EventInfo;

            // xóa cái cũ đi
            if (_oldReminderName != String.Empty && ScheduledActionService.Find(_oldReminderName) != null)
            {
                ScheduledActionService.Remove(_oldReminderName);
            }

            // save cái mới
            if (data.IsReminder)
            {
                var reminder = new Reminder(data.Name);
                reminder.Title = data.Name;
                reminder.Content = data.Description;
                reminder.BeginTime = data.Date;
                reminder.ExpirationTime = data.EndDate;
                reminder.RecurrenceType = RecurrenceInterval.None;
                
                ScheduledActionService.Add(reminder);
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            if(_eventInfo != null)
            {
                this.NavigationService.GoBack();
            }

            var data = ContentPanel.DataContext as EventInfo;
            if (data.IsReminder && ScheduledActionService.Find(data.Name) != null)
            {
                ScheduledActionService.Remove(data.Name);
            }

            App.EventList.Remove(data);

            SaveEventList();
        }
    }
}