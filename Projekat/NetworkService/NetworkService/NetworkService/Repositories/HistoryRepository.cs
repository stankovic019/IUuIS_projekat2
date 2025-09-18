using NetworkService.Services.UndoServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Repositories
{
    public class HistoryRepository
    {
        private static HistoryRepository _instance;

        public static HistoryRepository Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HistoryRepository();
                return _instance;
            }
        }

        private Stack<IUndoService> undoStack;

        public HistoryRepository() {
            undoStack = new Stack<IUndoService>();
        }

        public Stack<IUndoService> UndoStack { get { return undoStack; } }
     
    }
}


