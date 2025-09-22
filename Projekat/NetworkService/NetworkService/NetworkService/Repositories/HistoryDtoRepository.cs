using NetworkService.DTOs;
using System.Collections.ObjectModel;

namespace NetworkService.Repositories
{
    public class HistoryDtoRepository
    {
        private static HistoryDtoRepository _instance;

        public static HistoryDtoRepository Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HistoryDtoRepository();
                return _instance;
            }
        }

        private ObservableCollection<HistoryDto> historyDtos;

        public HistoryDtoRepository()
        {
            historyDtos = new ObservableCollection<HistoryDto>();
        }

        public ObservableCollection<HistoryDto> HistoryDtos { get { return historyDtos; } }

    }
}
