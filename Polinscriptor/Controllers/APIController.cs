﻿using Newtonsoft.Json.Linq;
using Polinscriptor.Models;
using Polinscriptor.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Polinscriptor.Controllers
{
    class APIController
    {
        private string shortAudioURL = "https://speech.googleapis.com/v1/speech:recognize?key={0}";
        //Non usiamo le long api perchè richiedono che per file di durata superiore a 1 minuto, essi debbano essere contenuti in GoogleCloudStore e non possono essere mandati in Base64
        //nemmeno tramite le API long running recognizing

        public async Task<(bool,Transcription)> TranslateShortText(string json)
        {
            try
            {
                var answer = await new RestService().PostRequest(string.Format(shortAudioURL, App.Data.G_API_KEY), json);
                if(answer.StatusCode == HttpStatusCode.OK)
                {
                    var results = (JObject)answer.JsonAnswer["results"][0];
                    var alternatives = results.ToObject<Alternatives>();
                    var mostProbableTranscriptionConfidence = alternatives.TranscriptionAlternatives.Max(alternative => alternative.Confidence);
                    var mostProbableTranscription = from alternative in alternatives.TranscriptionAlternatives 
                                                    where alternative.Confidence == mostProbableTranscriptionConfidence select alternative; 
                    return (true, mostProbableTranscription.First());
                }
                return (false, null);
            } catch(Exception e)
            {
                return (false, null);
            }
        }
    }
}
