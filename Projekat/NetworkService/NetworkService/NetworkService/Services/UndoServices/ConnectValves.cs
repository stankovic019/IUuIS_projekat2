using NetworkService.Model;
using NetworkService.ViewModel;
using System;

namespace NetworkService.Services.UndoServices
{
    public class ConnectValves : IUndoService
    {
        private readonly NetworkDisplayViewModel viewModel;
        private readonly NewLine line;
        private readonly string actionTitle;
        private readonly string dateTime;


        public ConnectValves(NetworkDisplayViewModel viewModel, NewLine line, int cIdx1, int cIdx2)
        {
            this.viewModel = viewModel;
            this.line = line;
            this.actionTitle = $"Fields {cIdx1 + 1} and {cIdx2 + 1} connected";
            this.dateTime = DateTime.Now.ToString();
        }
        public void Undo()
        {
            viewModel.LineCollection.Remove(line);
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
