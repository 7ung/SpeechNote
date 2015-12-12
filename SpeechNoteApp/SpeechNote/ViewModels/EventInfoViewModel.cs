using SpeechNote.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechNote.ViewModels
{
    public class EventInfoViewModel
    {
        private ObservableCollection<EventInfo> _eventList = new ObservableCollection<EventInfo>();

        public void AddEvent(EventInfo info)
        {
            _eventList.Add(info);
        }

        public ObservableCollection<EventInfo> EventList
        {
            get { return _eventList; }
            set { _eventList = value; }
        }

        public void CreateExample()
        {
            for(int i = 0; i < 10; i++)
            {
                _eventList.Add(new EventInfo("Test " + i, "This is a description.", DateTime.Now, new TimeSpan(2, 0, 0), false));
            }
        }
    }
}
