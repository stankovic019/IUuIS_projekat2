using NetworkService.Helpers;

namespace NetworkService.Model
{
    public class PlotValve : ValidationBase
    {
        private int measuredValue;
        private string timeStamp;

        public PlotValve(int measuredValue, string timeStamp)
        {
            this.measuredValue = measuredValue;
            this.timeStamp = timeStamp;
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
                }
            }
        }

        public string TimeStamp
        {
            get { return timeStamp; }
            set
            {
                if (timeStamp != value)
                {
                    timeStamp = value;
                    OnPropertyChanged("TimeStamp");
                }
            }
        }

        public override string ToString()
        {
            return $"Value {MeasuredValue} Timestamp {TimeStamp}";
        }

        protected override void ValidateSelf()
        {
            if (string.IsNullOrWhiteSpace(string.IsNullOrWhiteSpace(TimeStamp) ? "" : TimeStamp.Trim()))
            {
                this.ValidationErrors["TimeStamp"] = "TimeStamp cannot be empty.";
            }

            if (MeasuredValue < 0)
            {
                this.ValidationErrors["Value"] = "Value must be non-negative.";
            }

        }



    }
}
