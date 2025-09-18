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
    /// Interaction logic for NetworkDisplayPage.xaml
    /// </summary>
    public partial class NetworkDisplayPage : UserControl
    {

        public NetworkDisplayPage()
        {
            InitializeComponent();
        }

        private void displayTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (displayTreeView.SelectedItem is TreeViewItem item)
                {
                    DragDrop.DoDragDrop(displayTreeView, item.Header.ToString(), DragDropEffects.Move);
                    
                }
            }
        }

        private void canvas00_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void canvas00_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string text = (string)e.Data.GetData(DataFormats.StringFormat);
                canvas00.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));

            }
        }

        private void canvas01_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas01_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas02_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas02_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas10_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas10_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas11_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas11_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas12_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas12_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas20_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas20_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas21_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas21_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas22_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas22_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas30_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas30_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas31_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas31_PreviewDragOver(object sender, DragEventArgs e)
        {

        }

        private void canvas32_Drop(object sender, DragEventArgs e)
        {

        }

        private void canvas32_PreviewDragOver(object sender, DragEventArgs e)
        {

        }
    }
}
