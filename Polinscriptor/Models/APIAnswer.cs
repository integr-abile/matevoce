using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Polinscriptor.Models
{
    struct APIAnswer
    {
        public HttpStatusCode StatusCode;
        public JObject JsonAnswer;

        public APIAnswer(HttpStatusCode statusCode, JObject jsonAnswer)
        {
            StatusCode = statusCode;
            JsonAnswer = jsonAnswer;
        }
    }
}
