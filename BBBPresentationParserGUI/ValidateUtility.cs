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
        private const string BBBUrlValidationPattern = @"^https?:\/\/[a-z](webinar.bsu.edu.ru\/)|(webinar.bsu\-eis.ru\/)";

        public static bool ValidatePresentationUrl(ref string url)
        {
            Regex regex = new Regex(BBBUrlValidationPattern);

            if (!regex.IsMatch(url))
                return false;

            string[] parts = url.Split('/');
            url = string.Join("/", parts.Take(parts.Length - 1)?.ToArray()!) + "/";
            return true;
        }
    }
}
