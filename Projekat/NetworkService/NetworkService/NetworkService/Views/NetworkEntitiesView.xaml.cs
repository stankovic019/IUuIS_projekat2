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

namespace NetworkService.Views
{
    /// <summary>
    /// Interaction logic for NetworkEntitiesView.xaml
    /// </summary>
    public partial class NetworkEntitiesView : UserControl
    {

        public static readonly RoutedUICommand SearchCommand = new RoutedUICommand("Search", "SearchCommand", typeof(NetworkEntitiesView));

        private void InitializeRoutes()
        {
            CommandBindings.Add(new CommandBinding(SearchCommand, FocusSearchExecuted));
        }

        public NetworkEntitiesView()
        {
            InitializeComponent();
            this.InitializeRoutes();
        }

        private void FocusSearchExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            searchEntitiesTBox.Focus();
            Keyboard.Focus(searchEntitiesTBox); // Dodaj i ovo za sigurnost
            searchEntitiesTBox.SelectAll();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }
    }
}
