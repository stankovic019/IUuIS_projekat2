using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Repositories;
using NetworkService.Views;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        ValveRepository valveRepository;

        private ObservableCollection<Valve> valves;

        private int count = 0;

        public NetworkEntitiesViewModel NetworkEntitiesVM { get; }

        private NetworkEntitiesView networkEntitiesViewInstance;
        
        private string title = "Simulator infrastukturnih sistema";

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private string headerIcon = "/public/icons/home-icon.png";
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

        // Dugmići (visibility se rešava preko bool i Converters-a)
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
        public bool NotificationBtnVisible { get; set; }

        // Komande
        public MyICommand ShowNotificationCommand { get; }
        public MyICommand HomeCommand { get; }
        public MyICommand NetEntitiesCommand { get; }
        public MyICommand NetDisplayCommand { get; }
        public MyICommand GraphCommand { get; }
        public MyICommand ExitCommand { get; }

        private void SetButtons(int g, int b1, int b2, int b3, int b4, int b5)
        {
            ToolGridVisible = g == 1;
            AddBtnVisible = b1 == 1;
            DeleteBtnVisible = b2 == 1;
            UndoBtnVisible = b3 == 1;
            UndoAllBtnVisible = b4 == 1;
            NotificationBtnVisible = b5 == 1;

            // osvežavanje property-ja
            OnPropertyChanged(nameof(AddBtnVisible));
            OnPropertyChanged(nameof(DeleteBtnVisible));
            OnPropertyChanged(nameof(UndoBtnVisible));
            OnPropertyChanged(nameof(UndoAllBtnVisible));
            OnPropertyChanged(nameof(NotificationBtnVisible));
        }

        private void OnHomeCommand()
        {
            Title = "Simulator infrastukturnih sistema";
            HeaderIcon = "/public/icons/home-icon.png";
            SetButtons(1, 0, 0, 0, 1, 1);
            CurrentView = new HomePageView();
            NetworkEntitiesVM.IsActive = false;
        }

        private void OnNetEntitiesCommand()
        {
            Title = "Network Entities";
            HeaderIcon = "/public/icons/clock-icon.png";
            SetButtons(1, 1, 1, 1, 1, 0);
            CurrentView = networkEntitiesViewInstance;
            NetworkEntitiesVM.IsActive = true;
        }

        private void OnNetDisplayCommand()
        {
            Title = "Network Display";
            HeaderIcon = "/public/icons/network-display-icon.png";
            SetButtons(1, 0, 0, 1, 1, 0);
            CurrentView = new NetworkDisplayPage();
            NetworkEntitiesVM.IsActive = false;
        }

        private void OnGraphCommand()
        {
            Title = "Graph";
            HeaderIcon = "/public/icons/graph-icon.png";
            SetButtons(0, 0, 0, 0, 0, 0);
            CurrentView = null;
            NetworkEntitiesVM.IsActive = false;
        }

        private void OnExitCommand()
        {
            if (MessageBox.Show("Exiting will apply all changes. Exit app?", "Exit",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {   
                //ValveRepository.Instance.StoreValves();
                Application.Current.Shutdown();
            }
        }

        public MainWindowViewModel()
        {
            notificationManager = new NotificationManager();
            valveRepository = ValveRepository.Instance;
            valves = valveRepository.Valves;
            count = valves.Count;
            createListener(); //Povezivanje sa serverskom aplikacijom
            NetworkEntitiesVM = new NetworkEntitiesViewModel();
            networkEntitiesViewInstance = new NetworkEntitiesView { DataContext = NetworkEntitiesVM };
            HomeCommand = new MyICommand(OnHomeCommand);
            NetEntitiesCommand = new MyICommand(OnNetEntitiesCommand);
            NetDisplayCommand = new MyICommand(OnNetDisplayCommand);
            GraphCommand = new MyICommand(OnGraphCommand);
            ExitCommand = new MyICommand(OnExitCommand);
            OnHomeCommand(); // inicijalno stanje
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
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            /* Umesto sto se ovde salje count.ToString(), potrebno je poslati 
                             * duzinu liste koja sadrzi sve objekte pod monitoringom, odnosno
                             * njihov ukupan broj (NE BROJATI OD NULE, VEC POSLATI UKUPAN BROJ)
                             * */
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            string[] splits = incomming.Split(':');
                            int value = Convert.ToInt32(splits[1]);
                            int id = Convert.ToInt32(splits[0].Split('_')[1]);
                            id = id == 0 ? count : id;
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
