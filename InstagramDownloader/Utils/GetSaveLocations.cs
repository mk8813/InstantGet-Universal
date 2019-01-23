using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
namespace InstagramDownloader
{
  public static  class GetSaveLocations
    {

        public static async Task<StorageFolder> GetPicturesFolder()
        {
            ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
            try
            {
                string picturepath = AppSettings.Values["picture"].ToString();
                var folder =await StorageFolder.GetFolderFromPathAsync(picturepath);
                return folder;
            }
            catch (Exception)
            {
                return null;
               
            }
        }

        public static async Task<StorageFolder> GetVideosFolder()
        {
            ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
            try
            {
                string picturepath = AppSettings.Values["video"].ToString();
                var folder = await StorageFolder.GetFolderFromPathAsync(picturepath);
                return folder;
            }
            catch (Exception)
            {
                return null;

            }
        }

    }
}
