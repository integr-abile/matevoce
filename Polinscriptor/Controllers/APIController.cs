using System;
using System.Collections.Generic;
using System.Text;

namespace Polinscriptor.Controllers
{
    class APIController
    {
        private string shortAudioURL = "https://speech.googleapis.com/v1/speech:recognize";
        private string longAudioURL = "https://speech.googleapis.com/v1/speech:longrunningrecognize";
        private string googleSttOperationURL = "https://speech:googleapis.com/v1/operations/{0}";
    }
}
