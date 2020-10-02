using System.Collections.Concurrent;
using FiverrNotifications.Logic.Models;
using Telegram.Bot;

namespace FiverrNotifications.Telegram
{
    public class BotClientFactory
    {
        private readonly ConcurrentDictionary<long, TelegramBotClient> _clients = new ConcurrentDictionary<long, TelegramBotClient>();

        public TelegramBotClient GetClient(BotData bot) => _clients.GetOrAdd(bot.BotId, _ => CreateClient(bot));

        private TelegramBotClient CreateClient(BotData bot) => new TelegramBotClient($"{bot.BotId}:{bot.BotSecret}");
    }
}
