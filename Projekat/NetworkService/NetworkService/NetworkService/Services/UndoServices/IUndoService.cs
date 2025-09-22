namespace NetworkService.Services.UndoServices
{
    public interface IUndoService
    {
        void Undo();
        string getTitle();
        string getDateTime();
    }
}
