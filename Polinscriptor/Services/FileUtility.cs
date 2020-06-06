using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Polinscriptor.Services
{
    class FileUtility
    {
        public static string ReadFileAsBase64(string filePath)
        {
            FileStream fs = File.OpenRead(filePath);
            byte[] fileBytes = new byte[fs.Length];
            fs.Read(fileBytes, 0, fileBytes.Length);
            fs.Close();
            return Convert.ToBase64String(fileBytes);
        }
    }
}
