using System;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Models
{
    public interface ISessionCommunicator
    {
        Task SendMessage(FiverrRequest r);
        Task SendMessage(MessageType messageType);
        IObservable<string> Messages { get; }
    }
}
