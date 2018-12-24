using QuickType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Imaging;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Notifications;
using Newtonsoft.Json;
using DbHelper;
using Windows.Web.Http;

namespace BackgroundDownloadHandler
{
    public class BackgroundDownloaderManager
    {
        DownloadOperation downloadOperation;
        ResourceLoader res = ResourceLoader.GetForViewIndependentUse();
        private bool IsFullSizeImageBought=false;
        private string _downloadedFilePath;
        private Dimensions _postDimensions;
        public string DownloadedFilePath
        {
            get
            {
                return _downloadedFilePath;
            }

            set
            {
                _downloadedFilePath = value;
            }
        }

        public Dimensions PostDimensions
        {
            get
            {
                return _postDimensions;
            }

            set
            {
                _postDimensions = value;
            }
        }

        private async Task<bool> CreateDownload(string url, StorageFile file, Progress<DownloadOperation> progress, bool isVideo = false)
        {
            try
            {

                CancellationTokenSource cancellationToken;
                BackgroundDownloader backgroundDownloader = new BackgroundDownloader();

                string filename = Path.GetFileName(url);


                backgroundDownloader.SuccessToastNotification = CreateSuccessToast(filename, file.Path, isVideo);
                backgroundDownloader.FailureToastNotification = CreateFailureToast(filename);


                Uri durl = new Uri(url);
                downloadOperation = backgroundDownloader.CreateDownload(durl, file);

                downloadOperation.CostPolicy = BackgroundTransferCostPolicy.Default;///////////////new
                downloadOperation.Priority = BackgroundTransferPriority.High;////////////new

               // Progress<DownloadOperation> progress = new Progress<DownloadOperation>(progressChanged);
                cancellationToken = new CancellationTokenSource();
                try
                {
                
                    await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
                 
                    return true;
                }
                catch (TaskCanceledException)
                {
                    var res = downloadOperation.ResultFile.DeleteAsync();
                    downloadOperation = null;

                    //if (_deferral!=null)
                    //{
                    //    _deferral.Complete();
                    //}
                    return false;
                }

            }
            catch (Exception)
            {

                return false;
            }


        }

       


        public async Task<bool> DownloadPost(string url, bool IsProfile,Progress<DownloadOperation> progress)
        {

            try
            {
                Uri durl = new Uri(url);
                PostDetails tuResult;
                tuResult =await new JsonPostParser.FetchResourceUrl().GetPostFromUrl(new Uri(url),IsProfile,IsFullSizeImageBought);

                try
                {

                    //await dloperation.StartAsync().AsTask(cancelTk.Token);

                    ///////////////////////////////////////////////////// saving item record
                    tbl_History curDownload = new tbl_History();
                    bool isSuccess = false;
                    curDownload.DateInserted = DateTime.Now.ToString();
                    //////////////////////////////////////////////////////
                  
                    if (!string.IsNullOrEmpty(tuResult.PostShortCode))
                    {
  
                        string imgSrc = tuResult.Src;
                        string postIdOrUsername =tuResult.PostShortCode;
                        bool isVideoLink = tuResult.IsVideo;
                        PostDimensions = tuResult.PostDimensions;
                        StorageFile outputfile =await SetOutputFile(isVideoLink, postIdOrUsername);


                        //////////////////////////

                        curDownload.SavePath = outputfile.Path;
                        curDownload.Type = isVideoLink ? "video" : "picture";
                        curDownload.Url = durl.ToString();


                        ///////////////////

                        if (!String.IsNullOrEmpty(imgSrc))
                        {


                            ShowNotification(res.GetString("DownloadInProgress"), true);
                            isSuccess = await CreateDownload(imgSrc, outputfile,progress, isVideoLink);
                            DownloadedFilePath = outputfile.Path;

                            if (isSuccess)
                            {
                                if (isVideoLink)
                                {
                                    /////////////save video thumbnail

                                    await LoadVideo(outputfile.Path);


                                    ////////////////////////////
                                }

                                curDownload.IsDownloaded = "1";


                            }
                            else
                            {
                                curDownload.IsDownloaded = "0";



                                if (isVideoLink)
                                {

                                    ShowNotification(res.GetString("ErrorDownloadVideo"));
                                }
                                else
                                {
                                    ShowNotification(res.GetString("ErrorDownloadPicture"));
                                }

                            }


                        }
                        else
                        {
                            curDownload.IsDownloaded = "0";
                            ShowNotification(res.GetString("TargetFileNotFound"));
                        }




                        ///////////////////////////


                    }
                    else
                    {
                        curDownload.IsDownloaded = "0";
                        ShowNotification(res.GetString("FailedToRetrieveUrl"));

                    }
                    //////////////////////////////////////
                    //save record
                    if (isSuccess)
                    {
                        try
                        {
                            ///////////////////save record to database
                            using (var db = new dbHelperConnection())
                            {

                                db.tbl_History.Add(curDownload);
                                await db.SaveChangesAsync();

                            }

                            ///////////////////////////
                        }
                        catch (Exception ex)
                        {
                            await ErrorLogger.ErrorLog.WriteError("Error in BackgroundTask_SaveToDbBlock_imgDownloader\r\n" + ex.Message + ex.StackTrace + ex.InnerException.Message + ex.InnerException.StackTrace);

                        }
                    }

                    return isSuccess;

                    /////////////////////////////////
                }
                catch (TaskCanceledException)
                {
                    var res = downloadOperation.ResultFile.DeleteAsync();
                    return false;

                }
                catch (Exception ex)
                {
                    ShowNotification(ex.Message);
                    return false;
                }

            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message);
                return false;
            }
            finally
            {
                GC.Collect();
            }


        }
        private string GetProfilePictureUrl(string url)
        {
            Regex profileRegex = new Regex(@"s[0-9][0-9][0-9]x[0-9][0-9][0-9]");
            if (profileRegex.IsMatch(url))
            {
                url = profileRegex.Replace(url, "s");
            }
            return url;
        }

        private async Task<StorageFolder> GetPicturesFolder()
        {
            ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
            try
            {
                string picturepath = AppSettings.Values["picture"].ToString();
                var folder = await StorageFolder.GetFolderFromPathAsync(picturepath);
                return folder;
            }
            catch (Exception)
            {
                return null;

            }
        }

        private async Task<StorageFolder> GetVideosFolder()
        {
            ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
            try
            {
                string videopath = AppSettings.Values["video"].ToString();
                var folder = await StorageFolder.GetFolderFromPathAsync(videopath);
                return folder;
            }
            catch (Exception)
            {
                return null;

            }
        }

        private async Task<StorageFile> SetOutputFile(bool isVideoLink,string postIdOrUsername)
        {
            StorageFile outputfile = null;
            StorageFolder savefolder;
            try
            {
                if (isVideoLink)
                {
                    ////////////////////
                    var Settingfolder = await GetVideosFolder();
                    if (Settingfolder != null)
                    {
                        savefolder = Settingfolder;
                    }
                    else
                    {
                        savefolder = KnownFolders.VideosLibrary;//.PickSingleFolderAsync();
                    }

                   return  outputfile = await savefolder.CreateFileAsync("InstantGet-" + postIdOrUsername + ".mp4", CreationCollisionOption.GenerateUniqueName);
                    //////////////////
                }
                else
                {
                    var Settingfolder = await GetPicturesFolder();
                    if (Settingfolder != null)
                    {
                        savefolder = Settingfolder;
                    }
                    else
                    {   ////////////////////
                        savefolder = KnownFolders.PicturesLibrary;//.PickSingleFolderAsync();
                    }
                 return   outputfile = await savefolder.CreateFileAsync("InstantGet-" + postIdOrUsername + ".jpg", CreationCollisionOption.GenerateUniqueName);
                    //////////////////
                }
            }
            catch (Exception)
            {

                return null;
            }
         
           
        }
        public static ToastNotification CreateFailureToast(string FileName)
        {
            string title = ResourceLoader.GetForViewIndependentUse().GetString("DownloadFailedToast");
            string name = FileName;
            return CreateToast(title, name);
        }
        public static ToastNotification CreateSuccessToast(string FileName, string path, bool isVideo)
        {
            string title = ResourceLoader.GetForViewIndependentUse().GetString("DownloadCompleteToast");
            string name = FileName;
            if (isVideo)
            {
                return CreateVideoToast(title, name, path);
            }
            else
            {
                return CreateToast(title, name, path);
            }

        }

        private static ToastNotification CreateVideoToast(string title, string name, string imagePath = "")
        {
            // Create xml template
            string xmlToastTemplate = string.Format("<toast launch=\"{2}\">" +
    "<visual>" +
    "  <binding template=\"ToastGeneric\">" +

         " <text> {0} </text>" +

              " <text> {1} </text>" +

                       " </binding>" +

                    "  </visual>" +
                   "  </toast> ", title, name, imagePath);
            var toastXml = new XmlDocument();

            toastXml.LoadXml(xmlToastTemplate);

            ToastNotification toastDlComplete = new ToastNotification(toastXml);


            return toastDlComplete;
        }


        private static ToastNotification CreateToast(string title, string name, string imagePath = "")
        {
            // Create xml template
            string xmlToastTemplate = string.Format("<toast launch=\"{3}\">" +
    "<visual>" +
    "  <binding template=\"ToastGeneric\">" +

         " <text> {0} </text>" +

              " <text> {1} </text>" +

                     " <image placement=\"inline\" src=\"{2}\" />" +

                       "  </binding>" +

                    "  </visual>" +
                   "  </toast> ", title, name, imagePath, imagePath);
            var toastXml = new XmlDocument();

            toastXml.LoadXml(xmlToastTemplate);

            // Create toast
            return new ToastNotification(toastXml);
        }

        private async Task LoadVideo(string videopath)
        {
            try
            {

                SoftwareBitmap softwareBitmap = null;

             
                StorageFile videofile = await StorageFile.GetFileFromPathAsync(videopath);
                var thumbnail = await GetThumbnailAsync(videofile);
                Windows.Storage.Streams.InMemoryRandomAccessStream randomAccessStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                await Windows.Storage.Streams.RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);

                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

                softwareBitmap = await decoder.GetSoftwareBitmapAsync();


                StorageFile outputFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(Path.GetFileNameWithoutExtension(videopath) + ".jpg");
                using (Windows.Storage.Streams.IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Create an encoder with the desired format
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                    // Set the software bitmap
                    encoder.SetSoftwareBitmap(softwareBitmap);

                    // Set additional encoding parameters, if needed
                    encoder.BitmapTransform.ScaledWidth = 128;
                    encoder.BitmapTransform.ScaledHeight = 128;

                    encoder.IsThumbnailGenerated = true;

                    try
                    {
                        await encoder.FlushAsync();
                        if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                        {
                            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        }

                        var source = new Windows.UI.Xaml.Media.Imaging.SoftwareBitmapSource();
                        await source.SetBitmapAsync(softwareBitmap);

                        // Set the source of the Image control
                        return;

                    }
                    catch (Exception err)
                    {
                        switch (err.HResult)
                        {
                            case unchecked((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
                                                             // If the encoder does not support writing a thumbnail, then try again
                                                             // but disable thumbnail generation.
                                encoder.IsThumbnailGenerated = false;
                                break;
                            default:
                                throw err;
                        }
                    }

                    if (encoder.IsThumbnailGenerated == false)
                    {
                        await encoder.FlushAsync();
                    }
                    //image.SetSource(fileStream);

                }
                return;
            }
            //    return image;
            //});
            //     return image;

            catch (Exception)
            {
                return;

            }
            finally
            {


                GC.Collect();
            }

        }
        private async Task<Windows.Storage.Streams.IInputStream> GetThumbnailAsync(StorageFile file)
        {
            var mediaClip = await Windows.Media.Editing.MediaClip.CreateFromFileAsync(file);
            var mediaComposition = new Windows.Media.Editing.MediaComposition();
            mediaComposition.Clips.Add(mediaClip);
            return await mediaComposition.GetThumbnailAsync(
                TimeSpan.Zero, 0, 0, Windows.Media.Editing.VideoFramePrecision.NearestFrame);
        }

        private void ShowNotification(string text, bool isTemorary = false)
        {
            var xDoc = new XDocument(

new XElement("toast", new XAttribute("launch", "samplenotification"),

new XElement("visual",

new XElement("binding", new XAttribute("template", "ToastGeneric"),

new XElement("text", "InstantGet Universal"),

new XElement("text", text)))));

            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();

            xmlDoc.LoadXml(xDoc.ToString());
            var toast = new ToastNotification(xmlDoc);
            if (isTemorary)
            {
                toast.ExpirationTime = DateTime.Now.AddSeconds(5);
            }
            var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notifi.Show(toast);
        }

    }
}
