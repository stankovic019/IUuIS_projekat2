using NetworkService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NetworkService.DTOs
{
    public class HistoryDto : ValidationBase
    {
        private string actionName;
        private string dateTime;

        public HistoryDto(string actionName, string dateTime)
        {
            this.actionName = actionName;
            this.dateTime = dateTime;
        }

        public string ActionName
        {
            get { return this.actionName; }
            set {
                if (actionName != value)
                {
                    actionName = value;
                    OnPropertyChanged("ActionName");
                }
            }
        }

        public string TimeStamp
        {
            get { return this.dateTime; }
            set
            {
                if (dateTime != value)
                {
                    dateTime = value;
                    OnPropertyChanged("DateTime");
                }
            }
        }

        protected override void ValidateSelf()
        {
            if (string.IsNullOrWhiteSpace(string.IsNullOrWhiteSpace(ActionName) ? "" : ActionName.Trim()))
            {
                this.ValidationErrors["Name"] = "Name cannot be empty.";
            }

        }
    }
}
