using NetworkService.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NetworkService.Model
{
    public class Valve : ValidationBase
    {
        private int id;
        private string name;
        private ValveType type;
        private int measuredValue;
        private ValueValidation validation;
        private string dateTime;


        public Valve(int id, string name, ValveType type, int measuredValue, string dateTime)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.MeasuredValue = measuredValue;
            this.DateTime = dateTime;

        }

        public void updateValues(int measuredValue, string dateTime)
        {
            this.MeasuredValue = measuredValue;
            this.DateTime = dateTime;
        }

        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public ValveType Type
        {
            get { return type; }
            set
            {
                if(type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public int MeasuredValue
        {
            get { return measuredValue; }
            set
            {
                if (measuredValue != value)
                {
                    measuredValue = value;
                    OnPropertyChanged("MeasuredValue");
                    if(measuredValue < 5)
                        Validation = ValueValidation.Low;
                    else if(measuredValue > 16)
                        Validation = ValueValidation.High;
                    else
                        Validation = ValueValidation.Normal;
                }
            }
        }

        public ValueValidation Validation
        {
            get { return validation; }
            set
            {
                if (validation != value)
                {
                    validation = value;
                    OnPropertyChanged("Validation");
                }
            }
        }

        public string DateTime
        {
            get { return dateTime; }
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
            if (string.IsNullOrWhiteSpace(string.IsNullOrWhiteSpace(Name) ? "" : Name.Trim()))
            {
                this.ValidationErrors["Name"] = "Name cannot be empty.";
            }
 
            if (MeasuredValue < 0)
            {
                this.ValidationErrors["Value"] = "Value must be non-negative.";
            }


        }
    }
}
