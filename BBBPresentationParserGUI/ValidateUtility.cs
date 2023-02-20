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
            string pattern = @"^https?:\/\/[a-z](webinar.bsu.edu.ru\/)|(webinar.bsu\-eis.ru\/)";
            Regex regex = new Regex(pattern);

            if (!regex.IsMatch(url))
                return false;

            string[] parts = url.Split('/');
            url = string.Join("/", parts.Take(parts.Length - 1)?.ToArray()!) + "/";
            return true;
        }
    }
}
