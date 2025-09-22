using NetworkService.Model;
using NetworkService.ViewModel;
using System;

namespace NetworkService.Services.UndoServices
{
    public class DropOnCanvas : IUndoService
    {

        private readonly NetworkDisplayViewModel viewModel;
        private readonly Valve valve;
        private readonly int destCanvasIdx;
        private readonly int srcCanvasIdx;
        private readonly bool draggedFromList;
        private readonly string dateTime;

        public DropOnCanvas(NetworkDisplayViewModel viewModel, Valve valve, int destCanvasIdx, int srcCanvasIdx)
        {
            this.viewModel = viewModel;
            this.valve = valve;
            this.destCanvasIdx = destCanvasIdx;
            this.srcCanvasIdx = srcCanvasIdx;
            this.draggedFromList = srcCanvasIdx == -1;
            this.dateTime = DateTime.Now.ToString();
        }

        public void Undo()
        {
            //returning valve where it was
            if (draggedFromList)
            {
                viewModel.IsMoving = false;
                viewModel.draggingSourceIndex = -1;
                viewModel.PerformFreeCanvas(destCanvasIdx, valve);
            }
            else
            {
                viewModel.IsMoving = true;
                viewModel.PerformFreeCanvas(destCanvasIdx, valve);
                viewModel.draggingSourceIndex = destCanvasIdx;
                viewModel.RedoDropOnCanvas(valve, srcCanvasIdx, destCanvasIdx);
            }
        }

        public string getDateTime()
        {
            return dateTime;
        }

        public string getTitle()
        {

            if (draggedFromList)
                return $"Drop Valve {valve.Id} on field {destCanvasIdx + 1}";
            else
                return $"Move Valve {valve.Id} from field {srcCanvasIdx + 1} to field {destCanvasIdx + 1}";
        }



    }
}
