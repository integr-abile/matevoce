using System;
using System.Collections.Generic;
using System.Text;

namespace Polinscriptor.Models
{
    public enum AppState
    {
       Idle,Recognizing,Done,Error
    }

    public class EnumToStringConverter
    {
        public static string AppStatesToStringConverter(AppState appState)
        {
            switch (appState)
            {
                case AppState.Idle:
                    return "Pronto";
                case AppState.Done:
                    return "Fatto";
                case AppState.Recognizing:
                    return "Riconoscimento in corso...";
                case AppState.Error:
                    return "Errore";
                default:
                    return string.Empty;
            }
        }
    }
}
