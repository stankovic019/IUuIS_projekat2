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

        private ValveRepository valveRepository;
        public ObservableCollection<Valve> Valves { get; set; }

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

            //foreach (Valve v in Valves)
            //{
            //    Console.WriteLine(v.Id);
            //    foreach (PlotValve i in v.LastFiveMeasurements)
            //        Console.WriteLine(i);
            //    Console.WriteLine("---------------------------------------------");
            //}
            this.Observer();
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
                                type1Count = type2Count = 0;

                                foreach (Valve v in Valves)
                                    if (v.Type == ValveType.CableSensor) type1Count++;
                                    else type2Count++;

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

                                for(int i = type1Dist; i < 10; i++)
                                    Rectangles[i].Fill = type2Color;

                                
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

    }
}
