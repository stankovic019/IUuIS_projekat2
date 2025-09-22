using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
    public class GraphViewModel : BindableBase
    {

        private int r = 25;
        private ValveRepository valveRepository;
        public ObservableCollection<Valve> Valves { get; set; }
        
        public ObservableCollection<GraphPoint> GraphPoints { get; set; }
        public double CanvasWidth { get; set; } = 900;
        public double CanvasHeight { get; set; } = 350;

        public Brush type1Color { get; set; } = Brushes.Teal;
        public double dist1 { get; set; }
        private string dist1Str;
        public string Dist1Str
        {
            get => dist1Str;
            set => SetProperty(ref dist1Str, value);
        }
        private int type1Dist = 0;
        private int type1Count = 0;
        private string type1;
        public string Type1
        {
            get => type1;
            set => SetProperty(ref type1, value);
        }

        public double dist2 {  get; set; }
        private string dist2Str;
        public string Dist2Str
        {
            get => dist2Str;
            set => SetProperty(ref dist2Str, value);
        }
        private int type2Dist = 0;
        private int type2Count = 0;
        public Brush type2Color { get; set; } = Brushes.RoyalBlue;
        private string type2;
        public string Type2
        {
            get => type2;
            set => SetProperty(ref type2, value);
        }

        public ObservableCollection<NewRectangle> Rectangles {  get; set; }
        private Valve selectedValve;
        public Valve SelectedValve
        {
            get => selectedValve;
            set
            {
                SetProperty(ref selectedValve, value);
                UpdateGraph();
            }
        }

        public GraphViewModel() 
        { 
            Type1 = ValveType.CableSensor.ToString();
            Type2 = ValveType.DigitalManometer.ToString();
            Rectangles = new ObservableCollection<NewRectangle>();
            for (int i = 0; i < 10; ++i)
                Rectangles.Add(new NewRectangle
                {
                    Fill = Brushes.Black,
                });
            valveRepository = ValveRepository.Instance;
            Valves = valveRepository.Valves;

            GraphPoints = new ObservableCollection<GraphPoint>();
            valveRepository = ValveRepository.Instance;
            Valves = valveRepository.Valves;

            this.Observer();
        }

        private void UpdateGraph()
        {
            if (SelectedValve == null || SelectedValve.LastFiveMeasurements == null)
            {
                GraphPoints.Clear();
                return;
            }

            GraphPoints.Clear();

            ObservableCollection<PlotValve> measurements = SelectedValve.LastFiveMeasurements;

            int minValue = measurements.Min(m => m.MeasuredValue);
            int maxValue = measurements.Max(m => m.MeasuredValue);

            int valueRange = Math.Max(maxValue - minValue, 5); 
            int paddedMin = minValue - valueRange / 4; 
            int paddedMax = maxValue + valueRange / 4; 

            for (int i = 0; i < measurements.Count; i++)
            {
                PlotValve v = measurements[i];

                double xPosition = (CanvasWidth / (measurements.Count + 1)) * (i + 1); 
                double yPosition = CanvasHeight - 20 - ((v.MeasuredValue - paddedMin) / (double)(paddedMax - paddedMin)) * (CanvasHeight - 40);

                v.TimeStamp = FormatTime(v.TimeStamp);

                GraphPoint gp = new GraphPoint(v)
                {
                    X = xPosition,
                    Y = yPosition,
                    TimeLabelY = CanvasHeight + 10
                };

                if(i > 0)
                {
                    GraphPoint prev = GraphPoints[i - 1];
                    (gp.PreviousX, gp.PreviousY) = FindCoordinatesOnACircle(prev.X, prev.Y, gp.X, gp.Y, r);
                    gp.ShowLine = true;
                }

                GraphPoints.Add(gp);
            }
        }

        private string FormatTime(string timestamp)
        {
            //18-Sep-25 04:13:36
            string[] splits = timestamp.Split(' ');
            //04:13:36
            string[] timeSplitted = splits[1].Split(':');
            int hour = int.Parse(timeSplitted[0]);
            int minute = int.Parse(timeSplitted[1]);
            int seconds = int.Parse(timeSplitted[2]);

            //if half a minute passed - minute rounding is one up
            if (seconds > 30) minute++;

            //reseting minute if needed
            if(minute >= 60)
            {
                minute = 0;
                hour++;
            }

            hour = hour == 24 ? 0 : hour; //reseting time

            StringBuilder sb = new StringBuilder();
            
            if (hour < 10) sb.Append($"0{hour}");
            else sb.Append(hour);

            sb.Append(":");

            if (minute < 10) sb.Append($"0{minute}");
            else sb.Append(minute);

            return sb.ToString();
        }



        private void Observer()
        {
            var observeThread = new Thread(
                () =>
                {
                    while (true)
                    {
                        try
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                UpdateValveDistribution();
                                UpdateGraph();
                            });
                        }
                        catch (Exception ex)
                        {
                        }

                        Thread.Sleep(1000);
                    }
                });
            observeThread.IsBackground = true;
            observeThread.Start();
        }

        private void UpdateValveDistribution()
        {
            type1Count = type2Count = 0;

            foreach (Valve v in Valves)
                if (v.Type == ValveType.CableSensor) type1Count++;
                else type2Count++;

            if (Valves.Count > 0)
            {
                dist1 = type1Count * 100 / (double)Valves.Count;
                Dist1Str = $"{dist1:F2}%";
                dist2 = type2Count * 100 / (double)Valves.Count;
                Dist2Str = $"{dist2:F2}%";

                if (dist1 > dist2)
                {
                    type1Dist = Convert.ToInt32(Math.Ceiling(dist1 / 10));
                    type2Dist = Convert.ToInt32(Math.Floor(dist2 / 10));
                }
                else
                {
                    type1Dist = Convert.ToInt32(Math.Floor(dist1 / 10));
                    type2Dist = Convert.ToInt32(Math.Ceiling(dist2 / 10));
                }

                for (int i = 0; i < type1Dist; i++)
                    Rectangles[i].Fill = type1Color;

                for (int i = type1Dist; i < 10; i++)
                    Rectangles[i].Fill = type2Color;
            }
        }

        private (double, double) FindCoordinatesOnACircle(double x1, double y1, double x2, double y2, double r)
        {
            //centar kružnice mi je prethodna tacka -> (a,b) = (x1,y1)

            double k = (y2 - y1) / (x2 - x1); //izracunam koeficijent pravca prave
            double n = y1 - k * x1; //izracunam n

            //racunanje koeficijenata
            double a = Math.Pow(k, 2) + 1;
            double b = 2 * k * (n - y1) - 2 * x1;
            double c = Math.Pow(x1, 2) + Math.Pow((n - y1), 2) - Math.Pow(r, 2);

            //koreni kvadratne jednacine
            double pointX1 = (-b + Math.Sqrt((Math.Pow(b, 2) - 4 * a * c))) / (2 * a);
            double pointX2 = (-b - Math.Sqrt((Math.Pow(b, 2) - 4 * a * c))) / (2 * a);

            //racunanje odgovarajuce y vrednosti
            double pointY1 = k * pointX1 + n;
            double pointY2 = k * pointX2 + n;

            //racunanje euklidske udaljenosti izmedju tacke na kruznici i centra sledece kruznice
            double distanceX1Y1 = Math.Sqrt(Math.Pow(x2 - pointX1, 2) + Math.Pow(y2 - pointY1, 2));
            double distanceX2Y2 = Math.Sqrt(Math.Pow(x2 - pointX2, 2) + Math.Pow(y2 - pointY2, 2));

            //vracam tacku cija je euklidska duzina manja
            if(distanceX1Y1 < distanceX2Y2)
                return (pointX1, pointY1);
            else
                return (pointX2, pointY2);

        }



    }
}
