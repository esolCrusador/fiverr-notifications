using System.Collections.Generic;
using System.Threading.Tasks;
using FiverrNotifications.Logic.Models;

namespace FiverrNotifications.Logic.Repositories
{
    public interface IChatsRepository
    {
        Task<StoredSession> AddChat(int botId, long chatId);
        Task<StoredSession> RemoveChat(int botId, long chatId);
        Task<IReadOnlyCollection<BotData>> GetBots();
        Task<IReadOnlyCollection<StoredSession>> GetSessions(IReadOnlyCollection<int> botIds);
        Task UpdateSession(StoredSession storedSession);
    }
}
