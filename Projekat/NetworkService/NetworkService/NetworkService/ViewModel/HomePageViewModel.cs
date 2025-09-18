using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Repositories;
using NetworkService.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetworkService.ViewModel
{
    public class HomePageViewModel : BindableBase
    {
        // Quick stats

        ValveRepository valveRepository;
        public ObservableCollection<Valve> valves;
        
        private string totalEntites = "L..";
        public string TotalEntities
        {
            get => totalEntites;
            set => SetProperty(ref totalEntites, value);
        }

        private string lastEntityName = "Loading...";
        public string LastEntityName
        {
            get => lastEntityName;
            set => SetProperty(ref lastEntityName, value);
        }
        
        private string lastPressure = "Loading...";
        public string LastPressure
        {
            get => lastPressure;
            set => SetProperty(ref lastPressure, value);
        }

        private Brush borderBrush = Brushes.Black;
        public Brush BorderBrush
        {
            get => borderBrush;
            set => SetProperty(ref borderBrush, value);
        }

        private int activeFilters;
        public int ActiveFilters
        {
            get => activeFilters;
            set => SetProperty(ref activeFilters, value);
        }

        public HomePageViewModel()
        {   
            valveRepository = ValveRepository.Instance;
            valves = valveRepository.Valves;
            this.ReadFromFile();
          
        }


        private readonly object fileLock = new object();
        private void ReadFromFile()
        {
            var readingThread = new Thread(() =>
            {
            while (true)
            {
                try
                {
                    string path = @"C:\Users\Dimitrije\Documents\GitHub\IUuIS_projekat2\Projekat\NetworkService\NetworkService\NetworkService\public\files\Log.txt";
                    string[] readLines;

                    lock (fileLock) // čuvaj od preplitanja čitanja/pisanja
                    {
                        if (!File.Exists(path))
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        readLines = FileAccessManager.ReadFromFile(path);

                    }

                        // Update UI 
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            int total = readLines.Length;
                            string lastLine = readLines[total - 1];
                            string[] parts = lastLine.Split('|');
                            int lastPressureValue = Convert.ToInt32(parts[1]);
                            int id = Convert.ToInt32(parts[0]);

                            Valve last = valves.FirstOrDefault(v => v.Id == id);

                            if (last != null)
                            {
                                LastEntityName = last.Name;
                                LastPressure = parts[1] + " MPa";
                                TotalEntities = valves.Count.ToString();

                            }
                            else
                            {
                                LastEntityName = "Loading...";
                                LastPressure = "Loading...";
                                TotalEntities = "L..";
                            }

                            if (lastPressureValue < 5 || lastPressureValue > 16)
                                BorderBrush = Brushes.Red;
                            else
                                BorderBrush = Brushes.Black;

                        });
                        }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading file: " + ex.Message);
                    }

                    Thread.Sleep(1000); 
                }
            });

            readingThread.IsBackground = true;
            readingThread.Start();
        }

    }
}

