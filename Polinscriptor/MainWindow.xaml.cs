using EricOulashin;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polinscriptor.Controllers;
using Polinscriptor.Models;
using Polinscriptor.Services;
using Polinscriptor.Services.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Polinscriptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string audioBase64;
        private string fileName;
        private string filePath;
        private string mostProbableTranscription;
        private AppState appState;
        private bool isRestRequestRunning;
        private bool isShortFile;
        private WAVFile curAudioFile;
        private string googleCompliantAudioFilePath;
        public AppState StatoApp
        {
            get => appState;
            set
            {
                appState = value;
                StateStatusLbl.Text = EnumToStringConverter.AppStatesToStringConverter(value);
            }
        }

        public bool IsRestRequestRunning
        {
            get => isRestRequestRunning;
            set
            {
                isRestRequestRunning = value;
                if (value == true)
                    ProgressBar.Visibility = Visibility.Visible;
                else
                    ProgressBar.Visibility = Visibility.Hidden;
            }
        }

        public string FilePath
        {
            get => filePath;
            set
            {
                filePath = value;
                FilePathStatusTxt.Text = value;
                ManageButtonsState();
            }
        }

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                ChosenFileTextBlock.Text = fileName;
            }
        }

        public string MostProbableTranscription
        {
            get => mostProbableTranscription;
            set
            {
                mostProbableTranscription = value;
                TranscriptionTextBox.Text = mostProbableTranscription;
                ManageButtonsState();
                if (!string.IsNullOrEmpty(mostProbableTranscription))
                    TranscriptionTextBox.IsEnabled = true;
                else
                    TranscriptionTextBox.IsEnabled = false;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            SetupUI();
        }
        #region UI

        private void SetupUI()
        {
            StatoApp = AppState.Idle;
            IsRestRequestRunning = false;
            MostProbableTranscription = string.Empty;
        }

        private void ManageButtonsState()
        {
            if (!string.IsNullOrEmpty(MostProbableTranscription))
                CopyTranscriptionBtn.IsEnabled = true;
            else
                CopyTranscriptionBtn.IsEnabled = false;
            if (!string.IsNullOrEmpty(FilePath))
                TranslateBtn.IsEnabled = true;
            else
                TranslateBtn.IsEnabled = false;
        }

        #endregion

        #region UserActions

        private void About_Click(object sender, RoutedEventArgs e)
        {
            //TODO: aprire finestra coi riferimenti base alla versione, laboratorio polin, anno, ecc...
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "WAV Audio files (*.wav)|*.wav";
            if(ofd.ShowDialog() == true)
            {
                var filePath = ofd.FileName;
                try
                {
                    googleCompliantAudioFilePath = $"{System.IO.Path.GetDirectoryName(filePath)}\\polinscriptor_g_audio.wav";
                    WAVFile.CopyAndConvert(filePath, googleCompliantAudioFilePath, 16, false);
                    audioBase64 = FileUtility.ReadFileAsBase64(googleCompliantAudioFilePath);
                    WAVFile audioFile = new WAVFile();
                    string openRes = audioFile.Open(googleCompliantAudioFilePath, WAVFile.WAVFileMode.READ);
                    if(openRes == string.Empty)
                    {
                        curAudioFile = audioFile;
                        FileName = System.IO.Path.GetFileName(filePath);
                        FilePath = filePath;
                        
                        var audioDuration = audioFile.NumSamples / audioFile.SampleRateHz;
                        if (audioDuration < App.Data.SEC_THRESH_AUDIO_LONG_SHORT)
                        {
                            isShortFile = true;
                        }
                        else
                        {
                            isShortFile = false;
                        }
                    }
                    audioFile.Close();

                } catch(WAVFileException ex)
                {
                    MessageBox.Show("Si è verificato un errore nell'apertura del file. Verifica che non sia già aperto in un altro programma");
                    Debug.WriteLine($"errore lettura file: {ex}");
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }

        private async void ContinuousSpeech_Click(object sender, RoutedEventArgs e)
        {
            await ContinuousSpeechService.StreamingMicRecognizeAsync(30); //TODO: parametrizzare il tempo per il quale il sistema deve stare ad ascoltare
        }

        private async void Translate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(filePath))
                return;
            if (isShortFile)
            {
                string json = GoogleShortAPIJsonPayload;
                IsRestRequestRunning = true;
                StatoApp = AppState.Recognizing;
                var (success, mostProbableTranscription) = await new APIController().TranslateShortText(json);
                IsRestRequestRunning = false;
                if (success)
                {
                    Debug.WriteLine(mostProbableTranscription);
                    StatoApp = AppState.Done;
                    MostProbableTranscription = mostProbableTranscription.Text;
                }
                else
                {
                    StatoApp = AppState.Error;
                    MostProbableTranscription = string.Empty;
                    MessageBox.Show("C'è stato un errore nella chiamata alle API di Google");
                }
            }
            else //se il file è lungo...
            {
                var part = 1;
                long sampleIn60Seconds = curAudioFile.SampleRateHz * 59;
                List<string> filePartNames = new List<string>();
                if (curAudioFile.Open(googleCompliantAudioFilePath, WAVFile.WAVFileMode.READ) != string.Empty)
                    return;
                do
                {
                    WAVFile wavPart = new WAVFile();
                    var partFilename = $"{System.IO.Path.GetDirectoryName(FilePath)}\\polinscriptor_wavPart{part}.wav";
                    wavPart.Create(partFilename,false,curAudioFile.SampleRateHz,curAudioFile.BitsPerSample);
                    if(wavPart.Open(partFilename,WAVFile.WAVFileMode.READ_WRITE) == string.Empty)
                    {
                        for (int i = 0; i < sampleIn60Seconds; i++)
                        {
                            wavPart.AddSample_16bit(curAudioFile.GetNextSampleAs16Bit());
                            if (curAudioFile.NumSamplesRemaining == 0)
                                break;
                        }
                        wavPart.Close();
                        part++;
                        filePartNames.Add(partFilename);
                    }

                }
                while (curAudioFile.NumSamplesRemaining > 0);
                curAudioFile.Close();

                StringBuilder sb = new StringBuilder();
                IsRestRequestRunning = true;
                StatoApp = AppState.Recognizing;
                bool error = false;
                foreach (string wavPartFilePath in filePartNames)
                {
                    string json = CreateGoogleJSONPayloadOfFile(wavPartFilePath);
                    var (success, mostProbableTranscription) = await new APIController().TranslateShortText(json);
                    if (success)
                    {
                        sb.Append($"{mostProbableTranscription.Text} ");
                    }
                    else
                    {
                        error = true;
                        Debug.WriteLine("errore in parte delle API");
                    }
                    
                }
                IsRestRequestRunning = false;
                if (error)
                    StatoApp = AppState.Error;
                else
                    StatoApp = AppState.Done;
                MostProbableTranscription = sb.ToString();
                foreach(string tmpPartFile in filePartNames)
                {
                    if(File.Exists(tmpPartFile))
                        File.Delete(tmpPartFile);
                }
                if(File.Exists(googleCompliantAudioFilePath))
                    File.Delete(googleCompliantAudioFilePath);
            }
                
        }

        #endregion


        private string GoogleShortAPIJsonPayload
        {
            get
            {
                var dataConfig = new GoogleDataConfig
                {
                    SampleRateHz = curAudioFile.SampleRateHz,
                    Encoding = "LINEAR16",
                    LanguageCode = ((ComboBoxItem)LangCombobox.SelectedItem).Content.ToString()
                };
                JObject audioJObj = new JObject();
                audioJObj["config"] = JObject.FromObject(dataConfig);
                JObject audioContentJObj = new JObject();
                audioContentJObj["content"] = audioBase64;
                audioJObj["audio"] = audioContentJObj;
                return audioJObj.ToString();
            }
        }

        private string CreateGoogleJSONPayloadOfFile(string filePath)
        {
            Debug.WriteLine($"Processing {filePath}");
            string audioBase64 = FileUtility.ReadFileAsBase64(filePath);
            WAVFile audioFile = new WAVFile();
            string openRes = audioFile.Open(filePath, WAVFile.WAVFileMode.READ);
            if (openRes != string.Empty)
            {
                Debug.WriteLine("Errore nella generazione del json di configurazione Google");
                return null;
            }
                
            var dataConfig = new GoogleDataConfig
            {
                SampleRateHz = audioFile.SampleRateHz,
                Encoding = "LINEAR16",
                LanguageCode = ((ComboBoxItem)LangCombobox.SelectedItem).Content.ToString()
            };
            JObject audioJObj = new JObject();
            audioJObj["config"] = JObject.FromObject(dataConfig);
            JObject audioContentJObj = new JObject();
            audioContentJObj["content"] = audioBase64;
            audioJObj["audio"] = audioContentJObj;

            audioFile.Close();
            return audioJObj.ToString();
        }

        private void CopyTranscription_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TranscriptionTextBox.Text);
        }


        

        
    }
}
