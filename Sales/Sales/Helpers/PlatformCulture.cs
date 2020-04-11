using System;
using System.Collections.Generic;
using System.Text;

namespace Sales.Helpers
{
    public class PlatformCulture
    {
        public PlatformCulture(string PlatformCultureString)
        {
            if (string.IsNullOrEmpty(PlatformCultureString))
            {
                throw new ArgumentException("Expect culture identifier", "PlatformCultureString");
            }

            PlatformString = PlatformCultureString.Replace("_", "-");
            var dashIndex = PlatformString.IndexOf("-",StringComparison.Ordinal);
            if (dashIndex > 0)
            {
                var parts = PlatformString.Split('-');
                LanguageCode = parts[0];
                LocaleCode = parts[1];
            }
            else
            {
                LanguageCode = PlatformString;
                LocaleCode = "";
            }
        }
        public string PlatformString { get; private set; }
        public string LanguageCode { get; private set; }
        public string LocaleCode { get; private set; }

        public override string ToString()
        {
            return PlatformString;
        }
    }
}
