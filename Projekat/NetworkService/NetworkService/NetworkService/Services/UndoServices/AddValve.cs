using NetworkService.Model;
using NetworkService.ViewModel;
using System;

namespace NetworkService.Services.UndoServices
{
    public class AddValve : IUndoService
    {

        private readonly AddEntityViewModel viewModel;
        private readonly Valve valve;
        private readonly string actionTitle;
        private readonly string dateTime;

        public AddValve(AddEntityViewModel viewModel, Valve valve)
        {
            this.viewModel = viewModel;
            this.valve = valve;
            actionTitle = $"Add Valve {valve.Id}";
            dateTime = DateTime.Now.ToString();
        }

        public void Undo()
        {
            viewModel.Valves.Remove(valve);
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
