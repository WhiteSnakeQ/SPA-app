namespace SPA_приложение.Helpers
{
    using Ganss.Xss;
    using System.Text.RegularExpressions;

    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer;
        static HtmlSanitizerHelper()
        {
            _sanitizer = new HtmlSanitizer();
            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedTags.Add("a");
            _sanitizer.AllowedTags.Add("code");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("strong");

            _sanitizer.AllowedAttributes.Clear();
            _sanitizer.AllowedAttributes.Add("href");
            _sanitizer.AllowedAttributes.Add("title");
        }

        public static string Sanitize(string html)
        {
            return _sanitizer.Sanitize(html);
        }

        public static bool IsValid(string html)
        {
            try
            {
                var sanitized = _sanitizer.Sanitize(html);
                return sanitized == html;
            }
            catch
            {
                return false;
            }
        }

        public static bool HaveTextContent(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return false;

            var text = Regex.Replace(html, "<.*?>", "");

            return !string.IsNullOrWhiteSpace(text);
        }

        public static bool BeValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(
                url,
                UriKind.Absolute,
                out var result)
                &&
                (
                    result.Scheme == Uri.UriSchemeHttp
                    ||
                    result.Scheme == Uri.UriSchemeHttps
                );
        }
    }
}
