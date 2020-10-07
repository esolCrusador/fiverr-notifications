using System;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Models
{
    public interface ISessionCommunicator
    {
        Task SendMessage(FiverrRequest r, bool notify);
        Task SendMessage(MessageType messageType, bool notify);
        IObservable<string> Messages { get; }
    }
}
