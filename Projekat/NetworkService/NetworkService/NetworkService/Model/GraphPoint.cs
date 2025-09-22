using NetworkService.Helpers;
using System.Windows.Media;

namespace NetworkService.Model
{
    public class GraphPoint : BindableBase
    {
        private double x;
        private double y;
        private double previousX;
        private double previousY;
        private PlotValve plotValve; //value and timestamp
        private Brush dotColor;
        private bool showLine;
        private int xSpacing;

        public GraphPoint(PlotValve pv)
        {
            this.plotValve = pv;
            this.DotColor = (plotValve.MeasuredValue >= 5 && plotValve.MeasuredValue <= 16) ? Brushes.ForestGreen : Brushes.DarkRed;
            this.ShowLine = false;
            this.xSpacing = plotValve.MeasuredValue > 9 ? 12 : 6;
        }

        public double X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        public double Y
        {
            get => y;
            set => SetProperty(ref y, value);
        }

        public double PreviousX
        {
            get => previousX;
            set => SetProperty(ref previousX, value);
        }

        public double PreviousY
        {
            get => previousY;
            set => SetProperty(ref previousY, value);
        }

        public PlotValve PlotValve
        {
            get => plotValve;
            set => SetProperty(ref plotValve, value);
        }

        public int MeasuredValue { get => PlotValve.MeasuredValue; }
        public string TimeStamp { get => PlotValve.TimeStamp; }

        public Brush DotColor
        {
            get => dotColor;
            set => SetProperty(ref dotColor, value);
        }

        public bool ShowLine
        {
            get => showLine;
            set => SetProperty(ref showLine, value);
        }

        public double DotX => X - 25; //centerin the 50px wide dot

        public double DotY => Y - 25;

        public double TextX => X - xSpacing; //approximate center for text

        public double TextY => Y - 15;

        public double TimeLabelX => X - 24; // Center the label under the point

        public double TimeLabelY { get; set; }
    }
}