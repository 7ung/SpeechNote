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
using System.Threading.Tasks;

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
            CreateNewEvent(false);
        }

        private void createListenBtn_Click(object sender, EventArgs e)
        {
            statusPanel.Visibility = Visibility.Visible;
            
            AudioManager.getInstance().SphinxSpeechRecognizer.resultFound += SpeechRecognizer_ResultFound;
            AudioManager.getInstance().SphinxSpeechRecognizer.resultFinalizedBySilence += SpeechRecognizer_FinalResultFound;

            AudioManager.getInstance().StartRecorder("createnote");
            statusText.Text = "I'm listening...";
        }

        private void itemPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var obj = sender as Grid;
            var data =  obj.DataContext as EventInfo;
            var index = (eventList.ItemsSource as ObservableCollection<EventInfo>).IndexOf(data);
            
            Debug.WriteLine("Selected Index: " + index);

            CreateNewEvent(false, index);
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            statusPanel.Visibility = Visibility.Collapsed;

            AudioManager.getInstance().StopRecorder();
            AudioManager.getInstance().SphinxSpeechRecognizer.resultFound -= SpeechRecognizer_ResultFound;
            AudioManager.getInstance().SphinxSpeechRecognizer.resultFinalizedBySilence -= SpeechRecognizer_FinalResultFound;
        }

        private async void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (AudioManager.getInstance().SphinxSpeechRecognizer != null)
                return;

            // Khởi tạo bộ dịch phiên âm
            await initSpeechRecognizer();

            AudioManager.getInstance().InitAudioRecorder();

            // load xong mới enable listen
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
        }

        private async Task initSpeechRecognizer()
        {
            try
            {
                await Task.Run(() => {

                    AudioManager.getInstance().InitSpeechRecognizer();
                });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
        
        private void SpeechRecognizer_ResultFound(string result)
        {
            // nhận diện được từ
            if (String.IsNullOrEmpty(result) == false)
                this.statusText.Text = result;
        }

        private void SpeechRecognizer_FinalResultFound(string finalResult)
        {
            // kêt thúc câu nói
            if (String.IsNullOrEmpty(finalResult))
                return;

            this.statusText.Text = finalResult;

            if(finalResult == "take a note" || finalResult == "create a note" || finalResult == "take note")
            {
                AudioManager.getInstance().StopRecorder();
                statusPanel.Visibility = Visibility.Collapsed;

                Debug.WriteLine("Create new...");

                AudioManager.getInstance().SphinxSpeechRecognizer.resultFinalizedBySilence -= SpeechRecognizer_FinalResultFound;
                AudioManager.getInstance().SphinxSpeechRecognizer.resultFound -= SpeechRecognizer_ResultFound;

                CreateNewEvent(true);
            }
        }


        private void CreateNewEvent(bool speech)
        {
            NavigationService.Navigate(new Uri("/Views/CreateEventPage.xaml", UriKind.Relative));

            // -1 là tạo mới
            PhoneApplicationService.Current.State["selectedIndex"] = -1;
            PhoneApplicationService.Current.State["speechOrder"] = speech;
        }

        private void CreateNewEvent(bool speech, int index = -1)
        {
            NavigationService.Navigate(new Uri("/Views/CreateEventPage.xaml", UriKind.Relative));

            // -1 là tạo mới
            PhoneApplicationService.Current.State["selectedIndex"] = index;
            PhoneApplicationService.Current.State["speechOrder"] = speech;
        }
    }
}