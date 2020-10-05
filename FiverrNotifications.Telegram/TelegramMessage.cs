namespace FiverrNotifications.Telegram
{
    public class TelegramMessage
    {
        public TelegramMessageType Type { get; }
        public string ImageUrl { get; }
        public string Text { get; }

        public TelegramMessage(TelegramMessageType type, string text, string imageUrl)
        {
            Type = type;
            Text = text;
            ImageUrl = imageUrl;
        }

        public static TelegramMessage TextMessage(string message) => new TelegramMessage(TelegramMessageType.Text, message, null);
        public static TelegramMessage PhotoMessage(string message, string imageUrl) => new TelegramMessage(TelegramMessageType.Photo, message, imageUrl);
    }
}
