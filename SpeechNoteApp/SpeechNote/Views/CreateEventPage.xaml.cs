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
    public enum eState
    {
        CHOICE_TIME,
        CHOICE_FIELD,
        CONFIRM,
        WRITE_TEXT
    }

    public partial class CreateEventPage : PhoneApplicationPage
    {
        private EventInfo _eventInfo = null;
        private string _oldReminderName = String.Empty;
        private int _currentIndex = 0;

        private eState _state = eState.CHOICE_FIELD;
        private bool _speechOrder = false;

        private string _actionField = "";
        private string _savedValue = "";
        private bool _canEdit = true;

        public CreateEventPage()
        {
            InitializeComponent();

            int index = (int)PhoneApplicationService.Current.State["selectedIndex"];
            _speechOrder = (bool)PhoneApplicationService.Current.State["speechOrder"];

            if (index == -1)
            {
                _eventInfo = new EventInfo("", "", DateTime.Now + new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0), false);
                ContentPanel.DataContext = _eventInfo;
            }
            else
            {
                ContentPanel.DataContext = App.EventList[index];
                _oldReminderName = App.EventList[index].Name;
                _currentIndex = 3;
            }

            if(_speechOrder)
            {
                statusPanel.Visibility = Visibility.Visible;

                _actionField = "name";
                StartListenState(eState.WRITE_TEXT, "Give me a name...");

                AudioManager.getInstance().SphinxSpeechRecognizer.resultFound += SphinxSpeechRecognizer_resultFound;
                AudioManager.getInstance().SphinxSpeechRecognizer.resultFinalizedBySilence += SphinxSpeechRecognizer_resultFinalizedBySilence;
            }

            UpdateUI(_currentIndex);

            InitNumberDict();
        }
        
        private void SphinxSpeechRecognizer_resultFinalizedBySilence(string finalResult)
        {
            // kêt thúc câu nói
            if (String.IsNullOrEmpty(finalResult))
                return;
            
            switch (_currentIndex)
            {
                case 0:
                    {
                        if (_canEdit)
                            nameTextBox.Text = finalResult;

                        StartListenState(eState.CONFIRM, "Do you want to go next?");
                        _actionField = "name";
                        break;
                    }
                case 1:
                    {
                        break;
                    }
                case 2:
                    {
                        if (_canEdit)
                            detailTextBox.Text = finalResult;

                        _actionField = "content";
                        StartListenState(eState.CONFIRM, "Do you want to go next?");
                        
                        break;
                    }
                default:
                    break;
            }

            ValueSelection(finalResult);
        }

        private void ValueSelection(string result)
        {
            switch (_state)
            {
                case eState.CHOICE_FIELD:
                    {
                        if (result == "start time" || result == "end time")
                        {
                            _actionField = result;

                            StartListenState(eState.CHOICE_TIME, result.ToUpper() + ". Listening...");
                        }
                        else if (result == "reminder")
                        {
                            _actionField = result;
                            StartListenState(eState.CONFIRM, "Do you want me remind you?");
                        }
                        else if (result == "next")
                        {
                            NextPage();
                            StartListenState(eState.CHOICE_FIELD, "What field do you want to change?");
                        }
                        else if (result == "save")
                        {
                            SaveAll();
                        }
                        else if (result == "name")
                        {
                            _actionField = result;
                            UpdateUI(0);
                            StartListenState(eState.WRITE_TEXT, "Listening...");
                        }
                        else if (result == "content")
                        {
                            _actionField = result;
                            UpdateUI(2);
                            StartListenState(eState.WRITE_TEXT, "Listening...");
                        }
                        else if (result == "cancel" || result == "delete" || result == "remove")
                        {
                            DeleteItem();
                        }

                        break;
                    }
                case eState.CHOICE_TIME:
                    {
                        if (_actionField != "")
                        {
                            _savedValue = result;

                            StartListenState(eState.CONFIRM, StringToTime(_savedValue).ToShortTimeString() + ". Is it right?");
                        }

                        break;
                    }
                case eState.CONFIRM:
                    {
                        if (result == "ok" || result == "yes")
                        {
                            switch (_actionField)
                            {
                                case "start time":
                                    {
                                        startTime.Value = StringToTime(_savedValue);
                                        
                                        StartListenState(eState.CHOICE_FIELD, "Done. What's next?");

                                        break;
                                    }
                                case "end time":
                                    {
                                        endTime.Value = StringToTime(_savedValue);

                                        StartListenState(eState.CHOICE_FIELD, "Done. What's next?");

                                        break;
                                    }

                                case "reminder":
                                    {
                                        reminderCheckbox.IsChecked = true;

                                        StartListenState(eState.CHOICE_FIELD, "Done. What's next?");
                                        break;
                                    }
                                case "name":
                                    {
                                        UpdateUI(1);
                                        StartListenState(eState.CHOICE_FIELD, "What field do you want to change?");
                                        break;
                                    }
                                case "content":
                                    {
                                        UpdateUI(3);
                                        StopListening();
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                        else if (result == "no" || result == "cancel")
                        {
                            switch (_actionField)
                            {
                                case "start time":
                                case "end time":
                                    {
                                        StartListenState(eState.CHOICE_TIME, "Listening...");
                                        break;
                                    }
                                case "reminder":
                                    {
                                        reminderCheckbox.IsChecked = false;
                                        StartListenState(eState.CHOICE_FIELD, "Ok...What's next?");
                                        break;
                                    }
                                case "name":
                                case "content":
                                    {
                                        StartListenState(eState.WRITE_TEXT, "Listening...");
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                        break;
                    }
                case eState.WRITE_TEXT:
                    {
                        break;
                    }
                default:
                    break;
            }
        }

        private void SphinxSpeechRecognizer_resultFound(string result)
        {
            if (String.IsNullOrEmpty(result))
                return;
            
            if(_state == eState.CHOICE_TIME)
                statusText.Text = result;
        }

        private void StartListenState(eState state, string message)
        {
            _state = state;
            statusText.Text = message;

            AudioManager.getInstance().StopRecorder();

            switch (state)
            {
                case eState.CHOICE_TIME:
                    _savedValue = "";
                    _canEdit = false;
                    AudioManager.getInstance().StartRecorder("timesimple");
                    break;
                case eState.CHOICE_FIELD:
                    _actionField = "";
                    _canEdit = false;
                    AudioManager.getInstance().StartRecorder("notefield");
                    break;
                case eState.CONFIRM:
                    _canEdit = false;
                    AudioManager.getInstance().StartRecorder("yesno");
                    break;
                case eState.WRITE_TEXT:
                    _canEdit = true;
                    AudioManager.getInstance().StartRecorder("language");
                    break;
                default:
                    break;
            }
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            StopListening();
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            NextPage();
        }

        private void NextPage()
        {
            if (_currentIndex < 3)
            {
                _currentIndex++;
                UpdateUI(_currentIndex);
            }
        }

        private void UpdateUI(int index)
        {
            _currentIndex = index;

            if (_currentIndex == 3)
                (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;

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
        
        private void listenBtn_Click(object sender, EventArgs e)
        {
            statusPanel.Visibility = Visibility.Visible;

            _speechOrder = true;
            StartListenState(eState.CHOICE_FIELD, "What field do you want to change?");

            AudioManager.getInstance().SphinxSpeechRecognizer.resultFound += SphinxSpeechRecognizer_resultFound;
            AudioManager.getInstance().SphinxSpeechRecognizer.resultFinalizedBySilence += SphinxSpeechRecognizer_resultFinalizedBySilence;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            SaveAll();
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

        private void SaveAll()
        {
            // focus vô cái khác để các UI đang edit nó notify cho model update
            this.Focus();

            var data = ContentPanel.DataContext as EventInfo;
            if (data.Date <= DateTime.Now || data.EndDate <= DateTime.Now || data.EndDate < data.Date)
            {
                MessageBox.Show("Please check Start Time/End Time. They must be in the furture.", "Can't save!", MessageBoxButton.OK);
                return;
            }
            
            try
            {
                SetRemider();

                if (_eventInfo != null)
                    App.EventList.Add(_eventInfo);
                
                SaveEventList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            DeleteItem();
        }

        private void DeleteItem()
        {
            if(_speechOrder)
                StopListening();

            if (_eventInfo != null)
            {
                this.NavigationService.GoBack();
                return;
            }

            var data = ContentPanel.DataContext as EventInfo;
            if (data.IsReminder && ScheduledActionService.Find(data.Name) != null)
            {
                ScheduledActionService.Remove(data.Name);
            }

            App.EventList.Remove(data);

            SaveEventList();
        }

        private DateTime StringToTime(string input)
        {
           
            var arrTime = input.Split(' ');
            int h = 0;
            int m = 0;
            int add = 0;

            if (GetNumber(arrTime[0]) == -1)
            {
                return DateTime.Now;
            }

            switch (arrTime.Length)
            {
                case 2:
                    {
                        if (arrTime[1] == "pm")
                        {
                            add = 12;
                        }
                        else if (arrTime[1] != "o'clock" && arrTime[1] != "am")
                        {
                            m = GetNumber(arrTime[1]);
                        }

                        break;
                    }
                case 3:
                    {
                        m = GetNumber(arrTime[1]);

                        if (arrTime[2] == "pm")
                        {
                            add = 12;
                        }
                        else if (arrTime[2] != "am")
                        {
                            m += GetNumber(arrTime[2]);
                        }

                        break;
                    }
                case 4:
                    {
                        if (arrTime[3] == "pm")
                        {
                            add = 12;
                        }

                        m = GetNumber(arrTime[1]);
                        m += GetNumber(arrTime[2]);

                        break;
                    }
                default:
                    break;
            }

            h = GetNumber(arrTime[0]) + add;
            if (h == 24)
                h = 0;

            DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, h, m, 0, 0, DateTimeKind.Utc);

            return time;

        }

        static Dictionary<string, int> NumberDict = new Dictionary<string, int>();

        private void InitNumberDict()
        {
            NumberDict["one"] = 1;
            NumberDict["two"] = 2;
            NumberDict["three"] = 3;
            NumberDict["four"] = 4;
            NumberDict["five"] = 5;
            NumberDict["six"] = 6;
            NumberDict["seven"] = 7;
            NumberDict["eight"] = 8;
            NumberDict["nine"] = 9;
            NumberDict["ten"] = 10;
            NumberDict["eleven"] = 11;
            NumberDict["twelve"] = 12;
            NumberDict["thirteen"] = 13;
            NumberDict["fourteen"] = 14;
            NumberDict["fifteen"] = 15;
            NumberDict["sixteen"] = 16;
            NumberDict["seventeen"] = 17;
            NumberDict["eighteen"] = 18;
            NumberDict["nineteen"] = 19;
            NumberDict["twenty"] = 20;
            NumberDict["thirty"] = 30;
            NumberDict["forty"] = 40;
            NumberDict["fifty"] = 50;
        }

        private int GetNumber(string text)
        {
            if (NumberDict.ContainsKey(text))
                return NumberDict[text];
            else
                return -1;
        }

        private void StopListening()
        {
            statusPanel.Visibility = Visibility.Collapsed;
            _speechOrder = false;

            AudioManager.getInstance().StopRecorder();
            AudioManager.getInstance().SphinxSpeechRecognizer.resultFound -= SphinxSpeechRecognizer_resultFound;
            AudioManager.getInstance().SphinxSpeechRecognizer.resultFinalizedBySilence -= SphinxSpeechRecognizer_resultFinalizedBySilence;
        }
    }
}