using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Services.UndoServices
{
    public class AddValve : IUndoService
    {

        private ObservableCollection<Valve> valves; 
        private Valve valve;
        private string actionTitle;
        private string dateTime;

        public AddValve(ObservableCollection<Valve> valves, Valve valve)
        {
            this.valves = valves;
            this.valve = valve;
            actionTitle = $"Add Valve {valve.Id}";
            dateTime = DateTime.Now.ToString();
        }

        public bool Action()
        {
            valves.Add(valve);
            if (valves.Contains(valve))
            {
                NotificationService.Instance.ShowSuccess($"Valve {valve.Id} added", "SUCCESS Add");
                return true;
            }
            else
                NotificationService.Instance.ShowError($"Valve {valve.Id} is not added", "ERROR Add");

            return false;
        }
        public bool Undo()
        {
            valves.Remove(valve); 
            if (!valves.Contains(valve))
            {
                NotificationService.Instance.ShowSuccess($"Undo {actionTitle} success", "SUCCESS Undo Add");
                return true;
            }
            else
                NotificationService.Instance.ShowError($"Cannot Undo {actionTitle}", "ERROR Undo Add");
            return false;
        }

        public string getDateTime()
        {
            return dateTime;
        }

        public string getTitle()
        {
            return actionTitle;
        }

    }
}
