using System;
using System.Threading.Tasks;
using FiverrNotifications.Logic.Models;

namespace FiverrNotifications.Logic.Services
{
    public interface IAccountsService : IDisposable
    {
        Task InitializeAsync();
        void Start();
        IObservable<SessionData> GetSessions();
    }
}
