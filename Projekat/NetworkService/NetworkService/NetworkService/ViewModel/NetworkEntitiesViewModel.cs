using NetworkService.Helpers;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        // Kolekcija network entiteta (DataGrid binding)

        private ObservableCollection<Valve> valves = new ObservableCollection<Valve>();

        public ObservableCollection<Valve> Valves
        {
            get { return valves; }
            set {valves = value;}
        }

        private List<int> valvesInTable = new List<int>();

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

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (string line in readLines)
                            {
                                if (string.IsNullOrWhiteSpace(line)) continue;

                                string[] parts = line.Trim().Split('|');
                                if (parts.Length < 3) continue;

                                int id = Convert.ToInt32(parts[0]);
                                int measuredValue = Convert.ToInt32(parts[1]);
                                string dateTime = parts[2];

                                // ako nema taj ID → dodaj novi
                                if (!valvesInTable.Contains(id))
                                {
                                    string name = id % 2 == 0 ? "senzor_" : "manometar_";
                                    ValveType type = id % 2 == 0 ? ValveType.KablovskiSenzor : ValveType.DigitalniManometar;

                                    Valve newValve = new Valve(id, name + id, type, measuredValue, dateTime);
                                    valves.Add(newValve);
                                    valvesInTable.Add(id);
                                }
                                else
                                {
                                    // već postoji → update vrednosti
                                    Valve existing = valves.FirstOrDefault(v => v.Id == id);
                                    if (existing != null)
                                        existing.updateValues(measuredValue, dateTime);
                                }
                            }

                            TotalEntities = valves.Count; // tačan broj unikatnih ventila
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading file: " + ex.Message);
                    }

                    Thread.Sleep(1000); // čitaj svake sekunde
                }
            });

            readingThread.IsBackground = true;
            readingThread.Start();
        }




        // Filter UI properties
        private int selectedTypeIndex;
        public int SelectedTypeIndex
        {
            get => selectedTypeIndex;
            set => SetProperty(ref selectedTypeIndex, value);
        }

        private int selectedFilterIndex;
        public int SelectedFilterIndex
        {
            get => selectedFilterIndex;
            set => SetProperty(ref selectedFilterIndex, value);
        }

        private string searchIdText;
        public string SearchIdText
        {
            get => searchIdText;
            set => SetProperty(ref searchIdText, value);
        }

        private bool equalChecked = true;
        public bool EqualChecked
        {
            get => equalChecked;
            set => SetProperty(ref equalChecked, value);
        }

        private bool lessThanChecked;
        public bool LessThanChecked
        {
            get => lessThanChecked;
            set => SetProperty(ref lessThanChecked, value);
        }

        private bool greaterThanChecked;
        public bool GreaterThanChecked
        {
            get => greaterThanChecked;
            set => SetProperty(ref greaterThanChecked, value);
        }

        // Total entities count binding
        private int totalEntities;
        public int TotalEntities
        {
            get => totalEntities;
            set => SetProperty(ref totalEntities, value);
        }

        // Komande
        public MyICommand ResetCommand { get; }

        public NetworkEntitiesViewModel()
        {

            ResetCommand = new MyICommand(OnReset);
            this.ReadFromFile();
        }

        private void OnReset()
        {
            if (MessageBox.Show("Reset all filters?\nYou will need to set them up again.", "Clear filters?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                SelectedTypeIndex = 0;
                EqualChecked = true;
                LessThanChecked = false;
                GreaterThanChecked = false;
                SearchIdText = string.Empty;
                SelectedFilterIndex = 0;
            }
        }
    }
    }


