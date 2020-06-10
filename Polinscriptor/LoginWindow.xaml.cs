using Polinscriptor.Controllers;
using Polinscriptor.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Windows;

namespace Polinscriptor
{
    /// <summary>
    /// Logica di interazione per LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTxt.Text;
            using (MD5 md5Hash = MD5.Create())
            {
                string pwdHash = CryptoService.GetMd5Hash(md5Hash, PasswordTxt.Password);
                var loginSuccess = await new LoginController().Login(username, pwdHash);
                if(loginSuccess == true)
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Login fallito", "Verifica di aver inserito le credenziali corrette", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
    }
}
