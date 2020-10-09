using FiverrNotifications.Telegram;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Reflection;

namespace FiverrNotifications
{
    public class ResourceResolver: IResourceResolver
    {
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        public ResourceResolver()
        {
            _embeddedFileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        }

        public Stream GetResourceStream(string uri)
        {
            return _embeddedFileProvider.GetFileInfo(uri).CreateReadStream();
        }
    }
}
