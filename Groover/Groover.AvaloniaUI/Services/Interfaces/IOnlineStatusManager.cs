namespace Groover.AvaloniaUI.Services.Interfaces
{
    public interface IOnlineStatusManager
    {
        bool Disconnected(int userId, int groupId);
        bool GetUserStatuc(int userId, int groupId);
        bool LoggedOn(int userId, int groupId);
        void Reset();
    }
}