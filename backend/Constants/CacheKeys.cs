using SPA_app.Enums;

namespace SPA_app.Constants
{
    public static class CacheKeys
    {
        public static string CommentsCacheKey(int page, CommentSorting sorting, bool desc)
        {
            return $"comments_{page}_{sorting}_{desc}";
        }
        public static string CaptchaCacheKey(string id)
        {
            return $"captcha_{id}";
        }
        public static IEnumerable<string> FirstPageKeys()
        {
            yield return CommentsCacheKey(0, CommentSorting.CreatedAt, true);

            yield return CommentsCacheKey(0, CommentSorting.CreatedAt, false);

            yield return CommentsCacheKey(0, CommentSorting.UserName, true);

            yield return CommentsCacheKey(0, CommentSorting.UserName, false);

            yield return CommentsCacheKey(0, CommentSorting.Email, true);

            yield return CommentsCacheKey(0, CommentSorting.Email, false);
        }
    }
}
