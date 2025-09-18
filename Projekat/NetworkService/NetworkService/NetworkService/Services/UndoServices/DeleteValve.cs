using NetworkService.Model;
using NetworkService.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Services.UndoServices
{
    public class DeleteValve : IUndoService
    {
        private ObservableCollection<Valve> valves; //saving the refference to thevalves
        private Valve valve; //deleted valve
        private string actionTitle;
        private string dateTime;

        public DeleteValve(ObservableCollection<Valve> valves, Valve valve)
        {
            this.valves = valves;
            this.valve = valve;
            actionTitle = $"Delete Valve {valve.Id}";
            dateTime = DateTime.Now.ToString();
        }

        public bool Action()
        {
            valves.Remove(valve);
            if (!valves.Contains(valve))
            {
                NotificationService.Instance.ShowSuccess($"Valve {valve.Id} deleted", "SUCCESS Delete");
                return true;
            }
            else
                NotificationService.Instance.ShowError($"Valve {valve.Id} is not deleted", "ERROR Delete");

            return false;
        }

        public bool Undo()
        {
            valves.Add(valve); //putting it back
            if (valves.Contains(valve))
            {
                NotificationService.Instance.ShowSuccess($"Undo {actionTitle} success", "SUCCESS Undo Delete");
                return true;
            }
            else
                NotificationService.Instance.ShowError($"Cannot Undo {actionTitle}", "ERROR Undo Delete");
            return false;
        }

        public string getTitle()
        {
            return actionTitle;
        }

        public string getDateTime()
        {
            return dateTime;
        }
    }
}
