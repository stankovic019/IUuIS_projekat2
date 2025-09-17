using NetworkService.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace NetworkService.ViewModel
{
    public class HomePageViewModel : BindableBase
    {
        // Quick stats
        private int totalEntities;
        public int TotalEntities
        {
            get => totalEntities;
            set => SetProperty(ref totalEntities, value);
        }

        private double lastPressure;
        public double LastPressure
        {
            get => lastPressure;
            set => SetProperty(ref lastPressure, value);
        }

        private int activeFilters;
        public int ActiveFilters
        {
            get => activeFilters;
            set => SetProperty(ref activeFilters, value);
        }

        // Recent History
        public ObservableCollection<HistoryItem> RecentHistory { get; set; } = new ObservableCollection<HistoryItem>();

        // Saved Filters
        public ObservableCollection<FilterItem> SavedFilters { get; set; } = new ObservableCollection<FilterItem>();

        public HomePageViewModel()
        {
            this.ReadFromFile();
            // Dummy inicijalni podaci, zameni pravim iz baze ili servisa
            RecentHistory.Add(new HistoryItem { Timestamp = DateTime.Now, Action = "Added Entity", Status = "OK", Undo = "Available" });
            RecentHistory.Add(new HistoryItem { Timestamp = DateTime.Now.AddMinutes(-5), Action = "Removed Filter", Status = "OK", Undo = "Available" });
            RecentHistory.Add(new HistoryItem { Timestamp = DateTime.Now.AddMinutes(-10), Action = "Updated Entity", Status = "Failed", Undo = "Unavailable" });

            SavedFilters.Add(new FilterItem { FilterName = "High Pressure", Apply = "Apply" });
            SavedFilters.Add(new FilterItem { FilterName = "Low Pressure", Apply = "Apply" });
            SavedFilters.Add(new FilterItem { FilterName = "Normal Pressure", Apply = "Apply" });
        }



        private void ReadFromFile()
        {
            var readingThread = new Thread(() =>
            {
                while (true)
                {
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        string path = @"C:\Users\Dimitrije\Documents\GitHub\IUuIS_projekat2\Projekat\NetworkService\NetworkService\NetworkService\public\files\Log.txt";

                        try
                        {
                            

                            // Split linija po \r\n ili \n (sigurno)
                            string[] readLines = FileAccessManager.ReadFromFile(path);

                            if (readLines.Length == 0)
                                return;

                            // TotalEntities je broj linija
                            int total = readLines.Length;

                            // Parsiranje poslednje linije
                            string lastLine = readLines[total - 1];
                            string[] parts = lastLine.Split('|');
                            int lastPressureValue = Convert.ToInt32(parts[1]);

                            // Update UI property-ja u glavnoj niti
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                TotalEntities = total;
                                LastPressure = lastPressureValue;
                            });
                        }
                        catch (Exception ex)
                        {
                            // Log ili ignorisi greške
                            Console.WriteLine("Error reading file: " + ex.Message);
                        }
                    });

                    Thread.Sleep(1000); // Pauza da ne troši 100% CPU
                }
            });

            readingThread.IsBackground = true;
            readingThread.Start();
        }



        public class HistoryItem
        {
            public DateTime Timestamp { get; set; }
            public string Action { get; set; }
            public string Status { get; set; }
            public string Undo { get; set; }
        }

        public class FilterItem
        {
            public string FilterName { get; set; }
            public string Apply { get; set; }
        }
    }
}

