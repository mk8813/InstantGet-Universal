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

namespace UrlListenerComponent
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        DownloadOperation downloadOperation;
        BackgroundTaskDeferral _deferral = null;
        IBackgroundTaskInstance _taskInstance = null;
        private BackgroundTaskCancellationReason cancelReason = BackgroundTaskCancellationReason.Abort;
        private volatile bool cancelRequested = false;
        //object obj = new object();
        ResourceLoader res = ResourceLoader.GetForViewIndependentUse();
        private LicenseInformation licenseInformation;
        private bool IsFullSizeImageBought = false;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {

                taskInstance.Canceled += OnCanceled;

                if (cancelRequested == false)
                {


                    var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                    if (details != null)
                    {
                  
                        
                        string arguments = details.Argument;
                        var userInput = details.UserInput;
                        string inputurl = userInput["inputurl"].ToString(); // textbox value
                                                                            /////////////////////////////////////////////////////
                        Regex reg = new Regex(@"((http:\/\/(instagr\.am\/p\/.*|instagram\.com\/p\/.*|www\.instagram\.com\/p\/.*))|(https:\/\/(www\.instagram\.com\/p\/.*))|(https:\/\/(instagram\.com\/p\/.*)))");

                        Regex regProfile = new Regex(@"((http:\/\/(instagr\.am/.*|instagram\.com/.*|www\.instagram\.com/.*))|(https:\/\/(www\.instagram\.com/.*))|(https:\/\/(instagram\.com/.*)))");
                        /////////////////////////////////////////////////
                        switch (arguments)
                        {
                            ///////download
                            case "download":
                                {

                                    // Initialize the license info for testing.
                                    // UnComment the following line in the release version of your app.
                                    licenseInformation = CurrentApp.LicenseInformation;
                                    ///////////////////////
                                    if (licenseInformation.ProductLicenses["FullSizeImageDownload"].IsActive)
                                    {
                                        IsFullSizeImageBought = true;

                                    }
                                    else
                                    {
                                        IsFullSizeImageBought = false;

                                    }
                                    /////////////////////////////////////////////////////////////////////////////////


                                    if (reg.IsMatch(inputurl))
                                    {
                                        _deferral = taskInstance.GetDeferral();
                                        _taskInstance = taskInstance;
                                        string posturl = inputurl.Replace("http:", "https:");
                                        await imgDownloader(posturl, false);




                                    }
                                    else if (regProfile.IsMatch(inputurl))
                                    {
                                        _deferral = taskInstance.GetDeferral();
                                        _taskInstance = taskInstance;
                                        //_taskInstance = taskInstance;
                                        string posturl = inputurl.Replace("http:", "https:");
                                        await imgDownloader(posturl, true);

                                    }
                                    else
                                    {
                                        ShowNotification(res.GetString("InvalidInstagramUrl"), true);

                                        return;
                                    }
                                    break;
                                }
                            /////////////////////end downlad argument

                            //////postpone
                            case "postpone":
                                {
                                    if (reg.IsMatch(inputurl))
                                    {
                                        try
                                        {
                                            tbl_History postponeDownload = new tbl_History();
                                            postponeDownload.DateInserted = DateTime.Now.ToString();
                                            postponeDownload.IsDownloaded = "0";
                                            postponeDownload.SavePath = "";
                                            postponeDownload.Type = "";
                                            postponeDownload.Url = inputurl;

                                            using (var db = new dbHelperConnection())
                                            {

                                                db.tbl_History.Add(postponeDownload);
                                                int result = await db.SaveChangesAsync();
                                                //ShowNotification(cnt.ToString() + "  record saved");
                                                if (result > 0)
                                                {
                                                    ShowNotification(res.GetString("UrlPostponed"), true);
                                                }


                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            await ErrorLogger.ErrorLog.WriteError("Error in BackgroundTask SavePostpone_RunMethod" + ex.Message + ex.StackTrace);
                                        }

                                    }

                                    break;
                                }
                                //////////////////////end postpone
                        }




                    }

                }

            }
            catch (Exception)
            {

              
            }
            finally
            {
                if (_deferral != null)
                {
                    _deferral.Complete();
                }
            }


        }


        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            cancelRequested = true;
            cancelReason = reason;
            switch (reason)
            {
           
                case BackgroundTaskCancellationReason.IdleTask:
                    ShowNotification(res.GetString("appActionCenterCancelMsg"));
                    break;
                case BackgroundTaskCancellationReason.SystemPolicy:
                    ShowNotification(res.GetString("appActionCenterCancelMsg"));
                    break;
            }
            if (_deferral != null)
            {
                _deferral.Complete();
            }
        }

        private void ShowNotification(string text,bool isTemorary=false)
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


        public static void Sendnotidication()
        {
            var xmdock = CreateIntractiveToast();
            var toast = new ToastNotification(xmdock);
            toast.ExpirationTime = DateTime.Now.AddDays(1);
            toast.NotificationMirroring = NotificationMirroring.Allowed;
            toast.SuppressPopup = true;
           
            toast.Tag = "AgentToast";
            var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notifi.Show(toast);
            ////////////////////
        }
        public static Windows.Data.Xml.Dom.XmlDocument CreateIntractiveToast()
        {
            var xDoc = new XDocument(
               new XElement("toast", new XAttribute("launch", "interactivenotification"),
               new XElement("visual",
               new XElement("binding", new XAttribute("template", "ToastGeneric"),
               new XElement("text", ResourceLoader.GetForViewIndependentUse().GetString("PasteShareUrl"))
            )
            ),// actions  
            new XElement("actions",
            new XElement("input", new XAttribute("id", "inputurl"), new XAttribute("type", "text"), new XAttribute("placeHolderContent", "https://www.instagram.com/AbCde123"), new XAttribute("arguments", "actionUrl")),
            new XElement("action", new XAttribute("activationType", "background"), new XAttribute("imageUri", "Assets/postpone-96.png"), new XAttribute("content", ResourceLoader.GetForViewIndependentUse().GetString("PostponeButton")), new XAttribute("arguments", "postpone")),
            new XElement("action", new XAttribute("activationType", "background"), new XAttribute("imageUri", "Assets/Download-96.png"), new XAttribute("content", ResourceLoader.GetForViewIndependentUse().GetString("DownloadButton")), new XAttribute("arguments", "download")))

            ));

            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }

        private async Task<bool> Download(string url, StorageFile file,bool isVideo=false)
        {
            try
            {

                CancellationTokenSource cancellationToken;
                Windows.Networking.BackgroundTransfer.BackgroundDownloader backgroundDownloader = new Windows.Networking.BackgroundTransfer.BackgroundDownloader();

                string filename = Path.GetFileName(url);
              

                backgroundDownloader.SuccessToastNotification = CreateSuccessToast(filename, file.Path,isVideo);
                backgroundDownloader.FailureToastNotification = CreateFailureToast(filename);


                Uri durl = new Uri(url);
                downloadOperation = backgroundDownloader.CreateDownload(durl, file);

                downloadOperation.CostPolicy = BackgroundTransferCostPolicy.Default;///////////////new
                downloadOperation.Priority = BackgroundTransferPriority.High;////////////new

                Progress<DownloadOperation> progress = new Progress<DownloadOperation>(progressChanged);
                cancellationToken = new CancellationTokenSource();
                try
                {
                    //_deferral = _taskInstance.GetDeferral();
                    await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
                  //  _deferral.Complete();
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


        private void progressChanged(DownloadOperation downloadOperation)
        {


            double br = downloadOperation.Progress.BytesReceived;
            var result = br / downloadOperation.Progress.TotalBytesToReceive * 100;

            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    {
                       // SetDownloadStatusText("Downloading...");
                        break;
                    }
                case BackgroundTransferStatus.PausedByApplication:
                    {
                        ShowNotification(res.GetString("DownloadPaused"));
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        ShowNotification(res.GetString("PausedCostedNetwork"));
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        ShowNotification(res.GetString("PausedNoNetwork"));
                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        ShowNotification(res.GetString("DownloadError"));

                        break;
                    }
                case BackgroundTransferStatus.Completed:
                    {
             
                        break;
                    }

            }
            if (result >= 100)
            {
          
                downloadOperation = null;
        

            }
        }

        public string FetchLinksFromSource(string htmlSource, out bool isVideo, bool IsProfile)
        {

            List<string> links = new List<string>();

            string regexMeta = @"<meta\b[^>]*\bproperty=[""]og:image[""][^>]*\b+content[\s]?=[\s""']+(.*?)[""']+.*?";
            string videoRegex = @"<meta\b[^>]*\bproperty=[""]og:video[""][^>]*\b+content[\s]?=[\s""']+(.*?)[""']+.*?";
            Regex profileRegex = new Regex(@"s[0-9][0-9][0-9]x[0-9][0-9][0-9]");

            MatchCollection videomatches = Regex.Matches(htmlSource, videoRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (videomatches.Count != 0)
            {
                foreach (Match m in videomatches)
                {
                    string src = m.Groups[1].Value;
                    isVideo = true;
                    return src;
                }
            }
            else
            {
                MatchCollection matchesImgSrc = Regex.Matches(htmlSource, regexMeta, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (!IsProfile)
                {
                    //picture

                    foreach (Match m in matchesImgSrc)
                    {
                        string src = m.Groups[1].Value;
                        isVideo = false;
                        return src;
                    }
                }
                else
                {
                    //profile
                    foreach (Match m in matchesImgSrc)
                    {
                        string src = m.Groups[1].Value;
                        isVideo = false;

                        if (profileRegex.IsMatch(src))
                        {
                            src = profileRegex.Replace(src, "s");
                        }

                        return src;
                    }
                }


            }
            isVideo = false;
            return "";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="IsProfile"></param>
        private async Task imgDownloader(string url,bool IsProfile)
        {
            
            try
            {
                Uri durl = new Uri(url);
                JsonPostInfo jsonresult = null;

                using (var client = new HttpClient())
                {
                    Uri urlll = null;

                    if (!string.IsNullOrEmpty(durl.Query))
                    {
                        urlll = new Uri(durl.ToString()
                              .Remove(durl.ToString().IndexOf(durl.Query), durl.Query.ToString().Length)
                              .Insert(durl.ToString().IndexOf(durl.Query), "?__a=1"));
                    }
                    else
                    {
                        urlll = new Uri(durl.ToString()
                              .Insert(durl.ToString().Length, "?__a=1"));
                    }


                    HttpResponseMessage response = await client.GetAsync(urlll);
                    var json = await response.Content.ReadAsStringAsync();

                    jsonresult = JsonConvert.DeserializeObject<JsonPostInfo>(json);

                }


                //}



                ////////////////////////////////////////////////////////////////////
                //DownloadOperation dloperation;
                //CancellationTokenSource cancelTk;
                //BackgroundDownloader bgUrlDownloader = new BackgroundDownloader();


                //dloperation = bgUrlDownloader.CreateDownload(durl, file);
                //cancelTk = new CancellationTokenSource();
                try
                {

                    //await dloperation.StartAsync().AsTask(cancelTk.Token);

                    ///////////////////////////////////////////////////// saving item record
                    tbl_History curDownload = new tbl_History();
                    bool isSuccess = false;
                    curDownload.DateInserted = DateTime.Now.ToString();
                    //////////////////////////////////////////////////////
                    string postIdOrUsername = "";

                    if (jsonresult != null /*dloperation.Progress.Status == BackgroundTransferStatus.Completed*/)
                    {
                        StorageFolder savefolder;
                        StorageFile outputfile;
                        string imgSrc = "";
                        bool isVideoLink = false;
                        //    string htmlsrc = System.IO.File.ReadAllText(file.Path);
                        if (!IsProfile)
                        {
                            isVideoLink = jsonresult.Graphql.ShortcodeMedia.IsVideo;

                        }


                        if (!isVideoLink)
                        {
                            if (!IsProfile)//not profile requested
                            {
                                postIdOrUsername = jsonresult.Graphql.ShortcodeMedia.Shortcode;

                                if (IsFullSizeImageBought)//full hd
                                {
                                    imgSrc = jsonresult.Graphql.ShortcodeMedia.DisplayUrl; /* FetchLinksFromSource(htmlsrc, out isVideoLink,IsProfile);//.ToString());*/
                                }
                                else // sd image
                                {
                                    imgSrc = jsonresult.Graphql.ShortcodeMedia.DisplayResources[0].Src;
                                }

                            }// is profile picture
                            else
                            {
                                postIdOrUsername = jsonresult.User.Username;
                                if (IsFullSizeImageBought)
                                {
                                    imgSrc = GetProfilePictureUrl(jsonresult.User.ProfilePicUrlHd);
                                }
                                else
                                {
                                    imgSrc = jsonresult.User.ProfilePicUrlHd;
                                }

                            }


                        }
                        else // video link
                        {
                            postIdOrUsername = jsonresult.Graphql.ShortcodeMedia.Shortcode;

                            imgSrc = jsonresult.Graphql.ShortcodeMedia.VideoUrl;

                        }
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

                            outputfile = await savefolder.CreateFileAsync("InstantGet-" + postIdOrUsername + ".mp4", CreationCollisionOption.GenerateUniqueName);
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
                            outputfile = await savefolder.CreateFileAsync("InstantGet-" + postIdOrUsername + ".jpg", CreationCollisionOption.GenerateUniqueName);
                            //////////////////
                        }

                        //////////////////////////

                        curDownload.SavePath = outputfile.Path;
                        curDownload.Type = isVideoLink ? "video" : "picture";
                        curDownload.Url = durl.ToString();


                        ///////////////////

                        if (!String.IsNullOrEmpty(imgSrc))
                        {


                            ShowNotification(res.GetString("DownloadInProgress"), true);
                             isSuccess = await Download(imgSrc, outputfile, isVideoLink);


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
                            await ErrorLogger.ErrorLog.WriteError("Error in BackgroundTask_SaveToDbBlock_imgDownloader\r\n" + ex.Message + ex.StackTrace+ex.InnerException.Message+ex.InnerException.StackTrace);

                        }
                    }
                  

                  
                    /////////////////////////////////
                }
                catch (TaskCanceledException)
                {
                    var res = downloadOperation.ResultFile.DeleteAsync();
                   // dloperation = null;

                }
                catch (Exception ex)
                {
                    ShowNotification(ex.Message);
                }

            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message);
            }
            finally
            {
                GC.Collect();
               
                if (_deferral!=null)
                {

                    _deferral.Complete();
                }
              



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

        private  async Task<StorageFolder> GetPicturesFolder()
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

        public static ToastNotification CreateFailureToast(string FileName)
        {
            string title = ResourceLoader.GetForViewIndependentUse().GetString("DownloadFailedToast");
            string name = FileName;
            return CreateToast(title, name);
        }
        public static ToastNotification CreateSuccessToast(string FileName, string path,bool isVideo)
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
                   "  </toast> ", title, name, imagePath,imagePath);
            var toastXml = new XmlDocument();

            toastXml.LoadXml(xmlToastTemplate);
            //XmlDocument toastXml= ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            // Set elements
            //Windows.Data.Xml.Dom.XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            //IXmlNode element0 = stringElements[0];
            //element0.AppendChild(toastXml.CreateTextNode(title));

            //IXmlNode element1 = stringElements[1];
            //element1.AppendChild(toastXml.CreateTextNode(name));

            // Create toast
            return new ToastNotification(toastXml);
        }

        private async Task LoadVideo(string videopath)
        {
            try
            {

                SoftwareBitmap softwareBitmap = null;

                //StorageFile imagefile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/logo1.png"));
                StorageFile videofile = await StorageFile.GetFileFromPathAsync(videopath);
                var thumbnail = await GetThumbnailAsync(videofile);
                Windows.Storage.Streams.InMemoryRandomAccessStream randomAccessStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                await Windows.Storage.Streams.RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);

                //using ( await Windows.Storage.Streams.RandomAccessStream.CopyAsync(thumbnail, randomAccessStream))
                //{
                //    // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

                softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                //}

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
                    //encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
                    //encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
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


    }



}
