using System.IO;

namespace FiverrNotifications.Telegram
{
    public interface IResourceResolver
    {
        public Stream GetResourceStream(string uri);
    }
}
