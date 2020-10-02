using System;
using System.Text.RegularExpressions;

namespace FiverrNotifications.Telegram
{
    public class MessageSanitizer
    {
        private static readonly string Dash = "-";
        private readonly Regex _sanitizeRegexp = new Regex("[\\s!@#$%^&*(),.?\":{}|<>–]", RegexOptions.Compiled);
        private readonly Regex _minimizeDashesRegexp = new Regex("-{2,}");

        private readonly Regex _specialCharacters = new Regex("[\\[\\]()!.–-]", RegexOptions.Compiled);

        public string SanitizeUrlComponent(string part)
        {
            if (string.IsNullOrWhiteSpace(part))
                throw new ArgumentException("Should not be empty", nameof(part));

            part = part.ToLower();
            part = _sanitizeRegexp.Replace(part, Dash);
            part = _minimizeDashesRegexp.Replace(part, Dash);

            return part.Trim(' ', '-');
        }

        public string EscapeString(string str)
        {
            if (!_specialCharacters.IsMatch(str))
                return str;

            return _specialCharacters.Replace(str, el => $"\\{el}");
        }
    }
}
