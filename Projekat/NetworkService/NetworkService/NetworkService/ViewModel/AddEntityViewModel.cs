using NetworkService.DTOs;
using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Repositories;
using NetworkService.Services;
using NetworkService.Services.UndoServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Navigation;

namespace NetworkService.ViewModel
{
    public class AddEntityViewModel : BindableBase
    {
        private ValveRepository valveRepository;
        public ObservableCollection<Valve> Valves;

        private HistoryRepository historyRepository;
        private Stack<IUndoService> undoStack;

        private HistoryDtoRepository historyDtoRepository;
        private ObservableCollection<HistoryDto> historyDtos;
        public ObservableCollection<HistoryDto> HistoryDtos
        {
            get => historyDtos;
        }

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => SetProperty(ref isActive, value);
        }

        private bool onClose = false;
        public bool OnClose
        {
            get => onClose;
            set => SetProperty(ref onClose, value);
        }


        private bool isFormDirty;
        public bool IsFormDirty
        {
            get => isFormDirty;
            set {
                if(Name == string.Empty)
                SetProperty(ref isFormDirty, value);
            
            } 
        }

        private int entityId;
        public int EntityId { get => entityId;}

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private int selectedTypeIndex = 0;
        public int SelectedTypeIndex
        {
            get => selectedTypeIndex;
            set
            {
                SetProperty(ref selectedTypeIndex, value);
            }
        }

        private int lastValue = -1;
        public int LastValue
        {
            get => lastValue;
            set => SetProperty(ref lastValue, value);
        }

        private string createdAt;
        public string CreatedAt { get => createdAt; }
        public MyICommand SaveCommand { get; }
        public MyICommand ClearCommand { get; }
        public MyICommand DiscardCommand { get; }

        public AddEntityViewModel() {

            valveRepository = ValveRepository.Instance;
            Valves = valveRepository.Valves;
            entityId = valveRepository.GreatestId + 1; //the greatest in the sequence is n, so the next is n+1
            historyRepository = HistoryRepository.Instance;
            undoStack = historyRepository.UndoStack;
            historyDtoRepository = HistoryDtoRepository.Instance;
            historyDtos = historyDtoRepository.HistoryDtos;
            createdAt = DateTime.Now.ToString();
            SaveCommand = new MyICommand(OnSave);
            ClearCommand = new MyICommand(OnClear);
            DiscardCommand = new MyICommand(OnDiscard);

        }

        private void OnSave()
        {
            if (!IsActive) return;
            try
            {
                bool retVal = true;

                if (!Regex.IsMatch(Name, @"^[A-Za-z0-9_]+$")) //alphanumerical + _
                {
                    NotificationService.Instance.ShowError("Name must only contain letters, numbers, and underscore ('_')", "ERROR Name invalid");
                    retVal = false;
                }

                if(Valves.Any(v => v.Name.ToLower() == Name.ToLower()))
                {
                    NotificationService.Instance.ShowError("An entity with that name already exists.", "ERROR Name exists");
                    retVal = false;
                }

                if (SelectedTypeIndex == 0) 
                {     
                    NotificationService.Instance.ShowError("Please select an entity type.", "ERROR No type");
                    retVal = false;
                }

                if(LastValue < 0)
                {
                    NotificationService.Instance.ShowError("Value cannot be a negative number", "ERROR Negative value");
                    retVal = false;
                }

                if (retVal)
                {
                    ValveType type = SelectedTypeIndex == 1 ? ValveType.CableSensor : ValveType.DigitalManometer;

                    Valve newValve = new Valve(EntityId, Name, type, LastValue, CreatedAt);
                    Valves.Add(newValve);
                    IUndoService addValve = new AddValve(this, newValve);
                    undoStack.Push(addValve);
                    HistoryDtos.Insert(0, new HistoryDto(addValve.getTitle(), addValve.getDateTime()));
                    this.Name = string.Empty;
                    this.SelectedTypeIndex = 0;
                    this.LastValue = -1;
                    this.OnClose = true;    
                }
            }
            catch (Exception ex) { 
                        NotificationService.Instance.ShowError($"Entity '{Name}' cannot be added to the table", "ERROR Adding entity");
            }
        }

        private void OnClear()
        {
            if (!IsActive) return;
            try
            {
                if (MessageBox.Show("Do you really wanna clear all inputs?\n" +
                    "Once done - it's irreversible.", "Clear all inputs?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.Name = string.Empty;
                    this.SelectedTypeIndex = 0;
                    this.LastValue = -1;
                    NotificationService.Instance.ShowSuccess("All inputs cleared!", "SUCCESS Cleared inputs");
                }
            }catch(Exception ex)
            {
                    NotificationService.Instance.ShowError("Cannot clear inputs. Try again later.", "ERROR Clear inputs");
            }
        }

        private void OnDiscard()
        {
            if (!IsActive) return;
            try
            {
                if (MessageBox.Show("All inputs are going to be deleted.\n" +
                    "Once done - it's irreversible.", "Discard changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    this.Name = string.Empty;
                    this.SelectedTypeIndex = 0;
                    this.LastValue = -1;
                    this.OnClose = true;
                    NotificationService.Instance.ShowSuccess("Entity discarded", "SUCCESS Discard entity");
                }
            }
            catch (Exception ex) { 
                    NotificationService.Instance.ShowError("Entity cannot be discarded. Try again later.", "ERROR Discard entity");
            }

        }


    }
}
