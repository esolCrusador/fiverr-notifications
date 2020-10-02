using FiverrNotifications.Logic.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Clients
{
    public interface IFiverrClient: IDisposable
    {
        public Task<IReadOnlyCollection<FiverrRequest>> GetRequsts(string userName, Guid session, string token);
    }
}
