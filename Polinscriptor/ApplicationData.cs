using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Polinscriptor
{
    public class ApplicationData
    {

        public string G_API_KEY = string.Empty;
        public int SEC_THRESH_AUDIO_LONG_SHORT = 60;

        public string GetAppConfigValue(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }
    }
}
