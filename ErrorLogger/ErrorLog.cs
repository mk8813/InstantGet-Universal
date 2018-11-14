using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ErrorLogger
{
    public static class ErrorLog
    {
        private static StorageFolder local;

        private static StorageFile ErrorFile { get; set; }

        public static async Task WriteError(string message)
        {
            try
            {
                local = Windows.Storage.ApplicationData.Current.LocalCacheFolder;
                ErrorFile = await local.CreateFileAsync("errorlog.txt", CreationCollisionOption.OpenIfExists);
                if (ErrorFile != null)
                {
                    await Windows.Storage.FileIO.AppendTextAsync(ErrorFile, string.Format("\r\n============================================\r\n{0} - {1}\r\n", DateTime.Now.ToLocalTime().ToString(), message));
                }
            }
            catch
            {

            }
        }
    }
}
