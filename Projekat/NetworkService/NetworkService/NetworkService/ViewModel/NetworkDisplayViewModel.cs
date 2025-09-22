using NetworkService.DTOs;
using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Repositories;
using NetworkService.Services.UndoServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => SetProperty(ref isActive, value);
        }

        private bool isMoving = false;
        public bool IsMoving
        {
            get => isMoving;
            set => SetProperty(ref isMoving, value);
        }

        private string connectingString;
        public string ConnectingString
        {
            get => connectingString;
            set => SetProperty(ref connectingString, value);
        }


        private ValveRepository valveRepository;
        public ObservableCollection<Valve> Valves { get; private set; }
        public ObservableCollection<Valve> ValvesInList { get; set; }

        private HistoryRepository historyRepository;
        private Stack<IUndoService> undoStack { get; }

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
        public MyICommand<object> StartDragFromCanvas { get; set; }
        public MyICommand UndoCommand { get; }
        public MyICommand UndoAllCommand { get; }

        private bool isLineSourceSelected = false;
        private int sourceCanvasIndex = -1;
        private int destinationCanvasIndex = -1;
        private NewLine currentLine = new NewLine();
        private Point linePoint1 = new Point();
        private Point linePoint2 = new Point();

        public NetworkDisplayViewModel()
        {
            valveRepository = ValveRepository.Instance;
            Valves = valveRepository.Valves;
            ValvesInList = new ObservableCollection<Valve>();


            historyRepository = HistoryRepository.Instance;
            undoStack = historyRepository.UndoStack;
            historyDtoRepository = HistoryDtoRepository.Instance;
            historyDtos = historyDtoRepository.HistoryDtos;

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

                DescriptionCollection.Add("");
            }

            DragOverCanvas = new MyICommand<object>(OnDragOverCanvas);
            DropValveOnCanvas = new MyICommand<object>(OnDrop);
            LeftMouseButtonDownOnCanvas = new MyICommand<object>(OnLeftMouseButtonDown);
            MouseLeftButtonUp = new MyICommand(OnMouseLeftButtonUp);
            SelectionChanged = new MyICommand<object>(OnSelectionChanged);
            FreeCanvas = new MyICommand<object>(OnFreeCanvas);
            RightMouseButtonDownOnCanvas = new MyICommand<object>(OnRightMouseButtonDown);
            StartDragFromCanvas = new MyICommand<object>(OnStartDragFromCanvas);
            //UndoCommand = new MyICommand(OnUndo);
            //UndoAllCommand = new MyICommand(OnUndoAll);

            this.Observer();

        }

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

        private void OnStartDragFromCanvas(object parameter)
        {
            if (parameter is Border border)
            {
                if (border.Tag is string tagString && int.TryParse(tagString, out int index))
                {
                    if (CanvasCollection[index].Resources.Contains("taken"))
                    {
                        IsMoving = true;
                        draggedValve = (Valve)CanvasCollection[index].Resources["data"];
                        draggingSourceIndex = index;
                        dragging = true;

                        DragDrop.DoDragDrop(border, draggedValve, DragDropEffects.Move);
                    }
                }
            }
        }

        private void OnDragOverCanvas(object parameter)
        {
            if (parameter is DragEventArgs e)
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

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

        private void OnDrop(object parameter)
        {
            if (draggedValve != null)
            {
                int destinationIndex = Convert.ToInt32(parameter);

                if (CanvasCollection[destinationIndex].Resources["taken"] == null)
                {
                    // Perform the drop action
                    if (draggingSourceIndex != -1)
                    {
                        // This is a move from one canvas to another
                        IsMoving = true;
                        this.OnFreeCanvas(draggingSourceIndex);
                    }
                    else
                    {
                        // This is a drop from the list, so remove it from the list
                        ValvesInList.Remove(draggedValve);
                    }

                    // Place the valve on the new canvas. This is a crucial step that changes the state.
                    // You can use the RedoDropOnCanvas helper method here as well, since it's cleaner.
                    RedoDropOnCanvas(draggedValve, destinationIndex, draggingSourceIndex);
                    // Create and push the undo action
                    IUndoService undoAction = new DropOnCanvas(this, draggedValve, destinationIndex, draggingSourceIndex);
                    undoStack.Push(undoAction);
                    HistoryDtos.Insert(0, new HistoryDto(undoAction.getTitle(), undoAction.getDateTime()));



                    // Reset drag state
                    draggedValve = null;
                    draggingSourceIndex = -1;
                    dragging = false;
                    IsMoving = false;
                }
            }
        }
        private void Observer()
        {
            var observeThread = new Thread(
                () =>
                {
                    while (true)
                    {
                        try
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                //if the main valves are updated (added new or deleted)
                                List<Valve> valvesToAdd = Valves.Where(v =>
                                {
                                    // Check if the valve exists in ValvesInList based on its unique ID
                                    bool inList = ValvesInList.Any(vList => vList.Id == v.Id);
                                    // Check if the valve exists on any canvas
                                    bool onCanvas = CanvasCollection.Any(canvas =>
                                    {
                                        var valveOnCanvas = canvas.Resources["data"] as Valve;
                                        return valveOnCanvas != null && valveOnCanvas.Id == v.Id;
                                    });

                                    return !inList && !onCanvas;
                                }).ToList();

                                foreach (Valve valve in valvesToAdd)
                                {
                                    // Add a new, but unique, instance to your display list
                                    ValvesInList.Add(new Valve(valve));
                                }

                                //which valve needs to be removed from canvas (its deleted)
                                List<Valve> valvesToRemoveFromCanvas = new List<Valve>();
                                for (int i = 0; i < 12; ++i)
                                {
                                    Valve canvasValve = CanvasCollection[i].Resources["data"] as Valve;
                                    if (canvasValve != null && !Valves.Any(v => v.Id == canvasValve.Id))
                                    {
                                        valvesToRemoveFromCanvas.Add(canvasValve);
                                        this.OnFreeCanvas(i);
                                    }
                                }

                                //which valve needs to be removed from the list (its deleted)
                                List<Valve> valvesToRemoveFromList = ValvesInList.Where(vList =>
                                {
                                    Valve valveInMainList = Valves.FirstOrDefault(v => v.Id == vList.Id);
                                    return valveInMainList == null;
                                }).ToList();

                                foreach (Valve valve in valvesToRemoveFromList)
                                {
                                    this.DeleteValveFromList(valve);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                        }

                        Thread.Sleep(1000);
                    }
                });
            observeThread.IsBackground = true;
            observeThread.Start();
        }

        public void DeleteValveFromList(Valve valve)
        {
            try
            {
                var valveToRemove = ValvesInList.FirstOrDefault(v => v.Id == valve.Id);
                if (valveToRemove != null)
                {
                    ValvesInList.Remove(valveToRemove);
                }
                BuildGroupedValves();
            }
            catch (Exception ex) { }
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

        public void OnFreeCanvas(object parameter)
        {
            int index = Convert.ToInt32(parameter);

            if (CanvasCollection[index].Resources["taken"] == null)
            {
                return;
            }

            Valve valveToDelete = CanvasCollection[index].Resources["data"] as Valve;

            // Find all lines connected to this canvas
            List<NewLine> linesToDelete = LineCollection.Where(line =>
                line.Source == index || line.Destination == index).ToList();

            if (!IsMoving)
            {
                // This is the user's action, so we create an undo object.
                IUndoService undoAction = new FreeCanvas(this, valveToDelete, index, linesToDelete);
                undoStack.Push(undoAction);
                HistoryDtos.Insert(0, new HistoryDto(undoAction.getTitle(), undoAction.getDateTime()));
            }

            // Now, call the private method to perform the actual state changes.
            // This is the key separation to prevent recursion.
            PerformFreeCanvas(index, valveToDelete);

            // Reset line drawing state if necessary
            if (sourceCanvasIndex != -1)
            {
                isLineSourceSelected = false;
                sourceCanvasIndex = -1;
                linePoint1 = new Point();
                linePoint2 = new Point();
                currentLine = new NewLine();
            }
        }



        public void PerformFreeCanvas(int index, Valve valve)
        {
            // The view model is handling the IsMoving logic
            // Add the valve back to the list of available valves
            if (!IsMoving)
            {
                ValvesInList.Add(valve);
                DeleteLinesForCanvas(index);
            }

            // Clear the canvas's visual properties
            CanvasCollection[index].Children.Clear();
            CanvasCollection[index].Background = Brushes.Transparent;
            CanvasCollection[index].Resources.Remove("taken");
            CanvasCollection[index].Resources.Remove("data");
            BorderBrushCollection[index] = Brushes.Black;
            DescriptionCollection[index] = ($" ");
        }


        public void RedoDropOnCanvas(Valve valve, int index, int srcIdx)
        {
            string path = $@"C:\Users\Dimitrije\Documents\GitHub\IUuIS_projekat2\Projekat\NetworkService\NetworkService\NetworkService\public\pics\{valve.Type.ToString().ToLower()}{valve.Id % 3 + 1}.jpg";

            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri(path, UriKind.Absolute);
            logo.CacheOption = BitmapCacheOption.OnLoad;
            logo.EndInit();
            logo.Freeze();

            Image img = new Image
            {
                Source = logo,
                Stretch = Stretch.UniformToFill,
                Width = 95,
                Height = 95
            };

            CanvasCollection[index].Children.Clear();
            CanvasCollection[index].Children.Add(img);

            CanvasCollection[index].Resources["taken"] = true;
            CanvasCollection[index].Resources["data"] = valve;
            BorderBrushCollection[index] = valve.Validation == ValueValidation.Normal ? Brushes.Black : Brushes.Red;
            DescriptionCollection[index] = $"ID: {valve.Id} Value: {valve.MeasuredValue}";

            // Handle line updates if the valve was moved from another canvas
            if (srcIdx != -1)
            {
                UpdateLinesForCanvas(srcIdx, index);
            }
        }

        public Valve SelectedValve
        {
            get => selectedValve;
            set => SetProperty(ref selectedValve, value);
        }

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

        private void OnSelectionChanged(object parameter)
        {
            if (!dragging)
            {
                dragging = true;
                draggedValve = SelectedValve;
                DragDrop.DoDragDrop((ListView)parameter, draggedValve, DragDropEffects.Move);
            }
        }

        private void OnRightMouseButtonDown(object parameter)
        {
            int index = Convert.ToInt32(parameter);

            if (CanvasCollection[index].Resources["taken"] != null)
            {
                if (!isLineSourceSelected)
                {
                    ConnectingString = $"Connecting {index + 1} to ...";
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
                    ConnectingString = string.Empty;
                    if ((sourceCanvasIndex != destinationCanvasIndex) && !DoesLineAlreadyExist(sourceCanvasIndex, destinationCanvasIndex))
                    {
                        linePoint2 = GetPointForCanvasIndex(destinationCanvasIndex);

                        currentLine.X2 = linePoint2.X;
                        currentLine.Y2 = linePoint2.Y;
                        currentLine.Destination = destinationCanvasIndex;

                        NewLine line = new NewLine
                        {
                            X1 = currentLine.X1,
                            Y1 = currentLine.Y1,
                            X2 = currentLine.X2,
                            Y2 = currentLine.Y2,
                            Source = currentLine.Source,
                            Destination = currentLine.Destination
                        };

                        LineCollection.Add(line);
                        IUndoService connect = new ConnectValves(this, line, sourceCanvasIndex, destinationCanvasIndex);
                        undoStack.Push(connect);
                        HistoryDtos.Insert(0, new HistoryDto(connect.getTitle(), connect.getDateTime()));

                        isLineSourceSelected = false;
                        linePoint1 = new Point();
                        linePoint2 = new Point();
                        currentLine = new NewLine();
                    }
                    else
                    {
                        //beggining and end of the line is on the same valve
                        isLineSourceSelected = false;
                        ConnectingString = string.Empty;
                        linePoint1 = new Point();
                        linePoint2 = new Point();
                        currentLine = new NewLine();
                    }
                }
            }
            else
            {
                //canvas is not already taken
                isLineSourceSelected = false;
                ConnectingString = string.Empty;
                linePoint1 = new Point();
                linePoint2 = new Point();
                currentLine = new NewLine();
            }

        }

        public void UpdateLinesForCanvas(int sourceCanvas, int destinationCanvas)
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
    }
}