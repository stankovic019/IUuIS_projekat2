using NetworkService.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetworkService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string naslov = "Simulator infrastukturnih sistema";

        public static readonly RoutedUICommand HomeCommand = new RoutedUICommand("Home", "HomeCommand", typeof(MainWindow));
        public static readonly RoutedUICommand NetEntitiesCommand = new RoutedUICommand("Network Entities", "NetEntitiesCommand", typeof(MainWindow));
        public static readonly RoutedUICommand NetDisplayCommand = new RoutedUICommand("Network Display", "NetDisplayCommand", typeof(MainWindow));
        public static readonly RoutedUICommand GraphCommand = new RoutedUICommand("Graph", "GraphCommand", typeof(MainWindow));
        public static readonly RoutedUICommand ExitCommand = new RoutedUICommand("Exit", "ExitCommand", typeof(MainWindow));

        private void setButtonAndGridVisibillity(int g, int b1, int b2, int b3, int b4, int b5, int b6)
        {
            ToolBoard_Grid.Visibility = g == 1 ? Visibility.Visible : Visibility.Collapsed;
            addBtn.Visibility = b1 == 1 ? Visibility.Visible : Visibility.Collapsed;
            deleteBtn.Visibility = b2 == 1 ? Visibility.Visible : Visibility.Collapsed;
            undoBtn.Visibility = b3 == 1 ? Visibility.Visible : Visibility.Collapsed;
            undoAllBtn.Visibility = b4 == 1 ? Visibility.Visible : Visibility.Collapsed;
            saveBtn.Visibility = b5 == 1 ? Visibility.Visible : Visibility.Collapsed;
            notificationBtn.Visibility = b6 == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void InitializeRoutes()
        {
            CommandBindings.Add(new CommandBinding(HomeCommand, (sender, e) =>
            {
                homeBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }));

            CommandBindings.Add(new CommandBinding(NetEntitiesCommand, (sender, e) =>
            {
                netEntBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }));

            CommandBindings.Add(new CommandBinding(NetDisplayCommand, (sender, e) =>
            {
                netDispBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }));

            CommandBindings.Add(new CommandBinding(GraphCommand, (sender, e) =>
            {
                graphBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }));

            CommandBindings.Add(new CommandBinding(ExitCommand, (sender, e) =>
            {
                    exitBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }));

        }


        public MainWindow()
        {
            InitializeComponent();
            naslov_textBlock.Text = naslov;
            this.InitializeRoutes();
            this.setButtonAndGridVisibillity(1, 0, 0, 0, 1, 0, 1);
            var uc = new HomePageView();
            MainFrame.Content = uc;
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            naslov_textBlock.Text = naslov;
            headerIcon.Source = new BitmapImage(new Uri("/public/icons/home-icon.png", UriKind.Relative));
            this.setButtonAndGridVisibillity(1, 0, 0, 0, 1, 0, 1);
            var uc = new HomePageView();
            MainFrame.Content = uc;

        }

        private void netEntButton_Click(object sender, RoutedEventArgs e)
        {
            naslov_textBlock.Text = "Network Entities";
            headerIcon.Source = new BitmapImage(new Uri("/public/icons/clock-icon.png", UriKind.Relative));
            this.setButtonAndGridVisibillity(1, 1, 1, 1, 1, 1, 0);
            var uc = new NetworkEntitiesView();
            MainFrame.Content = uc;
            MainFrame.Focus();
        }

        private void netDispButton_Click(object sender, RoutedEventArgs e)
        {
            naslov_textBlock.Text = "Network Display";
            headerIcon.Source = new BitmapImage(new Uri("/public/icons/network-display-icon.png", UriKind.Relative));
            this.setButtonAndGridVisibillity(1,0, 0, 1, 1, 0, 0);
            var uc = new NetworkDisplayPage();
            MainFrame.Content = uc;
        }

        private void graphButton_Click(object sender, RoutedEventArgs e)
        {
            naslov_textBlock.Text = "Graph";
            headerIcon.Source = new BitmapImage(new Uri("/public/icons/graph-icon.png", UriKind.Relative));
            this.setButtonAndGridVisibillity(0, 0, 0, 0, 0, 0, 0);
            MainFrame.Content = null;
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Exiting the application will apply all changes and you won't be able to undo them?\n\n" +
                    "Exit the app?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    this.Close();
        }


        private void NetEntitiesExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            
        }
    }
}
