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
using NetworkService.Repositories;
using System.ComponentModel;
using System.Windows.Data;
using NetworkService.Services;
using NetworkService.Services.UndoServices;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        private Stack<IUndoService> undoStack = new Stack<IUndoService>();

        ValveRepository valveRepository;
        private ObservableCollection<Valve> valves = new ObservableCollection<Valve>();
        public ObservableCollection<Valve> Valves { get; } = new ObservableCollection<Valve>();

        private readonly object fileLock = new object();

        private void ObserveWindow()
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

                                Valve existing = Valves.FirstOrDefault(v => v.Id == id);
                                if (existing != null)
                                    existing.updateValues(measuredValue, dateTime);
                            }

                            var previouslySelected = SelectedValve;
                            ValvesView.Refresh();
                            if (previouslySelected != null && Valves.Contains(previouslySelected))
                            {
                                SelectedValve = previouslySelected;
                            }

                            TotalEntities = Valves.Count;
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
            set
            {
                SetProperty(ref selectedTypeIndex, value);
                var previouslySelected = SelectedValve;
            }
        }

        private int selectedFilterIndex;
        public int SelectedFilterIndex
        {
            get => selectedFilterIndex;
            set
            {
                SetProperty(ref selectedFilterIndex, value);
                var previouslySelected = SelectedValve;
                ValvesView.Refresh();
                if (previouslySelected != null && Valves.Contains(previouslySelected))
                {
                    SelectedValve = previouslySelected;
                }

            }
        }

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                // Validacija: dozvoljava samo cifre i prazno
                if (!string.IsNullOrEmpty(value) && !value.All(char.IsDigit))
                    return;

                SetProperty(ref searchText, value);
                var previouslySelected = SelectedValve;
            }
        }


        private bool equalChecked;
        public bool EqualChecked
        {
            get => equalChecked;
            set
            {
                SetProperty(ref equalChecked, value);
                var previouslySelected = SelectedValve;
                ValvesView.Refresh();
                if (previouslySelected != null && Valves.Contains(previouslySelected))
                {
                    SelectedValve = previouslySelected;
                }

            }
        }

        private bool lessThanChecked;
        public bool LessThanChecked
        {
            get => lessThanChecked;
            set
            {
                SetProperty(ref lessThanChecked, value);
                var previouslySelected = SelectedValve;
            }
        }

        private bool greaterThanChecked;
        public bool GreaterThanChecked
        {
            get => greaterThanChecked;
            set
            {
                SetProperty(ref greaterThanChecked, value);
                var previouslySelected = SelectedValve;
                ValvesView.Refresh();
                if (previouslySelected != null && Valves.Contains(previouslySelected))
                {
                    SelectedValve = previouslySelected;
                }

            }
        }

        // Total entities count binding
        private int totalEntities;
        public int TotalEntities
        {
            get => totalEntities;
            set
            {
                SetProperty(ref totalEntities, value);
                var previouslySelected = SelectedValve;
            }
        }

        private Valve selectedValve;
        public Valve SelectedValve
        {
            get => selectedValve;
            set => SetProperty(ref selectedValve, value);
        }

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                SetProperty(ref isActive, value);
                if (isActive)
                    ValvesView.Refresh();
            }
        }

        private ICollectionView valvesView;
        public ICollectionView ValvesView
        {
            get => valvesView;
            private set => SetProperty(ref valvesView, value);
        }

        private bool ValveFilter(object obj)
        {
            Valve v = obj as Valve;
            if (v == null) return false;

            // filter po tipu
            if (SelectedTypeIndex == 1 && v.Type != ValveType.CableSensor) return false;
            if (SelectedTypeIndex == 2 && v.Type != ValveType.DigitalManometer) return false;

            // filter po validaciji
            if (SelectedFilterIndex == 1 && v.Validation != ValueValidation.Normal) return false;
            if (SelectedFilterIndex == 2 && v.Validation != ValueValidation.High) return false;
            if (SelectedFilterIndex == 3 && v.Validation != ValueValidation.Low) return false;

            // filter po vrednosti
            if (!string.IsNullOrWhiteSpace(SearchText) && int.TryParse(SearchText, out int value))
            {
                if (EqualChecked && v.MeasuredValue != value) return false;
                if (LessThanChecked && v.MeasuredValue >= value) return false;
                if (GreaterThanChecked && v.MeasuredValue <= value) return false;
            }

            return true;
        }


        // Komande
        public MyICommand ResetCommand { get; }
        public MyICommand AddCommand { get; }
        public MyICommand DeleteCommand { get; }
        public MyICommand UndoCommand { get; }
        public MyICommand UndoAllCommand { get; }

        public NetworkEntitiesViewModel()
        {
            ResetCommand = new MyICommand(OnReset);
            AddCommand = new MyICommand(OnAdd);
            DeleteCommand = new MyICommand(OnDelete);
            UndoCommand = new MyICommand(OnUndo);
            UndoAllCommand = new MyICommand(OnUndoAll);
            valveRepository = ValveRepository.Instance;
            Valves = valveRepository.Valves;
            ValvesView = CollectionViewSource.GetDefaultView(Valves);
            ValvesView.Filter = ValveFilter;
            this.ObserveWindow();
        }

        private void OnReset()
        {
            if (MessageBox.Show("Reset all filters?\nYou will need to set them up again.", "Clear filters?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                SelectedTypeIndex = 0;
                EqualChecked = false;
                LessThanChecked = false;
                GreaterThanChecked = false;
                SearchText = string.Empty;
                SelectedFilterIndex = 0;
            }
        }

        private void OnAdd()
        {
            // neće raditi ako view nije aktivan
            if (!IsActive) return;

            MessageBox.Show("add nigga :D");
        }

        private void OnDelete()
        {
            if (!IsActive) return;
            if (SelectedValve == null) return;

            if (MessageBox.Show($"Do you really wanna delete Valve: {SelectedValve.Id} : {SelectedValve.Name} ?\n" +
                $"This change is irreversible?", $"Delete Valve {SelectedValve.Id}", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                int id = SelectedValve.Id;
                undoStack.Push(new DeleteValve(this.Valves, SelectedValve));
                Valves.Remove(SelectedValve);
                ValvesView.Refresh();
                if(!Valves.Contains(SelectedValve))
                    NotificationService.Instance.ShowSuccess($"Valve {id} deleted", "SUCCESS Delete");
                else
                    NotificationService.Instance.ShowError($"Valve {id} is not deleted", "ERROR Delete");
            }
        }

        private void OnUndo()
        {   
            if(!IsActive) return;

            if (undoStack.Any())
            {
                IUndoService action = undoStack.Pop();
                action.Undo();
            }
        }

        private void OnUndoAll()
        {
            if (!IsActive) return;

            if (MessageBox.Show("Do you want to Undo All changes and go back to the beginning?", "Undo all", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                while (undoStack.Any())
                {
                    var action = undoStack.Pop();
                    action.Undo();
                }
            }
        }

    }
}


