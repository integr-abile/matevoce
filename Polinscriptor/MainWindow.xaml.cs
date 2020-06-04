using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private string filePath;
        public MainWindow()
        {
            InitializeComponent();
        }

        #region UserActions

        private void About_Click(object sender, RoutedEventArgs e)
        {
            //TODO: aprire finestra coi riferimenti base alla versione, laboratorio polin, anno, ecc...
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FLAC Audio files (*.FLAC)|*.FLAC";
            if(ofd.ShowDialog() == true)
            {
                filePath = ofd.FileName;
            }
        }

        private void Translate_Click(object sender, RoutedEventArgs e)
        {
            //chiamata alle API di Google
        }

        private void FlacConverter_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            //open browser
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true});
            e.Handled = true;
        }

        private void CopyTranscription_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TrascriptionTextBox.Text);
        }

        #endregion

    }
}
