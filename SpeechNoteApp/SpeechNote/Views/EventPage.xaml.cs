using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SpeechNote.ViewModels;
using System.IO;
using Windows.Storage;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Xml;
using SpeechNote.Models;
using System.Diagnostics;

namespace SpeechNote.Views
{
    public partial class EventPage : PhoneApplicationPage
    {
        private EventInfoViewModel _eventInfoViewModel = new EventInfoViewModel();

        public EventPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadData();
            
            eventList.ItemsSource = App.EventList;
        }

        private async void LoadData()
        {
            if (App.EventList != null)
                return;

            try
            {
                using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("events-data.xml"))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<EventInfo>));

                    App.EventList = new ObservableCollection<EventInfo>();
                    App.EventList = (xmlSerializer.Deserialize(stream)) as ObservableCollection<EventInfo>;

                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                App.EventList = new ObservableCollection<EventInfo>();
            }
        }

        private void createBtn_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/CreateEventPage.xaml", UriKind.Relative));

            // -1 là tạo mới
            PhoneApplicationService.Current.State["selectedIndex"] = -1;
        }

        private void createListenBtn_Click(object sender, EventArgs e)
        {
            statusPanel.Visibility = Visibility.Visible;
        }

        private void itemPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var obj = sender as Grid;
            var data =  obj.DataContext as EventInfo;
            var index = (eventList.ItemsSource as ObservableCollection<EventInfo>).IndexOf(data);
            
            Debug.WriteLine("Selected Index: " + index);

            NavigationService.Navigate(new Uri("/Views/CreateEventPage.xaml", UriKind.Relative));

            PhoneApplicationService.Current.State["selectedIndex"] = index;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            statusPanel.Visibility = Visibility.Collapsed;
        }
    }
}