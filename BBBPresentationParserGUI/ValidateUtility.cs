using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BBBPresentationParser
{
    internal class ValidatorUtility
    {
        public static bool ValidatePresentationUrl(ref string url)
        {
            if (string.IsNullOrEmpty(url) ||
                !url.StartsWith("https://") ||
                (!url.Contains("bsu.edu.ru/") && !url.Contains("bsu-eis.ru/")))
                return false;

            string[] parts = url.Split('/');
            url = string.Join("/", parts.Take(parts.Length - 1).ToArray()) + "/";
            return true;
        }
    }
}
