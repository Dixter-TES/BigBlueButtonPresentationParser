using System.Linq;


namespace BBBPresentationParser
{
    internal static class ValidatorUtility
    {
        public static bool ValidatePresentationUrl(ref string url)
        {
            if (string.IsNullOrEmpty(url) ||
                !url.StartsWith("https://") ||
                (!url.Contains("bsu.edu.ru/") && !url.Contains("bsu-eis.ru/")))
                return false;

            var parts = url.Split('/');
            url = string.Join("/", parts.Take(parts.Length - 1).ToArray()) + "/";
            return true;
        }
    }
}
