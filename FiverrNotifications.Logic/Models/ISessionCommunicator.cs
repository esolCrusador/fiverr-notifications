using FiverrNotifications.Logic.Models.Messages;
using System;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Models
{
    public interface ISessionCommunicator
    {
        Task<int> SendMessage(FiverrRequest r, bool notify);
        Task<int> SendMessage(StandardMessage messageType, bool notify);
        Task<int> SendMessage(StandardMessage messageType, bool notify, string[] arguments);
        Task<int> SendMessage(TelegramMessage message, bool notify);
        Task DeleteMessage(int messageId);
        IObservable<string> Messages { get; }
        IObservable<string> Replies { get; }
        IObservable<Location> LocationMessages { get; }
    }
}
