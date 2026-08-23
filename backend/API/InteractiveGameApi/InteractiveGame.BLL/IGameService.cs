namespace InteractiveGameApi.InteractiveGame.BLL
{
    public interface IGameService
    {
        void UpdateStatus(string status);
        (string LastStatus, int Mistakes) GetStatus();
        void Reset();
        Task SaveResultAsync();
    }

}
