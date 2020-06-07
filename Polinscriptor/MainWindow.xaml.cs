using EricOulashin;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polinscriptor.Controllers;
using Polinscriptor.Models;
using Polinscriptor.Services;
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
                    var googleCompliantAudioFilePath = $"{System.IO.Path.GetDirectoryName(filePath)}\\polinscriptor_g_audio.wav";
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

        private async void Translate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(filePath))
                return;
            if (isShortFile)
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
                string json = audioJObj.ToString();
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
            else
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
                string json = audioJObj.ToString();
                IsRestRequestRunning = true;
                StatoApp = AppState.Recognizing;
                var (success, opName) = await new APIController().SendLongTranslationRequest(json);
                if (success)
                {
                    var (getTranslationSuccess, mostProbableTranscription) = await new APIController().GetTranslationFromGoogleOperationName(opName);
                    if (getTranslationSuccess)
                    {
                        StatoApp = AppState.Done;
                        MostProbableTranscription = mostProbableTranscription.Text;
                    }
                    else
                    {
                        StatoApp = AppState.Error;
                        MostProbableTranscription = string.Empty;
                        MessageBox.Show("C'è stato un errore nella chiamata alle API di Google.");
                    }
                }
                else
                {
                    StatoApp = AppState.Error;
                    MostProbableTranscription = string.Empty;
                    MessageBox.Show("C'è stato un errore nella chiamata alle API di Google");
                }

            }
                
        }

        private void CopyTranscription_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TranscriptionTextBox.Text);
        }

        #endregion

    }
}
