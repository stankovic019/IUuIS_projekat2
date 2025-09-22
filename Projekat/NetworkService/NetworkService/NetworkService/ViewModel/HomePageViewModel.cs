using NetworkService.DTOs;
using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Repositories;
using NetworkService.Services.UndoServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace NetworkService.ViewModel
{
    public class HomePageViewModel : BindableBase
    {

        private HistoryRepository historyRepository;
        private Stack<IUndoService> undoStack;
        private string[] toolTips;

        private DispatcherTimer toolTipTimer;
        private DispatcherTimer clockTimer;

        private HistoryDtoRepository historyDtoRepository;
        private ObservableCollection<HistoryDto> historyDtos;
        public ObservableCollection<HistoryDto> HistoryDtos
        {
            get { return new ObservableCollection<HistoryDto>(historyDtos.Take(3)); }
        }

        ValveRepository valveRepository;
        private ObservableCollection<Valve> valves;
        public ObservableCollection<Valve> Valves
        {
            get { return new ObservableCollection<Valve>(valves.Take(3)); }
        }

        private int cntToolTips = 3;

        private string toolTip;
        public string ToolTip
        {
            get => toolTip;
            set => SetProperty(ref toolTip, value);
        }

        private int toolTipFontSize = 25;
        public int ToolTipFontSize
        {
            get => toolTipFontSize;
            set => SetProperty(ref toolTipFontSize, value);
        }


        private ObservableCollection<Valve> lastThreeValves = new ObservableCollection<Valve>();
        public ObservableCollection<Valve> LastThreeValves
        {
            get => lastThreeValves;
            set => SetProperty(ref lastThreeValves, value);
        }


        private int totalEntities = 0;
        public int TotalEntities
        {
            get => totalEntities;
            set => SetProperty(ref totalEntities, value);
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
            historyRepository = HistoryRepository.Instance;
            undoStack = historyRepository.UndoStack;
            historyDtoRepository = HistoryDtoRepository.Instance;
            historyDtos = historyDtoRepository.HistoryDtos;
            this.readToolTips();
            this.StartToolTipChange();
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

                        lock (fileLock) //pausing the current thread so it will overload
                        {
                            if (!File.Exists(path))
                            {
                                Thread.Sleep(1000);
                                continue;
                            }

                            readLines = FileAccessManager.ReadFromFile(path);

                        }


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
                                TotalEntities = valves.Count;

                            }

                            LastThreeValves.Clear();
                            for (int i = total - 2; i >= Math.Max(total - 4, 0); i--)
                            {
                                string line = readLines[i];
                                string[] splits = line.Split('|');
                                int smallId = Convert.ToInt32(splits[0]);

                                Valve v = valves.FirstOrDefault(val => val.Id == smallId);
                                if (v != null)
                                {
                                    LastThreeValves.Add(new Valve(v));
                                }
                            }

                            if (lastPressureValue < 5 || lastPressureValue > 16)
                                BorderBrush = Brushes.Red;
                            else
                                BorderBrush = Brushes.Black;
                        });
                    }
                    catch (Exception ex)
                    {
                    }

                    Thread.Sleep(1000);
                }
            });

            readingThread.IsBackground = true;
            readingThread.Start();
        }

        public void readToolTips()
        {
            string filePath = "C:\\Users\\Dimitrije\\Documents\\GitHub\\IUuIS_projekat2\\Projekat\\NetworkService\\NetworkService\\NetworkService\\public\\files\\ToolTips.txt";

            toolTips = FileAccessManager.ReadFromFile(filePath);

            ToolTip = toolTips[new Random().Next(0, toolTips.Length)]; //initial tooltip

        }

        private void StartToolTipChange()
        {
            toolTipTimer = new DispatcherTimer();
            toolTipTimer.Interval = TimeSpan.FromMilliseconds(6900);
            toolTipTimer.Tick += ToolTipTimer_Tick;
            toolTipTimer.Start();
        }

        private void ToolTipTimer_Tick(object sender, EventArgs e)
        {

            Random r = new Random();

            // on every five tooltips it will show a clock
            bool showClock = cntToolTips == 4;

            if (showClock)
            {
                ToolTipFontSize = 80;
                cntToolTips = 0;
                StartClockTooltip();
            }
            else
            {
                ToolTipFontSize = 25;
                cntToolTips++;
                if (clockTimer != null)
                {
                    clockTimer.Stop();
                    clockTimer = null;
                }
                ToolTip = toolTips[r.Next(toolTips.Length)];
            }
        }

        private void StartClockTooltip()
        {
            if (clockTimer == null)
            {
                clockTimer = new DispatcherTimer();
                clockTimer.Interval = TimeSpan.FromMilliseconds(1000);
                clockTimer.Tick += (s, e) =>
                {
                    ToolTip = DateTime.Now.ToString("HH:mm:ss");
                };
            }
            clockTimer.Start();
            ToolTip = DateTime.Now.ToString("HH:mm:ss");
        }

    }
}

