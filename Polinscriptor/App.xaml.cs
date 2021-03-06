﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Polinscriptor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ApplicationData Data { get; } = new ApplicationData();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "polinspeech-6f20c9425b61.json");
        }
    }
}
