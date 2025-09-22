using NetworkService.Helpers;

namespace NetworkService.Model
{
    public class NewLine : BindableBase
    {
        private double x1;
        private double y1;
        private double x2;
        private double y2;

        private int source;
        private int destination;

        public double X1
        {
            get { return x1; }
            set => SetProperty(ref x1, value);
        }
        public double Y1
        {
            get { return y1; }
            set => SetProperty(ref y1, value);
        }
        public double X2
        {
            get { return x2; }
            set => SetProperty(ref x2, value);
        }
        public double Y2
        {
            get { return y2; }
            set => SetProperty(ref y2, value);
        }
        public int Source
        {
            get { return source; }
            set => SetProperty(ref source, value);
        }
        public int Destination
        {
            get { return destination; }
            set => SetProperty(ref destination, value);
        }
    }
}