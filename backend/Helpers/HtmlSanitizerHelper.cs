namespace SPA_приложение.Helpers
{
    using Ganss.Xss;
    using System.Text.RegularExpressions;

    public static class HtmlSanitizerHelper
    {
        public static string Sanitize(string html)
        {
            var sanitizer = new HtmlSanitizer();

            sanitizer.AllowedTags.Clear();

            sanitizer.AllowedTags.Add("a");
            sanitizer.AllowedTags.Add("code");
            sanitizer.AllowedTags.Add("i");
            sanitizer.AllowedTags.Add("strong");

            sanitizer.AllowedAttributes.Clear();

            sanitizer.AllowedAttributes.Add("href");
            sanitizer.AllowedAttributes.Add("title");

            return sanitizer.Sanitize(html);
        }
        public static bool IsValid(string html)
        {
            try
            {
                var sanitizer = new HtmlSanitizer();

                sanitizer.AllowedTags.Clear();

                sanitizer.AllowedTags.Add("a");
                sanitizer.AllowedTags.Add("code");
                sanitizer.AllowedTags.Add("i");
                sanitizer.AllowedTags.Add("strong");

                sanitizer.AllowedAttributes.Clear();

                sanitizer.AllowedAttributes.Add("href");
                sanitizer.AllowedAttributes.Add("title");

                var sanitized = sanitizer.Sanitize(html);

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
