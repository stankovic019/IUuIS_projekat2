using NetworkService.DTOs;
using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
        #region Initialize
        public ObservableCollection<Valve> ValvesInList { get; set; }
        public ObservableCollection<ValveGroupDto> GroupedValves { get; set; }
        public ObservableCollection<Brush> BorderBrushCollection { get; set; }
        public ObservableCollection<Canvas> CanvasCollection { get; set; }
        public ObservableCollection<NewLine> LineCollection { get; set; }
        public ObservableCollection<string> DescriptionCollection { get; set; }

        private Valve selectedValve;

        private Valve draggedValve = null;
        private bool dragging = false;
        public int draggingSourceIndex = -1;

        public MyICommand<object> DragOverCanvas { get; set; }
        public MyICommand<object> DropValveOnCanvas { get; set; }
        public MyICommand<object> LeftMouseButtonDownOnCanvas { get; set; }
        public MyICommand MouseLeftButtonUp { get; set; }
        public MyICommand<object> SelectionChanged { get; set; }
        public MyICommand<object> FreeCanvas { get; set; }
        public MyICommand<object> RightMouseButtonDownOnCanvas { get; set; }
        //public MyICommand OrganizeAllCommand { get; set; }

        private bool isLineSourceSelected = false;
        private int sourceCanvasIndex = -1;
        private int destinationCanvasIndex = -1;
        private NewLine currentLine = new NewLine();
        private Point linePoint1 = new Point();
        private Point linePoint2 = new Point();

        public NetworkDisplayViewModel()
        {
            ValvesInList = new ObservableCollection<Valve>
            {
            ValveRepository.Instance.Valves.ElementAt(0),
            ValveRepository.Instance.Valves.ElementAt(2)
            };
            ValvesInList.CollectionChanged += (s, e) => BuildGroupedValves();
            BuildGroupedValves();

            LineCollection = new ObservableCollection<NewLine>();
            CanvasCollection = new ObservableCollection<Canvas>();
            BorderBrushCollection = new ObservableCollection<Brush>();
            DescriptionCollection = new ObservableCollection<string>();
            BuildGroupedValves();
            for (int i = 0; i < 12; i++)
            {
                BorderBrushCollection.Add(Brushes.Black);
                CanvasCollection.Add(new Canvas
                {
                    Width = 100,
                    Height = 100,
                    AllowDrop = true,
                    Background = Brushes.Transparent
                });

                DescriptionCollection.Add(" ");
            }

            DragOverCanvas = new MyICommand<object>(OnDragOverCanvas);
            DropValveOnCanvas = new MyICommand<object>(OnDrop);
            LeftMouseButtonDownOnCanvas = new MyICommand<object>(OnLeftMouseButtonDown);
            MouseLeftButtonUp = new MyICommand(OnMouseLeftButtonUp);
            SelectionChanged = new MyICommand<object>(OnSelectionChanged);
            FreeCanvas = new MyICommand<object>(OnFreeCanvas);
            RightMouseButtonDownOnCanvas = new MyICommand<object>(OnRightMouseButtonDown);

        }
        #endregion

        private void BuildGroupedValves()
        {
            var groups = ValvesInList
                .GroupBy(v => v.Type)
                .Select(g => new ValveGroupDto(
                    g.Key,
                    g.Key.ToString(),             
                    g.OrderBy(v => v.Name).ToList() 
                ));
            GroupedValves = new ObservableCollection<ValveGroupDto>(groups);
        }




        private void OnDragOverCanvas(object parameter)
        {
            if (parameter is DragEventArgs e)
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }



        #region Helper/GetCanvasIndexForEntityId
        public int GetCanvasIndexForValveId(int id)
        {
            for (int i = 0; i < CanvasCollection.Count; i++)
            {
                Valve valve = (CanvasCollection[i].Resources["data"]) as Valve;

                if ((valve != null) && (valve.Id == id))
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion

        #region OnDrop
        private void OnDrop(object parameter)
        {
            MessageBox.Show("eo");

            if (draggedValve != null)
            {
                int index = Convert.ToInt32(parameter);

                string path = $@"C:\Users\Dimitrije\Documents\GitHub\IUuIS_projekat2\Projekat\NetworkService\NetworkService\NetworkService\public\pics\{draggedValve.Type.ToString().ToLower()}.jpg";
                MessageBox.Show(path);


                if (CanvasCollection[index].Resources["taken"] == null)
                {
                    BitmapImage logo = new BitmapImage();
                    logo.BeginInit();
                    logo.UriSource = new Uri(path, UriKind.Absolute);
                    logo.CacheOption = BitmapCacheOption.OnLoad;
                    logo.EndInit();
                    logo.Freeze(); // optional

                    // Create an Image element instead of setting Background
                    Image img = new Image
                    {
                        Source = logo,
                        Stretch = Stretch.UniformToFill,
                        Width = 100,
                        Height = 100
                    };

                    // Clear any old children (if the canvas already had something)
                    CanvasCollection[index].Children.Clear();
                    CanvasCollection[index].Children.Add(img);

                    // Mark canvas as taken
                    CanvasCollection[index].Resources["taken"] = true;
                    CanvasCollection[index].Resources["data"] = draggedValve;
                    BorderBrushCollection[index] = draggedValve.Validation == ValueValidation.Normal ? Brushes.Black : Brushes.Red;
                    DescriptionCollection[index] = ($"ID: {draggedValve.Id} Value: {draggedValve.MeasuredValue}");

                    // PREVLACENJE IZ DRUGOG CANVASA
                    if (draggingSourceIndex != -1)
                    {
                        CanvasCollection[draggingSourceIndex].Background = Brushes.Yellow;

                        CanvasCollection[draggingSourceIndex].Resources.Remove("taken");
                        CanvasCollection[draggingSourceIndex].Resources.Remove("data");
                        BorderBrushCollection[draggingSourceIndex] = Brushes.Black;
                        DescriptionCollection[draggingSourceIndex] = (" ");


                        UpdateLinesForCanvas(draggingSourceIndex, index);

                        // Crtanje linije se prekida ako je, izmedju postavljanja tacaka, entitet pomeren na drugo polje
                        if (sourceCanvasIndex != -1)
                        {
                            isLineSourceSelected = false;
                            sourceCanvasIndex = -1;
                            linePoint1 = new Point();
                            linePoint2 = new Point();
                            currentLine = new NewLine();
                        }

                        draggingSourceIndex = -1;
                    }

                    //PREVLACENJE IZ LISTE
                    if (ValvesInList.Contains(draggedValve))
                    {
                        ValvesInList.Remove(draggedValve);
                    }
                }
            }
        }

        #endregion

        #region UpdateValveOnCanvas
        public void UpdateEntityOnCanvas(Valve valve)
        {
            int canvasIndex = GetCanvasIndexForValveId(valve.Id);

            if (canvasIndex != -1)
            {
                DescriptionCollection[canvasIndex] = ($"ID: {valve.Id} Value: {valve.MeasuredValue}");
                if (valve.Validation == ValueValidation.Normal)
                {
                    BorderBrushCollection[canvasIndex] = Brushes.Black;
                }
                else
                {
                    BorderBrushCollection[canvasIndex] = Brushes.Red;
                }
            }
        }
        #endregion

        #region Delete
        //MainWindowViewModel
        public void DeleteValveFromCanvas(Valve valve)
        {
            int canvasIndex = GetCanvasIndexForValveId(valve.Id);

            if (canvasIndex != -1)
            {
                CanvasCollection[canvasIndex].Background = Brushes.Yellow;
                CanvasCollection[canvasIndex].Resources.Remove("taken");
                CanvasCollection[canvasIndex].Resources.Remove("data");
                BorderBrushCollection[canvasIndex] = Brushes.Black;
                DescriptionCollection[canvasIndex] = ($" ");

                DeleteLinesForCanvas(canvasIndex);
            }
        }
        private void DeleteLinesForCanvas(int canvasIndex)
        {
            List<NewLine> linesToDelete = new List<NewLine>();

            for (int i = 0; i < LineCollection.Count; i++)
            {
                if ((LineCollection[i].Source == canvasIndex) || (LineCollection[i].Destination == canvasIndex))
                {
                    linesToDelete.Add(LineCollection[i]);
                }
            }

            foreach (NewLine line in linesToDelete)
            {
                LineCollection.Remove(line);
            }
        }
        private void OnFreeCanvas(object parameter)
        {
            int index = Convert.ToInt32(parameter);

            if (CanvasCollection[index].Resources["taken"] != null)
            {
                // Crtanje linije se prekida ako je, izmedju postavljanja tacaka, entitet uklonjen sa canvas-a
                if (sourceCanvasIndex != -1)
                {
                    isLineSourceSelected = false;
                    sourceCanvasIndex = -1;
                    linePoint1 = new Point();
                    linePoint2 = new Point();
                    currentLine = new NewLine();
                }

                DeleteLinesForCanvas(index);

                ValvesInList.Add((Valve)CanvasCollection[index].Resources["data"]);
                CanvasCollection[index].Children.Clear();
                CanvasCollection[index].Background = Brushes.Transparent;
                CanvasCollection[index].Resources.Remove("taken");
                CanvasCollection[index].Resources.Remove("data");
                BorderBrushCollection[index] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1F22"));
                DescriptionCollection[index] = ($" ");
            }
        }
        public Valve SelectedValve
        {
            get => selectedValve;
            set => SetProperty(ref selectedValve, value);
        }
        #endregion

        #region LeftMouseBTN
        private void OnLeftMouseButtonDown(object parameter)
        {
            if (!dragging && parameter is UIElement uiElement)
            {
                dragging = true;

                if (uiElement is FrameworkElement fe && fe.Tag != null && int.TryParse(fe.Tag.ToString(), out int index))
                {
                    draggingSourceIndex = index;

                    if (CanvasCollection[index].Resources["taken"] != null)
                    {
                        draggedValve = (Valve)CanvasCollection[index].Resources["data"];
                        DragDrop.DoDragDrop(uiElement, draggedValve, DragDropEffects.Move);
                    }
                }
            }
        }


        private void OnMouseLeftButtonUp()
        {
            draggedValve = null;
            SelectedValve = null;
            dragging = false;
            draggingSourceIndex = -1;
        }
        #endregion

        #region SelectionChanged
        private void OnSelectionChanged(object parameter)
        {
            if (!dragging)
            {
                dragging = true;
                draggedValve = SelectedValve;
                DragDrop.DoDragDrop((ListView)parameter, draggedValve, DragDropEffects.Move);
            }
        }
        #endregion

        #region RightMouseButton
        private void OnRightMouseButtonDown(object parameter)
        {
            int index = Convert.ToInt32(parameter);

            if (CanvasCollection[index].Resources["taken"] != null)
            {
                if (!isLineSourceSelected)
                {
                    sourceCanvasIndex = index;

                    linePoint1 = GetPointForCanvasIndex(sourceCanvasIndex);

                    currentLine.X1 = linePoint1.X;
                    currentLine.Y1 = linePoint1.Y;
                    currentLine.Source = sourceCanvasIndex;

                    isLineSourceSelected = true;
                }
                else
                {
                    destinationCanvasIndex = index;

                    if ((sourceCanvasIndex != destinationCanvasIndex) && !DoesLineAlreadyExist(sourceCanvasIndex, destinationCanvasIndex))
                    {
                        linePoint2 = GetPointForCanvasIndex(destinationCanvasIndex);

                        currentLine.X2 = linePoint2.X;
                        currentLine.Y2 = linePoint2.Y;
                        currentLine.Destination = destinationCanvasIndex;

                        LineCollection.Add(new NewLine
                        {
                            X1 = currentLine.X1,
                            Y1 = currentLine.Y1,
                            X2 = currentLine.X2,
                            Y2 = currentLine.Y2,
                            Source = currentLine.Source,
                            Destination = currentLine.Destination
                        });

                        isLineSourceSelected = false;

                        linePoint1 = new Point();
                        linePoint2 = new Point();
                        currentLine = new NewLine();
                    }
                    else
                    {
                        // Pocetak i kraj linije su u istom canvasu

                        isLineSourceSelected = false;

                        linePoint1 = new Point();
                        linePoint2 = new Point();
                        currentLine = new NewLine();
                    }
                }
            }
            else
            {
                // Canvas na koji se postavlja tacka nije zauzet

                isLineSourceSelected = false;

                linePoint1 = new Point();
                linePoint2 = new Point();
                currentLine = new NewLine();
            }
        }
        #endregion

        #region Line
        private void UpdateLinesForCanvas(int sourceCanvas, int destinationCanvas)
        {
            for (int i = 0; i < LineCollection.Count; i++)
            {
                if (LineCollection[i].Source == sourceCanvas)
                {
                    Point newSourcePoint = GetPointForCanvasIndex(destinationCanvas);
                    LineCollection[i].X1 = newSourcePoint.X;
                    LineCollection[i].Y1 = newSourcePoint.Y;
                    LineCollection[i].Source = destinationCanvas;
                }
                else if (LineCollection[i].Destination == sourceCanvas)
                {
                    Point newDestinationPoint = GetPointForCanvasIndex(destinationCanvas);
                    LineCollection[i].X2 = newDestinationPoint.X;
                    LineCollection[i].Y2 = newDestinationPoint.Y;
                    LineCollection[i].Destination = destinationCanvas;
                }
            }
        }

        private bool DoesLineAlreadyExist(int source, int destination)
        {
            foreach (NewLine line in LineCollection)
            {
                if ((line.Source == source) && (line.Destination == destination))
                {
                    return true;
                }
                if ((line.Source == destination) && (line.Destination == source))
                {
                    return true;
                }
            }
            return false;
        }
        // Centralna tacka na Canvas kontroli
        private Point GetPointForCanvasIndex(int canvasIndex)
        {
            double x = 0, y = 0;

            for (int row = 0; row <= 3; row++)
            {
                for (int col = 0; col <= 2; col++)
                {
                    int currentIndex = row * 3 + col;

                    if (canvasIndex == currentIndex)
                    {
                        x = 50 + (col * 135);
                        y = 50 + (row * 135);

                        break;
                    }
                }
            }
            return new Point(x, y);
        }
        #endregion

    }
}