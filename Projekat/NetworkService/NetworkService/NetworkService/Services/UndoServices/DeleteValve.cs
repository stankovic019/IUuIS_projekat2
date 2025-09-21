using NetworkService.Model;
using NetworkService.Repositories;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Services.UndoServices
{
    public class DeleteValve : IUndoService
    {
        private readonly NetworkEntitiesViewModel viewModel;
        private readonly Valve valve; //deleted valve
        private string actionTitle;
        private string dateTime;

        public DeleteValve(NetworkEntitiesViewModel viewModel, Valve valve)
        {
            this.viewModel = viewModel;
            this.valve = valve;
            actionTitle = $"Delete Valve {valve.Id}";
            dateTime = DateTime.Now.ToString();
        }

        public void Undo()
        {
            viewModel.Valves.Add(valve);
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
