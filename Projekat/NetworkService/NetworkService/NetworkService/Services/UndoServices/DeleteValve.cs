using NetworkService.Model;
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

        public DeleteValve(ObservableCollection<Valve> valves, Valve valve)
        {
            this.valves = valves;
            this.valve = valve;
            actionTitle = $"Delete Valve {valve.Id}";
        }

        public void Undo()
        {
            valves.Add(valve); //putting it back
            if (valves.Contains(valve))
                NotificationService.Instance.ShowSuccess($"Undo {actionTitle} success", "SUCCESS Undo Delete");
            else
                NotificationService.Instance.ShowError($"Cannot Undo {actionTitle}", "ERROR Undo Delete");
        }
        


    }
}
