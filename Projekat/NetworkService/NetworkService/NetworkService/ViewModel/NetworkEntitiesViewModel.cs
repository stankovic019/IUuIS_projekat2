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
using NetworkService.DTOs;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        private HistoryRepository historyRepository;
        private Stack<IUndoService> undoStack;
        private HistoryDtoRepository historyDtoRepository;
        private ObservableCollection<HistoryDto> historyDtos;
        public ObservableCollection<HistoryDto> HistoryDtos
        {
            get => historyDtos;
        }
        ValveRepository valveRepository;
        public ObservableCollection<Valve> Valves { get; private set; }


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

                        lock (fileLock)
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
                                {
                                    existing.updateValues(measuredValue, dateTime);
                                    existing.insertLatestValue(measuredValue, dateTime);
                                }
                            }

                            var previouslySelected = SelectedValve;
                            if (previouslySelected != null && Valves.Contains(previouslySelected))
                            {
                                SelectedValve = previouslySelected;
                            }

                            TotalEntities = Valves.Count;
                            RefreshFilter();
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

        private int selectedTypeIndex;
        public int SelectedTypeIndex
        {
            get => selectedTypeIndex;
            set
            {
                if (selectedTypeIndex != value)
                {
                    SetProperty(ref selectedTypeIndex, value);
                    RefreshFilter();
                }
            }
        }

        private int selectedFilterIndex;
        public int SelectedFilterIndex
        {
            get => selectedFilterIndex;
            set
            {
                if (selectedFilterIndex != value)
                {
                    SetProperty(ref selectedFilterIndex, value);
                    RefreshFilter();
                }
            }
        }

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.All(char.IsDigit))
                    return;

                if (searchText != value)
                {
                    SetProperty(ref searchText, value);
                    RefreshFilter();
                }
            }
        }

        private bool equalChecked;
        public bool EqualChecked
        {
            get => equalChecked;
            set
            {
                if (equalChecked != value)
                {
                    SetProperty(ref equalChecked, value);
                    if (value)
                    {
                        LessThanChecked = false;
                        GreaterThanChecked = false;
                    }
                    RefreshFilter();
                }
            }
        }

        private bool lessThanChecked;
        public bool LessThanChecked
        {
            get => lessThanChecked;
            set
            {
                if (lessThanChecked != value)
                {
                    SetProperty(ref lessThanChecked, value);
                    if (value)
                    {
                        EqualChecked = false;
                        GreaterThanChecked = false;
                    }
                    RefreshFilter();
                }
            }
        }

        private bool greaterThanChecked;
        public bool GreaterThanChecked
        {
            get => greaterThanChecked;
            set
            {
                if (greaterThanChecked != value)
                {
                    SetProperty(ref greaterThanChecked, value);
                    if (value)
                    {
                        EqualChecked = false;
                        LessThanChecked = false;
                    }
                    RefreshFilter();
                }
            }
        }

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

        private HistoryDto selectedHistoryItem = new HistoryDto(string.Empty, "");
        public HistoryDto SelectedHistoryItem
        {
            get => selectedHistoryItem;
            set
            {
                if (selectedHistoryItem != value)
                {
                    selectedHistoryItem = value;
                    OnPropertyChanged(nameof(SelectedHistoryItem));
                }
            }
        }

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                SetProperty(ref isActive, value);
                if (isActive)
                    if (ValvesView != null)
                        ValvesView.Refresh();
            }
        }

        private ICollectionView valvesView;
        public ICollectionView ValvesView
        {
            get => valvesView;
            private set => SetProperty(ref valvesView, value);
        }

        private void RefreshFilter()
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ValvesView != null)
                {
                    var filtered = Valves.Where(v =>
                    {
                        if (SelectedTypeIndex == 1 && v.Type != ValveType.CableSensor) return false;
                        if (SelectedTypeIndex == 2 && v.Type != ValveType.DigitalManometer) return false;

                        if (SelectedFilterIndex == 1 && v.Validation != ValueValidation.Normal) return false;
                        if (SelectedFilterIndex == 2 && v.Validation != ValueValidation.High) return false;
                        if (SelectedFilterIndex == 3 && v.Validation != ValueValidation.Low) return false;

                        if (!string.IsNullOrWhiteSpace(SearchText) && int.TryParse(SearchText, out int value))
                        {
                            if (EqualChecked && v.MeasuredValue != value) return false;
                            if (LessThanChecked && v.MeasuredValue >= value) return false;
                            if (GreaterThanChecked && v.MeasuredValue <= value) return false;
                        }

                        return true;
                    }).ToList();

                    ValvesView = CollectionViewSource.GetDefaultView(filtered);

                    var previouslySelected = SelectedValve;
                    if (previouslySelected != null && Valves.Contains(previouslySelected))
                    {
                        SelectedValve = previouslySelected;
                    }
                }
            });
        }

        private bool ValveFilter(object obj)
        {
            return true;
        }


        public MyICommand ResetCommand { get; }
        public MyICommand DeleteCommand { get; }

        public NetworkEntitiesViewModel()
        {
            ResetCommand = new MyICommand(OnReset);
            DeleteCommand = new MyICommand(OnDelete);
            valveRepository = ValveRepository.Instance;
            Valves = valveRepository.Valves;
            ValvesView = CollectionViewSource.GetDefaultView(Valves);
            ValvesView.Filter = ValveFilter;
            if (ValvesView.Filter == null)
            SelectedTypeIndex = 0;
            SelectedFilterIndex = 0;
            SearchText = string.Empty;
            historyRepository = HistoryRepository.Instance;
            undoStack = historyRepository.UndoStack;
            historyDtoRepository = HistoryDtoRepository.Instance;
            historyDtos = historyDtoRepository.HistoryDtos;
            this.ObserveWindow();
        }

        private void OnReset()
        {
            try
            {
                if (MessageBox.Show("Reset all filters?\nYou will need to set them up again.", "Clear filters?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    SelectedTypeIndex = 0;
                    EqualChecked = false;
                    LessThanChecked = false;
                    GreaterThanChecked = false;
                    SearchText = string.Empty;
                    SelectedFilterIndex = 0;
                    SelectedHistoryItem = new HistoryDto(string.Empty, ""); ;
                    NotificationService.Instance.ShowSuccess("", "Filters successfully reseted");
                }
            }catch(Exception ex)
            {
                NotificationService.Instance.ShowError("Error while reseting filters. Try again later.", "ERROR Reset");
            }
        }

        private async Task deleteTime()
        {
            await Task.Delay(1000);
        }

        private async void OnDelete()
        {
            if (!IsActive) return;
            if (SelectedValve == null) return;
            try
            {
                if (MessageBox.Show($"Do you really wanna delete 'Valve: {SelectedValve.Id} : {SelectedValve.Name}' ?", $"Delete Valve {SelectedValve.Id}", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    int id = SelectedValve.Id;
                    IUndoService deleteValve = new DeleteValve(this, SelectedValve);

                    Valves.Remove(SelectedValve);
                    NotificationService.Instance.ShowInfo("Please wait as the application is updating states", "Deleting...");
                    await Task.Delay(2000);
                    undoStack.Push(deleteValve);
                    HistoryDtos.Insert(0, new HistoryDto(deleteValve.getTitle(), deleteValve.getDateTime()));
                    if (ValvesView != null)
                        ValvesView.Refresh();
                    NotificationService.Instance.ShowSuccess("", "Valve successfully deleted");
                    
                }
            }
            catch (Exception ex) {
                NotificationService.Instance.ShowError($"Error while Deleting {SelectedValve}, try again later", "ERROR Delete");
            }
        }
    }
}


