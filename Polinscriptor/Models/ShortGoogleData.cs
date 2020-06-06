using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polinscriptor.Models
{
    class ShortGoogleDataConfig
    {
        [JsonProperty("encoding")]
        public string Encoding { get; set; }
        [JsonProperty("sampleRateHertz")]
        public int SampleRateHz { get; set; }
        [JsonProperty("languageCode")]
        public string LanguageCode { get; set; }
    }

    //Answer
    class Transcription
    {
        [JsonProperty("transcript")]
        public string Text { get; set; }
        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    class Alternatives
    {
        [JsonProperty("alternatives")]
        public Transcription[] TranscriptionAlternatives { get; set; }
    }
}
