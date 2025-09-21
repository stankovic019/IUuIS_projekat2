using NetworkService.DTOs;
using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Repositories;
using NetworkService.Services;
using NetworkService.Services.UndoServices;
using NetworkService.Views;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {

        NotificationManager notificationManager;

        private HistoryRepository historyRepository;
        private Stack<IUndoService> undoStack;
        private HistoryDtoRepository historyDtoRepository;
        private ObservableCollection<HistoryDto> historyDtos;
        public ObservableCollection<HistoryDto> HistoryDtos
        {
            get => historyDtos;
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

        ValveRepository valveRepository;

        private ObservableCollection<Valve> valves;

        private int maxCount;
        public int MaxCount
        {
            get => maxCount;
            private set => SetProperty(ref maxCount, value);
        }

        public NetworkEntitiesViewModel NetworkEntitiesVM { get; }

        private NetworkEntitiesView networkEntitiesViewInstance;
        public AddEntityViewModel AddEntityVM { get; }
        private AddEntityView addEntityViewInstance;

        public NetworkDisplayViewModel NetworkDisplayVM { get; }
        private NetworkDisplayView networkDisplayViewInstance;

        private string title = "Infrastructural systems simulator";

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private string headerIcon = "/public/icons/gear-logo-trans.png";
        public string HeaderIcon
        {
            get => headerIcon;
            set => SetProperty(ref headerIcon, value);
        }

        private object currentView;
        public object CurrentView
        {
            get => currentView;
            set => SetProperty(ref currentView, value);
        }

        private bool toolGridVisible;
        public bool ToolGridVisible
        {
            get => toolGridVisible;
            set => SetProperty(ref toolGridVisible, value);
        }

        public bool AddBtnVisible { get; set; }
        public bool DeleteBtnVisible { get; set; }
        public bool UndoBtnVisible { get; set; }
        public bool UndoAllBtnVisible { get; set; }
        public bool SaveBtnVisible { get; set; }
        public bool ClearBtnVisible { get; set; }
        public bool DiscardBtnVisible { get; set; }
        public int Span { get; set; }

        public MyICommand ShowNotificationCommand { get; }
        public MyICommand HomeCommand { get; }
        public MyICommand NetEntitiesCommand { get; }
        public MyICommand NetDisplayCommand { get; }
        public MyICommand GraphCommand { get; }
        public MyICommand ExitCommand { get; }
        public MyICommand AddCommand { get; }
        public MyICommand UndoCommand { get; }
        public MyICommand UndoAllCommand { get; }
        private void SetButtons(int g, int b1, int b2, int b3, int b4, int b5, int b6, int b7)
        {
            ToolGridVisible = g == 1;
            Span = ToolGridVisible ? 0 : 2;
            AddBtnVisible = b1 == 1;
            DeleteBtnVisible = b2 == 1;
            UndoBtnVisible = b3 == 1;
            UndoAllBtnVisible = b4 == 1;
            SaveBtnVisible = b5 == 1;
            ClearBtnVisible = b6 == 1;
            DiscardBtnVisible = b7 == 1;

            OnPropertyChanged(nameof(AddBtnVisible));
            OnPropertyChanged(nameof(DeleteBtnVisible));
            OnPropertyChanged(nameof(UndoBtnVisible));
            OnPropertyChanged(nameof(UndoAllBtnVisible));
            OnPropertyChanged(nameof(SaveBtnVisible));
            OnPropertyChanged(nameof(ClearBtnVisible));
            OnPropertyChanged(nameof(DiscardBtnVisible));
            OnPropertyChanged(nameof(ToolGridVisible));
            OnPropertyChanged(nameof(Span));
        }

        private void OnHomeCommand()
        {
            Title = "Infrastructural systems simulator";
            HeaderIcon = "/public/icons/gear-logo-trans.png";
            SetButtons(0, 0, 0, 0, 0, 0, 0, 0);
            CurrentView = new HomePageView();
            NetworkEntitiesVM.IsActive = false;
            AddEntityVM.IsActive = false;
            NetworkDisplayVM.IsActive = false;
        }

        private void OnNetEntitiesCommand()
        {
            Title = "Network Entities";
            HeaderIcon = "/public/icons/clock-icon.png";
            SetButtons(1, 1, 1, 1, 1, 0, 0, 0);
            CurrentView = networkEntitiesViewInstance;
            NetworkEntitiesVM.IsActive = true;
            AddEntityVM.IsActive = false;
            NetworkDisplayVM.IsActive = false;
        }

        private void OnNetDisplayCommand()
        {
            Title = "Network Display";
            HeaderIcon = "/public/icons/network-display-icon.png";
            SetButtons(1, 0, 0, 1, 1, 0, 0, 0);
            CurrentView = networkDisplayViewInstance;
            NetworkEntitiesVM.IsActive = false;
            AddEntityVM.IsActive = false;
            NetworkDisplayVM.IsActive = true;
        }

        private void OnGraphCommand()
        {
            Title = "Graph";
            HeaderIcon = "/public/icons/graph-icon.png";
            SetButtons(0, 0, 0, 0, 0, 0, 0, 0);
            CurrentView = null;
            NetworkEntitiesVM.IsActive = false;
            AddEntityVM.IsActive = false;
            NetworkDisplayVM.IsActive = false;
            
        }

        private async void OnExitCommand()
        {
            if (MessageBox.Show("Exiting will save all the changes, including added and deleted valves.\n" +
                "Exit application?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                //ValveRepository.Instance.StoreValves();
                NotificationService.Instance.ShowSuccess("", "Data saved successfully");
                await Task.Delay(1500);
                Application.Current.Shutdown();
            }
        }

        public void OnUndoCommand()
        {
            if (!NetworkEntitiesVM.IsActive && !NetworkDisplayVM.IsActive) return;
            try
            {
                if (undoStack.Any())
                {
                    IUndoService action = undoStack.Pop();
                    action.Undo();
                    HistoryDto forRemoveFromHistory = HistoryDtos.FirstOrDefault(h => h.ActionName == action.getTitle());
                    HistoryDtos.Remove(forRemoveFromHistory);
                    NotificationService.Instance.ShowSuccess("", "Undo successful");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError("An error occured while trying to undo changes.", "Undo aborted");
            }
        }

        public void OnUndoAllCommand()
        {
            if (!NetworkEntitiesVM.IsActive && !NetworkDisplayVM.IsActive) return;
            try
            {
                if (NetworkEntitiesVM.SelectedHistoryItem != null && NetworkEntitiesVM.IsActive)
                    SelectedHistoryItem = NetworkEntitiesVM.SelectedHistoryItem;
                else if(NetworkDisplayVM.SelectedHistoryItem != null && NetworkDisplayVM.IsActive) 
                    SelectedHistoryItem = NetworkDisplayVM.SelectedHistoryItem;
                else
                    SelectedHistoryItem = new HistoryDto(string.Empty, string.Empty);

                if (SelectedHistoryItem.ActionName == string.Empty)
                {
                    if (MessageBox.Show("Do you want to Undo All changes and go back to the beginning?", "Undo all", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        while (undoStack.Any())
                        {
                            if (undoStack.Any())
                            {
                                IUndoService action = undoStack.Pop();
                                action.Undo();
                                HistoryDto forRemoveFromHistory = HistoryDtos.FirstOrDefault(h => h.ActionName == action.getTitle());
                                HistoryDtos.Remove(forRemoveFromHistory);
                            }
                        }
                        NotificationService.Instance.ShowSuccess("You are back at the beginning", "Undo All successful");
                    }
                }
                else if (SelectedHistoryItem.ActionName != string.Empty)
                {
                    if (MessageBox.Show($"Do you want to Undo All changes and go back to '{SelectedHistoryItem.ActionName}' ?", "Undo all", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        HistoryDto item;
                        do
                        {
                            item = HistoryDtos.ElementAt(0);
                            if (item.ActionName != SelectedHistoryItem.ActionName)
                            {
                                if (undoStack.Any())
                                {
                                    IUndoService action = undoStack.Pop();
                                    action.Undo();
                                    HistoryDto forRemoveFromHistory = HistoryDtos.FirstOrDefault(h => h.ActionName == action.getTitle());
                                    HistoryDtos.Remove(forRemoveFromHistory);
                                }
                            }
                        } while (item.ActionName != SelectedHistoryItem.ActionName);
                        NotificationService.Instance.ShowSuccess($"You are back to the '{selectedHistoryItem.ActionName}' in history", "Undo All successful");
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError("An error occured while trying to undo all changes.", "Undo All aborted");
            }
            finally
            {
                selectedHistoryItem = new HistoryDto(string.Empty, "");
                NetworkEntitiesVM.SelectedHistoryItem = new HistoryDto("", "");
                NetworkDisplayVM.SelectedHistoryItem = new HistoryDto("", "");
            }
        }


        private async void OnAddCommand()
        {
            if (!NetworkEntitiesVM.IsActive) return;
            try
            {
                Title = "Add New Entity";
                HeaderIcon = "/public/icons/add-icon.png";
                SetButtons(1, 0, 0, 0, 0, 1, 1, 1);
                CurrentView = addEntityViewInstance;
                NetworkEntitiesVM.IsActive = false;
                AddEntityVM.IsActive = true;
                AddEntityVM.OnClose = false;
                WaitForCloseAsync();
            }
            catch (Exception ex) {
                NotificationService.Instance.ShowError("Adding a new entity is temporarily not permitted. Try again later.", "Add Entity aborted");
            }
        }

        private async Task WaitForCloseAsync()
        {
            try
            {
                while (!AddEntityVM.OnClose)
                {
                    await Task.Delay(100); 
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.ShowError("Adding a new entity is temporarily not permitted. Try again later.", "Add Entity aborted");
            }
            finally
            {
                this.OnNetEntitiesCommand();
            }
        }


        public MainWindowViewModel()
        {
            notificationManager = new NotificationManager();
            valveRepository = ValveRepository.Instance;
            valves = valveRepository.Valves;
            MaxCount = valves.Count; //initial count is max value
            //if new element is added -> new max count is the valves count
            //if element is deleted -> max count stays the same because of the simulation
            //i.e. if element id == 3 is deleted, if we shift counting, the last element
            //will not be affected
            valves.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    MaxCount = Math.Max(MaxCount + 1, valves.Count);
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    //if element is removed - do nothing - it will simulate that element, but wont affect application
                }
            };
            historyRepository = HistoryRepository.Instance;
            undoStack = historyRepository.UndoStack;
            historyDtoRepository = HistoryDtoRepository.Instance;
            historyDtos = historyDtoRepository.HistoryDtos;
            createListener(); 
            NetworkEntitiesVM = new NetworkEntitiesViewModel();
            networkEntitiesViewInstance = new NetworkEntitiesView { DataContext = NetworkEntitiesVM };
            AddEntityVM = new AddEntityViewModel();
            addEntityViewInstance = new AddEntityView{ DataContext =  AddEntityVM};
            NetworkDisplayVM = new NetworkDisplayViewModel();
            networkDisplayViewInstance = new NetworkDisplayView{ DataContext = NetworkDisplayVM };
            HomeCommand = new MyICommand(OnHomeCommand);
            NetEntitiesCommand = new MyICommand(OnNetEntitiesCommand);
            NetDisplayCommand = new MyICommand(OnNetDisplayCommand);
            GraphCommand = new MyICommand(OnGraphCommand);
            ExitCommand = new MyICommand(OnExitCommand);
            AddCommand = new MyICommand(OnAddCommand);
            UndoCommand = new MyICommand(OnUndoCommand);
            UndoAllCommand = new MyICommand(OnUndoAllCommand);
            OnHomeCommand(); 
        }

        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25675);
            tcp.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {

                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        if (incomming.Equals("Need object count"))
                        {
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(MaxCount.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            string[] splits = incomming.Split(':');
                            int value = Convert.ToInt32(splits[1]);
                            int id = Convert.ToInt32(splits[0].Split('_')[1]);
                            id = id == 0 ? maxCount : id;
                            string dt = DateTime.Now.ToString();

                            string path = "C:\\Users\\Dimitrije\\Documents\\GitHub\\IUuIS_projekat2\\Projekat\\NetworkService\\NetworkService\\NetworkService\\public\\files\\Log.txt";

                            string content = $"{id}|{value}|{dt}";

                            FileAccessManager.WriteToFile(path, content);

                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }
    }
}
