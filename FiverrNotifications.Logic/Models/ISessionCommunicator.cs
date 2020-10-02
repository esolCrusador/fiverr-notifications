using System;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Models
{
    public interface ISessionCommunicator
    {
        Task SendMessage(string message);
        Task SendMessage(FiverrRequest r);
        IObservable<string> Messages { get; }
    }
}
