using Google.Cloud.Speech.V1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Polinscriptor.Services.Audio
{
    public class ContinuousSpeechService
    {
        public static async Task<object> StreamingMicRecognizeAsync(int seconds)
        {
            var speech = SpeechClient.Create();
            var streamingCall = speech.StreamingRecognize();
            await streamingCall.WriteAsync(new StreamingRecognizeRequest()
            {
                StreamingConfig = new StreamingRecognitionConfig()
                {
                    Config = new RecognitionConfig()
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 16000,
                        LanguageCode = "en"
                    },
                    InterimResults = true
                }
            });
            //Stama risposte in tempo reale
            Task printResponses = Task.Run(async () =>
            {
                var responseStream = streamingCall.GetResponseStream();
                while (await responseStream.MoveNextAsync())
                {
                    StreamingRecognizeResponse response = responseStream.Current;
                    foreach (StreamingRecognitionResult result in response.Results)
                    {
                        foreach (SpeechRecognitionAlternative alternative in result.Alternatives)
                        {
                            Debug.WriteLine(alternative.Transcript);
                        }
                    }
                }
            });
            //Lettura dal microfono e stream alle API di Google
            object writeLock = new object();
            bool writeMore = true;
            var waveIn = new NAudio.Wave.WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
            waveIn.DataAvailable += (object sender, NAudio.Wave.WaveInEventArgs args) =>
            {
                lock (writeLock)
                {
                    if (!writeMore)
                    {
                        return;
                    }
                    streamingCall.WriteAsync(new StreamingRecognizeRequest()
                    {
                        AudioContent = Google.Protobuf.ByteString.CopyFrom(args.Buffer, 0, args.BytesRecorded)
                    }).Wait();
                }
            };
            waveIn.StartRecording();
            Debug.WriteLine("Parla ora!");
            await Task.Delay(TimeSpan.FromSeconds(seconds)); //il task è programmato per finire dopo seconds secondi
            //stop recording and shut down
            waveIn.StopRecording();
            lock (writeLock)
            {
                writeMore = false;
            }
            await streamingCall.WriteCompleteAsync();
            await printResponses;

            return 0;
        }
    }
}
