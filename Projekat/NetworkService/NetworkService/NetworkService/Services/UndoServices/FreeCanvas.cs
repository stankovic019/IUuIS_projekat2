using NetworkService.Model;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;

namespace NetworkService.Services.UndoServices
{
    public class FreeCanvas : IUndoService
    {
        private readonly NetworkDisplayViewModel viewModel;
        private readonly Valve valve;
        private readonly int canvasIndex;
        private readonly List<NewLine> lines;
        private readonly string dateTime;
        private readonly string actionTitle;

        public FreeCanvas(NetworkDisplayViewModel viewModel, Valve valve, int canvasIndex, List<NewLine> lines)
        {
            this.viewModel = viewModel;
            this.valve = valve;
            this.canvasIndex = canvasIndex;
            this.lines = new List<NewLine>(lines);
            this.dateTime = DateTime.Now.ToString();
            this.actionTitle = $"Valve {valve.Id} removed from field {canvasIndex + 1}";
        }

        public void Undo()
        {
            viewModel.RedoDropOnCanvas(valve, canvasIndex, -1);
            viewModel.ValvesInList.Remove(valve);

            foreach (var line in lines)
            {
                viewModel.LineCollection.Add(line);
            }
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