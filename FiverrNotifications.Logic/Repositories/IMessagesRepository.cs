using FiverrNotifications.Logic.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Repositories
{
    public interface IMessagesRepository
    {
        Task<IReadOnlyCollection<FiverrRequest>> FindNewRequests(int sessionId, IReadOnlyCollection<FiverrRequest> requests);
    }
}
