using Polinscriptor.Models;
using Polinscriptor.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Polinscriptor.Controllers
{
    class APIController
    {
        private string shortAudioURL = "https://speech.googleapis.com/v1/speech:recognize?key={0}";
        private string longAudioURL = "https://speech.googleapis.com/v1/speech:longrunningrecognize?key={0}";
        private string googleSttOperationURL = "https://speech:googleapis.com/v1/operations/{0}?key={1}";

        public async Task<(bool,string)> TranslateShortText(string languageCode, string json)
        {
            try
            {
                var answer = await new RestService().PostRequest(string.Format(shortAudioURL, App.Data.G_API_KEY), json);
                //parsare JObject per estrarre trascrizione più probabile
            } catch(Exception e)
            {
                return (false, string.Empty);
            }
        }  
    }
}
