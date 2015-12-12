using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechNote.Models
{
    public class EventInfo : INotifyPropertyChanged
    {
        private string _name;
        private string _description;
        private TimeSpan _time;
        private DateTime _date;
        private DateTime _endDate;


        private bool _isReminder;

        #region Fields

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                onPropertyChanged("Name");
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                _description = value;
                onPropertyChanged("Description");
            }
        }
        
        public bool IsReminder
        {
            get
            {
                return _isReminder;
            }

            set
            {
                _isReminder = value;
                onPropertyChanged("IsReminder");
            }
        }

        public TimeSpan Time
        {
            get
            {
                return _time;
            }

            set
            {
                _time = value;
                onPropertyChanged("Time");
            }
        }

        public DateTime Date
        {
            get
            {
                return _date;
            }

            set
            {
                _date = value;
                onPropertyChanged("Date");
            }
        }


        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
                Time = EndDate - Date;
                onPropertyChanged("EndDate");
            }
        }

        #endregion

        public EventInfo()
        {
        }

        public EventInfo(string name, string desc, DateTime date, TimeSpan time, bool reminder)
        {
            this.Name = name;
            this.Description = desc;
            this.Date = date;
            this.EndDate = date + time;
            this.Time = time;
            this.IsReminder = reminder;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string propertyName)
        {
            if(this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
    }
}