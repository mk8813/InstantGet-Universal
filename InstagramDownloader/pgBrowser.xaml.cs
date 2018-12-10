using DbHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Imaging;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InstagramDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgBrowser : Page
    {
        DownloadOperation downloadOperation;
        ResourceLoader res = ResourceLoader.GetForCurrentView();
        private bool IsProfile=false;
        private LicenseInformation licenseInformation;
        private int WhatToDownload = -1;//0 pic - 1 video - 2 profile
        private bool IsFullSizeImageBought = false;


        public pgBrowser()
        {
            try
            {
                this.InitializeComponent();
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
                licenseInformation = CurrentApp.LicenseInformation;

                this.Loaded += PgBrowser_Loaded;
                brwInstagram.NavigationStarting += BrwInstagram_NavigationStarting;
                CheckBackAndForward();

              
            }
            catch (Exception)
            {

            
            }
      
        }

        private void BrwInstagram_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            try
            {
                brwInstagram.NavigationStarting -= BrwInstagram_NavigationStarting;
                args.Cancel = true;
                NavigateWithHeader(args.Uri);
            }
            catch (Exception)
            {

               
            }
          
        }
        private void NavigateWithHeader(Uri uri)
        {
            try
            {
                //Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; Microsoft; Lumia 950) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/14.14263
                string userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1";
                var requestMsg = new Windows.Web.Http.HttpRequestMessage(HttpMethod.Post, uri);
                requestMsg.Headers.Add("User-Agent", userAgent);

                brwInstagram.NavigateWithHttpRequestMessage(requestMsg);//start navigate

                brwInstagram.NavigationStarting += BrwInstagram_NavigationStarting;
            

            }
            catch (Exception)
            {

              
            }
           
        }

        private void TryNavigateWithMobileUserAgent(Uri uri)
        {
            try
            {
                NavigateWithHeader(uri);
            }
            catch (Exception)
            {
                brwInstagram.Navigate(uri);

            }
        }
        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            try
            {
                if (e!=null)
                {
                    var url = e.Parameter.ToString();
                    TryNavigateWithMobileUserAgent(new Uri(url));
                    txtBrwUrl.Text = url;
                }
            }
            catch (Exception)
            {

            }
        }
        private void PgBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (licenseInformation.ProductLicenses["FullSizeImageDownload"].IsActive)
                {
                   
                    IsFullSizeImageBought = true;
                   
                
                }
                if (brwInstagram.Source==null)
                {
                    TryNavigateWithMobileUserAgent(new Uri("https://www.instagram.com/accounts/login/"));

                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private  void brwInstagram_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            try
            {
                prgLoadingUrl.Visibility = Visibility.Collapsed;

                //  txtBrwUrl.Text = await brwInstagram.InvokeScriptAsync("eval", new string[] { "var newURL = window.location.protocol + '//' + window.location.host + '/" + window.location.pathname;" });//pictures

                txtBrwUrl.Text = brwInstagram.Source.ToString();

                CheckBackAndForward();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBrwUrl.IsEnabled)
                {
                    DataPackageView dataPackageView = Clipboard.GetContent();
                    if (dataPackageView.Contains(StandardDataFormats.Text))
                    {

                        string text = await dataPackageView.GetTextAsync();

                        txtBrwUrl.Text = text;
                    }
                }

            }
            catch (Exception)
            {


            }
        }

        private void btnNavigate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBrwUrl.Text.Length>=10)
                {
                    TryNavigateWithMobileUserAgent(new Uri(txtBrwUrl.Text));
                }
               
            }
            catch (Exception)
            {

               
            }
        }

        private void brwInstagram_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            try
            {
                prgLoadingUrl.Visibility = Visibility.Visible;
             
                txtBrwUrl.Text = brwInstagram.Source.ToString();
            }
            catch (Exception)
            {

               
            }  
        }

     void CheckBackAndForward()
        {
            try
            {
                //btnForward.IsEnabled = brwInstagram.CanGoForward;
                //btnBack.IsEnabled = brwInstagram.CanGoBack;
              
            }
            catch (Exception)
            {

              
            }
        }
       
        private void brwInstagram_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            try
            {
                prgLoadingUrl.Visibility = Visibility.Collapsed;
             
                txtBrwUrl.Text = brwInstagram.Source.ToString();
     
                CheckBackAndForward();

            }
            catch (Exception)
            {

               
            }
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                try
                {
                    Regex regPost = new Regex(@"((http:\/\/(instagr\.am\/p\/.*|instagram\.com\/p\/.*|www\.instagram\.com\/p\/.*))|(https:\/\/(www\.instagram\.com\/p\/.*))|(https:\/\/(instagram\.com\/p\/.*)))");
                    Regex regProfile = new Regex(@"((http:\/\/(instagr\.am/.*|instagram\.com/.*|www\.instagram\.com/.*))|(https:\/\/(www\.instagram\.com/.*))|(https:\/\/(instagram\.com/.*)))");
                    Regex regIGTv = new Regex(@"((http:\/\/(instagr\.am\/tv\/.*|instagram\.com\/tv\/.*|www\.instagram\.com\/tv\/.*))|(https:\/\/(www\.instagram\.com\/tv\/.*))|(https:\/\/(instagram\.com\/tv\/.*)))");

                    if (regPost.IsMatch(txtBrwUrl.Text) ||
                        regIGTv.IsMatch(txtBrwUrl.Text))
                    {
                        IsProfile = false;
                    }
                    else if (regProfile.IsMatch(txtBrwUrl.Text))
                    {
                        IsProfile = true;
                    }
                    else
                    {
                        ////////////////////////////
                        MessageDialog msg = new MessageDialog(res.GetString("InvalidInstagramUrl"));
                        await msg.ShowAsync();
                        return;
                    }

                    string htmlSrc = "";
                    switch (WhatToDownload)
                    {
                        case 0: // picture
                            try
                            {
                                htmlSrc = await brwInstagram.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName('KL4Bh')[0].innerHTML;" });//pictures

                            }
                            catch (Exception)
                            {
                                try
                                {
                                    htmlSrc = await brwInstagram.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName('eLAPa kPFhm')[0].innerHTML;" });//
                                }
                                catch (Exception)
                                {
                                    htmlSrc = "";

                                }


                            }

                            break;

                        case 1://video
                            try
                            {
                                htmlSrc = await brwInstagram.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName('_5wCQW')[0].innerHTML;" });//videos
                            }
                            catch (Exception)
                            {

                                htmlSrc = "";
                            }
                            break;

                        case 2:// profile pic

                            try
                            {
                                htmlSrc = await brwInstagram.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName('_b0acm')[0].innerHTML;" });//videos
                                IsProfile = true;
                            }
                            catch (Exception)
                            {

                                htmlSrc = "";
                            }
                            break;

                    }
                  

                    /////////////////////////saving item record
                    tbl_History curDownload = new tbl_History();
                    curDownload.DateInserted = DateTime.Now.ToString();


                    ///////////////////////////////////

                    if (!string.IsNullOrEmpty(htmlSrc))//check img - video src
                    {
                        StorageFolder savefolder;
                        StorageFile outputfile;

                        string imgSrc = "";
                       
                        bool isVideoLink = false;
                       
                       
                        imgSrc = FetchLinksFromSource(htmlSrc, out isVideoLink, IsProfile);//.ToString());
                        if (isVideoLink)
                        {
                            ////////////////////
                            var Settingfolder = await GetSaveLocations.GetVideosFolder();
                            if (Settingfolder != null)
                            {
                                savefolder = Settingfolder;
                            }
                            else
                            {
                                savefolder = KnownFolders.VideosLibrary;//.PickSingleFolderAsync();
                            }

                            outputfile = await savefolder.CreateFileAsync("InstantGet-" + DateTime.Now.Ticks.ToString() + ".mp4", CreationCollisionOption.GenerateUniqueName);
                            //////////////////
                        }
                        else
                        {
                            var Settingfolder = await GetSaveLocations.GetPicturesFolder();
                            if (Settingfolder != null)
                            {
                                savefolder = Settingfolder;
                            }
                            else
                            {   ////////////////////
                                savefolder = KnownFolders.PicturesLibrary;//.PickSingleFolderAsync();
                            }
                            outputfile = await savefolder.CreateFileAsync("InstantGet-" + DateTime.Now.Ticks.ToString() + ".jpg", CreationCollisionOption.GenerateUniqueName);
                            //////////////////
                        }

                        //////////////////////////

                        curDownload.SavePath = outputfile.Path;
                        curDownload.Type = isVideoLink ? "video" : "picture";
                        curDownload.Url = brwInstagram.Source.ToString();


                        ///////////////////

                        if (!String.IsNullOrEmpty(imgSrc))
                        {


                            ShowNotification(res.GetString("DownloadInProgress"), true);
                            bool isSuccess = await Download(imgSrc, outputfile, isVideoLink);


                            if (isSuccess)
                            {
                                if (isVideoLink)
                                {
                                    /////////////save video thumbnail

                                    await  LoadVideo(outputfile.Path);


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

                        }




                        ///////////////////////////


                    }
                    else//nothig found to download-- no pic no video and profile
                    {
                        //curDownload.IsDownloaded = "0";
                        ////////////////////////////
                        MessageDialog msg = new MessageDialog(res.GetString("NothingFound"));
                        await msg.ShowAsync();
                        return;

                    }
                    //////////////////////////////////////
                    //save record

                    using (var db = new dbHelperConnection())
                    {

                        db.tbl_History.Add(curDownload);
                        await db.SaveChangesAsync();
                       


                    }


                    /////////////////////////////////
                }
                catch (TaskCanceledException)
                {
                    var res = downloadOperation.ResultFile.DeleteAsync();
                    //dloperation = null;

                }
                catch (Exception ex)
                {
                    ShowNotification(ex.Message);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<bool> Download(string url, StorageFile file, bool isVideo = false)
        {
            try
            {

                CancellationTokenSource cancellationToken;
                Windows.Networking.BackgroundTransfer.BackgroundDownloader backgroundDownloader = new Windows.Networking.BackgroundTransfer.BackgroundDownloader();

                string filename = Path.GetFileName(url);


                backgroundDownloader.SuccessToastNotification = CreateSuccessToast(filename, file.Path, isVideo);
                backgroundDownloader.FailureToastNotification = CreateFailureToast(filename);


                Uri durl = new Uri(url);
                downloadOperation = backgroundDownloader.CreateDownload(durl, file);
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
                    GC.Collect();
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

            string regexMeta = "<img.+?src=[\"'](.+?)[\"'].*?>"; /* @" <meta\b[^>]*\bproperty=[""]og:image[""][^>]*\b+content[\s]?=[\s""']+(.*?)[""']+.*?";*/
            string videoRegex = "<video.+?src=[\"'](.+?)[\"'].*?>";  /* @"<meta\b[^>]*\bproperty=[""]og:video[""][^>]*\b+content[\s]?=[\s""']+(.*?)[""']+.*?";*/
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

       
        private void btnPicture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WhatToDownload = 0;
                btnDownload_Click(null, null);
            }
            catch (Exception)
            {

              
            }
        }

        private void btnVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WhatToDownload = 1;
                btnDownload_Click(null, null);
            }
            catch (Exception)
            {


            }

        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WhatToDownload = 2;
                btnDownload_Click(null, null);
            }
            catch (Exception)
            {


            }
        }

        private void brwInstagram_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            try
            {
                prgLoadingUrl.Visibility = Visibility.Collapsed;
                txtBrwUrl.Text = brwInstagram.Source.ToString();
                CheckBackAndForward();
            }
            catch (Exception)
            {

            }
        }

        private void brwInstagram_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                prgLoadingUrl.Visibility = Visibility.Collapsed;
                txtBrwUrl.Text = brwInstagram.Source.ToString();
                CheckBackAndForward();
            }
            catch (Exception)
            {


            }
        }

        private void brwInstagram_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            try
            {
                prgLoadingUrl.Visibility = Visibility.Collapsed;
                txtBrwUrl.Text = brwInstagram.Source.ToString();
                CheckBackAndForward();
            }
            catch (Exception)
            {


            }
        }

        private void brwInstagram_FrameNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            try
            {
                prgLoadingUrl.Visibility = Visibility.Visible;
                txtBrwUrl.Text = brwInstagram.Source.ToString();
            }
            catch (Exception)
            {


            }
        }

        private void brwInstagram_FrameContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            try
            {
                prgLoadingUrl.Visibility = Visibility.Visible;
                txtBrwUrl.Text = brwInstagram.Source.ToString();

            }
            catch (Exception)
            {


            }
        }

        private void brwInstagram_FrameDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            try
            {
                prgLoadingUrl.Visibility = Visibility.Collapsed;
                txtBrwUrl.Text = brwInstagram.Source.ToString();
                CheckBackAndForward();
            }
            catch (Exception)
            {


            }
        }

        private void brwInstagram_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                txtBrwUrl.Text = brwInstagram.Source.ToString();
          
                CheckBackAndForward();
            }
            catch (Exception)
            {


            }
        }

        private void brwInstagram_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            try
            {
                txtBrwUrl.Text = brwInstagram.Source.ToString();
                CheckBackAndForward();
            }
            catch (Exception)
            {


            }
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (brwInstagram.CanGoForward)
                {
                    brwInstagram.GoForward();
                }
            }
            catch (Exception)
            {

               
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (brwInstagram.CanGoBack)
                {
                    brwInstagram.GoBack();

                }
            }
            catch (Exception)
            {


            }

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //FrameworkElement senderElement = sender as FrameworkElement;
                //Windows.UI.Xaml.Controls.Primitives.FlyoutBase flyoutBase = Windows.UI.Xaml.Controls.Primitives.FlyoutBase.GetAttachedFlyout(senderElement);
                //flyoutBase.ShowAt(senderElement);

                MenuFlyout mf = (MenuFlyout)this.Resources["myFlyout"];

                mf.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom;
                mf.ShowAt(this.btnSave);
            }
            catch (Exception)
            {

                
            }
        }

      

       
        private void txtBrwUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                btnBack.Visibility = btnForward.Visibility =  Visibility.Collapsed;
            }
            catch (Exception)
            {


            }
        }

        private void txtBrwUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                btnBack.Visibility = btnForward.Visibility  = Visibility.Visible;
            }
            catch (Exception)
            {


            }
        }



   
        private void txtUrl_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                btnNavigate_Click(null, null);
                btnNavigate.Focus(FocusState.Programmatic);
            }
        }

        private async void btnResetCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.UI.Xaml.Controls.WebView.ClearTemporaryWebDataAsync();

                Windows.Web.Http.Filters.HttpBaseProtocolFilter baseFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
                foreach (var cookie in baseFilter.CookieManager.GetCookies(new Uri("https://www.instagram.com")))
                {
                    baseFilter.CookieManager.DeleteCookie(cookie);
                }
                TryNavigateWithMobileUserAgent(new Uri("https://www.instagram.com/accounts/login/"));
            }
            catch (Exception)
            {


            }
           

        }

        private async void btnPinBrowserToStartMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tileID = "browserTile";
                if (!SecondaryTile.Exists(tileID))
                {
                    string displayName = "InstantGet Browser";
                  
                    string arguments = "viewBrowser";

                    // Initialize the tile with required arguments
                    SecondaryTile tile = new SecondaryTile(
                        tileID,
                        displayName,
                        arguments,
                        new Uri("ms-appx:///Assets/Square150x150Logo.scale-400.png"),
                        TileSize.Default);
                    ///////////////wide tiles
                    tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.scale-400.png");
                    tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Square310x310Logo.scale-400.png");
                    tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Square71x71Logo.scale-400.png");
                    tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.scale-400.png");
                    tile.VisualElements.ShowNameOnSquare310x310Logo = tile.VisualElements.ShowNameOnWide310x150Logo = tile.VisualElements.ShowNameOnSquare150x150Logo = true;

                    ///////////
                    bool isPinned = await tile.RequestCreateAsync();
                    if (isPinned)
                    {
                        await new MessageDialog(res.GetString("PinnedBrowserTileMsg")).ShowAsync();
                    }
                }
                else
                {
                    SecondaryTile toBeDeleted = new SecondaryTile(tileID);
                    bool unpinned = await toBeDeleted.RequestDeleteAsync();
                    if (unpinned)
                    {
                        await new MessageDialog(res.GetString("UnPinBrowserTileMsg")).ShowAsync();
                    }

                }
            }
            catch (Exception)
            {

                
            }
           
        }

        ////////////////////////////////////////////////////////////////////
    }

}
