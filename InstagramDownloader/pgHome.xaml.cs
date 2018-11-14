using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.IO;
using Windows.UI.Popups;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using System.Threading;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using QuickType;
using Windows.Web.Http;
using Newtonsoft.Json;
using DbHelper;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InstagramDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgHome : Page
    {
        DownloadOperation downloadOperation;
        ResourceLoader res = ResourceLoader.GetForCurrentView();
        private bool IsAutomaticallyPaste = false;
        private bool IsRatingOpened = false;
        private bool IsDonateBoxViewed = false;
        private LicenseInformation licenseInformation;
        private bool IsFullSizeImageBought = false;
        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;

        public pgHome()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
         
            /////////////////////////
            // Uncomment the following line in the release version of your app.
            licenseInformation = CurrentApp.LicenseInformation;

            // Initialize the license info for testing.
            // Comment the following line in the release version of your app.
            //licenseInformation = CurrentAppSimulator.LicenseInformation;
            ///////////////////////
            this.Loaded += MainPage_Loaded;
            try
            {


                if (txtUrl.Text.Length >= 10)
                {
                    btnDownload.IsEnabled = true;
                }
                else
                {
                    btnDownload.IsEnabled = false;
                }



                object clipboard = AppSettings.Values["autoPaste"];
                if (clipboard != null)
                {
                    switch (clipboard.ToString())
                    {
                        case "1":
                            IsAutomaticallyPaste = true;
                            break;
                        case "0":
                            IsAutomaticallyPaste = false;
                            break;

                    }
                }
            }


            catch (Exception)
            {


            }
        }


        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ///////////check for fullsized image download
                if (licenseInformation.ProductLicenses["FullSizeImageDownload"].IsActive)
                {
                    iconBuyFullsizeimg.Visibility = Visibility.Collapsed;
                    IsFullSizeImageBought = true;
                    chkFullSizeImage.IsChecked = true;
                   
                    //AppSettings.Values["fullsizeimage"] = "0";
                }
                else
                {
                    iconBuyFullsizeimg.Visibility = Visibility.Visible;
                }

                /////////////////////////////////////////
                //////////////show statusbar
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {

                    var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                    if (statusBar != null)
                    {
                        if (this.RequestedTheme == ElementTheme.Dark)
                        {
                            statusBar.BackgroundColor = Windows.UI.Colors.Black;
                            statusBar.ForegroundColor = Windows.UI.Colors.White;
                            await statusBar.ShowAsync();
                        }
                        else if (this.RequestedTheme == ElementTheme.Light)
                        {
                            statusBar.BackgroundOpacity = 100;
                            statusBar.BackgroundColor = Windows.UI.Colors.White;
                            statusBar.ForegroundColor = Windows.UI.Colors.Black;
                            await statusBar.ShowAsync();
                        }

                    }
                }
                /////////////////// rate msg
                try
                {

                    int rnd = new Random().Next(1, 15);

                    if (rnd == 3)
                    {
                        if (!IsRatingOpened)
                        {
                            RateApp();
                        }

                    }
                    ///////////////////////////////
                }
                catch (Exception)
                {


                }


            }
            catch (Exception)
            {


            }

        }

        private async void RateApp()
        {
            try
            {
                ///////////////////// begin read setting
                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                object rated = AppSettings.Values["IsRated"];

                if (rated != null)
                {
                    if (rated.ToString() == "0")
                    {
                        ContentDialog deleteFileDialog = new ContentDialog()
                        {
                            Title = res.GetString("RatepopupTitle"),
                            Content = res.GetString("RatepopupContent"),
                            PrimaryButtonText = res.GetString("RatepopupPrimaryButtonText"),
                            SecondaryButtonText = res.GetString("RatepopupSecondaryButtonText")
                        };

                        IsRatingOpened = true;
                        ContentDialogResult result = await deleteFileDialog.ShowAsync();


                        if (result == ContentDialogResult.Primary)
                        {
                            try
                            {
                                AppSettings.Values["IsRated"] = "1";
                                await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?PFN={Package.Current.Id.FamilyName}"));
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                else
                {
                    AppSettings.Values["IsRated"] = "0";
                }


            }
            catch (Exception)
            {


            }
        }
        public string OuputFilePath = "";


        public async Task<bool> Download(string url, StorageFile file, bool isVideo = false)
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

                    await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
                    return true;
                }
                catch (TaskCanceledException)
                {
                    var res = downloadOperation.ResultFile.DeleteAsync();
                    downloadOperation = null;
                  
                    return false;
                }

            }
            catch (Exception)
            {

                return false;
            }


        }

        private async Task SetDownloadStatusText(string text)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtDownloadStatus.Text = text;

            }).AsTask().AsAsyncAction();

        }
        private async Task SetMsgDialogText(string text)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await new MessageDialog(text).ShowAsync();

            }).AsTask().AsAsyncAction();

        }
        private async Task SetProgressValue(double val, double max)
        {

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (val.ToString())
                {
                    case "-5":
                        prgDownload.IsIndeterminate = true;
                        break;
                    case "-10":
                        prgDownload.IsIndeterminate = false;
                        break;
                    case "-1":
                        prgDownload.Visibility = Visibility.Collapsed;
                        break;

                    default:

                        prgDownload.Maximum = max;
                        prgDownload.Value = val;
                        break;
                }
                //if (val == -5)
                //{
                //    prgDownload.IsIndeterminate = true;
                //}
                //else if (val==-10)
                //{
                //    prgDownload.IsIndeterminate = false;
                //}
                //if (val == -1)
                //{
                //    prgDownload.Visibility = Visibility.Collapsed;
                //}
                //else
                //{

                //    prgDownload.Maximum = max;
                //    prgDownload.Value = val;

                //}

            }).AsTask().AsAsyncAction();
        }

        private async void progressChanged(DownloadOperation downloadOperation)
        {

            double br = downloadOperation.Progress.BytesReceived;
            double TotalbytesReceived = downloadOperation.Progress.TotalBytesToReceive;
            var result = br / TotalbytesReceived * 100;
         
            await SetProgressValue(br, TotalbytesReceived);


            await SetDownloadStatusText(String.Format(res.GetString("Downloading"), ((int)(br / 1024)).ToString(), ((int)(TotalbytesReceived / 1024)).ToString()));

            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    {
                        // SetDownloadStatusText("Downloading...");
                        break;
                    }
                case BackgroundTransferStatus.PausedByApplication:
                    {
                        await SetDownloadStatusText(res.GetString("DownloadPaused"));
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        await SetDownloadStatusText(res.GetString("PausedCostedNetwork"));
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        await SetDownloadStatusText(res.GetString("PausedNoNetwork"));
                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        await SetDownloadStatusText(res.GetString("DownloadError"));

                        break;
                    }
                case BackgroundTransferStatus.Completed:
                    {
                        await SetDownloadStatusText(res.GetString("DownloadCompleted"));

                        break;
                    }

            }
            if (result >= 100)
            {
                await SetDownloadStatusText(res.GetString("DownloadCompleted"));
                downloadOperation = null;
                await SetProgressValue(-1, -1);



            }
        }


        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtUrl.Text.Length >= 10)
                {
                    btnDownload.IsEnabled = true;
                }
                else
                {
                    btnDownload.IsEnabled = false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

      public  dlgAlbumPost dlgLoadalbum = new dlgAlbumPost();

        private async Task<string> ShowAlbumListDialog(List<StickyNode> lstnode)
        {
            var tcs = new TaskCompletionSource<string>();
            var dialogTask = tcs.Task;

          
           await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

           
               btnChoosefromAlbum.Visibility = Visibility.Visible;
               //dlgAlbumPost loadalbum = new dlgAlbumPost();
               dlgLoadalbum.IsFullSized = IsFullSizeImageBought;
               dlgLoadalbum.LstAlbumPost = lstnode;
               var ress = await dlgLoadalbum.ShowAsync();
               if (ress == ContentDialogResult.Primary)
               {
                   isVideoLink = dlgLoadalbum.IsVideo;
                   postIdOrUsername = dlgLoadalbum.PostId;
                   tcs.SetResult(dlgLoadalbum.SelectedNode);
               }
               else
               {
                   tcs.SetResult("");
               };
               
            });

            var result = await dialogTask;
            
            return result;

          
        }

        bool isVideoLink = false;
        string postIdOrUsername = "";
        double _downloadedImageWidth = 0;
        double _downloadedImageHeight = 0;
        private async Task imgDownloader(string url, bool IsSelectedFromAlbum = false)
        {
            try
            {
                await SetProgressValue(-5, 0);//marquee progress
                await SetDownloadStatusText(res.GetString("DownloadInitialize"));
                isVideoLink = false;

                // StorageFolder folder = ApplicationData.Current.LocalCacheFolder;//.PickSingleFolderAsync();
                //  StorageFile file = await folder.CreateFileAsync(DateTime.Now.Ticks.ToString() + ".txt", CreationCollisionOption.GenerateUniqueName);
                Uri durl = new Uri(url);
                JsonPostInfo jsonresult = null;
                ///////////////////////////////////////////////////// saving item record
                tbl_History curDownload = new tbl_History();
                bool isSuccess = false;
                curDownload.DateInserted = DateTime.Now.ToString();
                //////////////////////////////////////////////////////
                StorageFolder savefolder;
                StorageFile outputfile;
                string imgSrc = "";


                if (!IsSelectedFromAlbum)
                {
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

                    if (jsonresult != null /*dloperation.Progress.Status == BackgroundTransferStatus.Completed*/)
                    {
                        /////////////////get image size for viewing
                        _downloadedImageHeight = jsonresult.Graphql.ShortcodeMedia.Dimensions.Height;
                        _downloadedImageWidth = jsonresult.Graphql.ShortcodeMedia.Dimensions.Width;
                        /////////////////////
                        if (jsonresult.Graphql.ShortcodeMedia.Typename == Instagram__Typename.GraphImage.ToString())//single image post
                        {

                            postIdOrUsername = jsonresult.Graphql.ShortcodeMedia.Shortcode;

                            if (IsFullSizeImageBought)//full hd
                            {
                                imgSrc = jsonresult.Graphql.ShortcodeMedia.DisplayResources.LastOrDefault().Src;//Displayurl /* FetchLinksFromSource(htmlsrc, out isVideoLink,IsProfile);//.ToString());*/
                            }
                            else // sd image
                            {
                                imgSrc = jsonresult.Graphql.ShortcodeMedia.DisplayResources.FirstOrDefault().Src;//[0]
                            }
                        }
                        else if (jsonresult.Graphql.ShortcodeMedia.Typename == Instagram__Typename.GraphVideo.ToString())//video post
                        {
                            postIdOrUsername = jsonresult.Graphql.ShortcodeMedia.Shortcode;
                            imgSrc = jsonresult.Graphql.ShortcodeMedia.VideoUrl;
                            isVideoLink = jsonresult.Graphql.ShortcodeMedia.IsVideo;
                            
                        }
                        else if (jsonresult.Graphql.ShortcodeMedia.Typename == Instagram__Typename.GraphSidecar.ToString())//album  post
                        {
                            //to do
                            if (jsonresult.Graphql.ShortcodeMedia.EdgeSidecarToChildren != null)
                            {
                                List<StickyNode> lstNodes = new List<StickyNode>();
                                foreach (var item in jsonresult.Graphql.ShortcodeMedia.EdgeSidecarToChildren.Edges)
                                {
                                    lstNodes.Add(item.Node);
                                }

                                imgSrc = await ShowAlbumListDialog(lstNodes);


                            }
                        }

                    }
                    else// json result is null
                    {
                        curDownload.IsDownloaded = "0";
                        await SetDownloadStatusText(res.GetString("FailedToRetrieveUrl"));
                        await SetMsgDialogText(res.GetString("FailedToRetrieveUrl"));

                    }

                }
                else// from album dialog
                {
                    imgSrc = dlgLoadalbum.SelectedNode;
                    prgDownload.Visibility = Visibility.Visible;
                }



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

                    outputfile = await savefolder.CreateFileAsync("InstantGet-" + postIdOrUsername + ".mp4", CreationCollisionOption.GenerateUniqueName);
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
                        //SetProgressValue(-10, 0);
                        isSuccess = await Download(imgSrc, outputfile, isVideoLink);
                        ////////////////////////////////////////
                        if (isVideoLink)
                        {
                            if (isSuccess)
                            {
                                curDownload.IsDownloaded = "1";


                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                                {
                                    try
                                    {

                                        imgDownloaded.Visibility = Visibility.Collapsed;
                                        plyVideo.Visibility = Visibility.Visible;
                                        plyVideo.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(outputfile);
                                        btnCommandViewFile.Tag = outputfile.Path;
                                        btnCommandViewFile.Visibility = Visibility.Visible;
                                        btnCommandShareFile.Visibility = Visibility.Collapsed;
                                        prgDownload.Visibility = Visibility.Collapsed;



                                        /////////////save video thumbnail
                                        ThumbnailVideos tb = new ThumbnailVideos();

                                        await tb.LoadVideo(outputfile.Path);


                                    }
                                    catch (Exception)
                                    {


                                    }


                                });



                            }
                            else
                            {
                                curDownload.IsDownloaded = "0";
                                await SetDownloadStatusText(res.GetString("ErrorDownloadVideo"));
                                await SetMsgDialogText(res.GetString("ErrorDownloadVideo"));

                             
                            }
                        }
                        else
                        {

                            if (isSuccess)
                            {
                                curDownload.IsDownloaded = "1";

                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                                {
                                    try
                                    {
                                        var outfileStorage = await StorageFile.GetFileFromPathAsync(outputfile.Path);
                                        var stream = await outfileStorage.OpenReadAsync();
                                        var imageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                                        await imageSource.SetSourceAsync(stream);

                                        //isVideo = false;

                                        //imgDownloaded.Height = _downloadedImageHeight;
                                        //imgDownloaded.Width = _downloadedImageWidth;

                                        imgDownloaded.Source = imageSource;
                                        imgDownloaded.Visibility = Visibility.Visible;
                                        plyVideo.Visibility = Visibility.Collapsed;


                                        btnCommandViewFile.Tag = outputfile.Path;
                                        btnCommandViewFile.Visibility = Visibility.Visible;
                                        btnCommandShareFile.Visibility = Visibility.Visible;
                                        prgDownload.Visibility = Visibility.Collapsed;

                                       
                                        //MessageDialog msg = new MessageDialog(res.GetString("PictureSuccessfullySaved"));
                                        //var result = msg.ShowAsync();


                                    }
                                    catch (Exception)
                                    {


                                    }


                                });




                            }
                            else
                            {
                                curDownload.IsDownloaded = "0";
                                await SetDownloadStatusText(res.GetString("ErrorDownloadPicture"));
                                await SetMsgDialogText(res.GetString("ErrorDownloadPicture"));


                            }
                        }



                    }
                    else
                    {
                        //imgsrc is null
                        curDownload.IsDownloaded = "0";
                        await SetDownloadStatusText(res.GetString("TargetFileNotFound"));
                        await SetMsgDialogText(res.GetString("TargetFileNotFound"));

                    }



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



                //}

                //catch (Exception ex)
                //{

                //        await SetDownloadStatusText(ex.Message);
                //        await SetMsgDialogText(ex.Message);
                //        await ErrorLogger.ErrorLog.WriteError("Error in pgHome_imgDownloader_tryBlockTwo\r\n" + ex.ToString());


                //}

            }
            catch (TaskCanceledException)
            {
                var res = downloadOperation.ResultFile.DeleteAsync();
                //dloperation = null;

            }
            catch (Exception ex)
            {

                await SetDownloadStatusText(res.GetString("ErrorWhenInitialize"));
                await SetMsgDialogText(ex.Message);
                await ErrorLogger.ErrorLog.WriteError("Error in pgHome_imgDownloader_tryBlockOne_Initialize\r\n" + ex.ToString());


            }
            finally
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {

                    txtUrl.Text = "";
                    prgDownload.Visibility = Visibility.Collapsed;
                    btnDownload.IsEnabled = chkFullSizeImage.IsEnabled = btnPickUrlFromList.IsEnabled = txtUrl.IsEnabled = true;

                    txtstatusFadeAnimation.Storyboard.Stop();
                    //////////////////////////
                    /////////////////////begin showing donate 
                    if (!IsDonateBoxViewed)
                    {
                        int rnd = new Random().Next(1, 20);
                        if (rnd <= 2)//
                        {
                            Donate();
                        }

                    }

                    /////////////////////////
                    /////////////////////
                });


                GC.Collect();

            }

        }

        private async void Donate()
        {
            try
            {
                ///////////////////// begin read setting
                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                object donated = AppSettings.Values["IsDonated"];
                if (!licenseInformation.ProductLicenses["FullSizeImageDownload"].IsActive) //not bought
                {
                    if (donated != null)
                    {
                        if (donated.ToString() == "0")
                        {
                            ContentDialog donateDialog = new ContentDialog()
                            {
                                Title = res.GetString("DonatepopupTitle"),
                                Content = res.GetString("DonatepopupContent"),
                                PrimaryButtonText = res.GetString("DonatepopupPrimaryButtonText"),
                                SecondaryButtonText = res.GetString("DonatepopupSecondaryButtonText")
                            };

                            IsRatingOpened = true;
                            ContentDialogResult result = await donateDialog.ShowAsync();


                            if (result == ContentDialogResult.Primary)
                            {
                                try
                                {
                                    AppSettings.Values["IsDonated"] = "1";
                                    await new contentDonateList().ShowAsync();
                                    //  await Windows.System.Launcher.LaunchUriAsync(new Uri(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SR6U393C95FT6"));
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        AppSettings.Values["IsDonated"] = "0";
                    }
                }
                 


            }
            catch (Exception)
            {


            }
        }
        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Regex regPost = new Regex(@"((http:\/\/(instagr\.am\/p\/.*|instagram\.com\/p\/.*|www\.instagram\.com\/p\/.*))|(https:\/\/(www\.instagram\.com\/p\/.*))|(https:\/\/(instagram\.com\/p\/.*)))");
                Regex regProfile = new Regex(@"((http:\/\/(instagr\.am/.*|instagram\.com/.*|www\.instagram\.com/.*))|(https:\/\/(www\.instagram\.com/.*))|(https:\/\/(instagram\.com/.*)))");

                txtstatusFadeAnimation.Storyboard.Begin();
                if (regPost.IsMatch(txtUrl.Text))
                {
                    imgDownloaded.Source = null;
                    imgDownloaded.Visibility = Visibility.Collapsed;
                    btnCommandViewFile.Visibility = Visibility.Collapsed;
                    btnCommandShareFile.Visibility = Visibility.Collapsed;
                    plyVideo.Visibility = Visibility.Collapsed;
                    btnChoosefromAlbum.Visibility = Visibility.Collapsed;
                    prgDownload.Value = 0;
                    prgDownload.Visibility = Visibility.Visible;
                    btnDownload.IsEnabled=chkFullSizeImage.IsEnabled = btnPickUrlFromList.IsEnabled = txtUrl.IsEnabled = false;

                    Uri posturl = new Uri(txtUrl.Text.Replace("http:", "https:"));


                  
                    await Task.Run(() => imgDownloader(posturl.ToString()));

                    txtUrl.Text = "";
                }
                else if (regProfile.IsMatch(txtUrl.Text))
                {
                    await SetDownloadStatusText(res.GetString("ErrorProfilePicture"));
                    await new MessageDialog(res.GetString("ErrorProfilePicture")).ShowAsync();
                    //imgDownloaded.Source = null;
                    //imgDownloaded.Visibility = Visibility.Collapsed;
                    //btnCommandViewFile.Visibility = Visibility.Collapsed;
                    //btnCommandShareFile.Visibility = Visibility.Collapsed;
                    //plyVideo.Visibility = Visibility.Collapsed;

                    //prgDownload.Value = 0;
                    //prgDownload.Visibility = Visibility.Visible;

                    //btnDownload.IsEnabled = chkFullSizeImage.IsEnabled = btnPickUrlFromList.IsEnabled = txtUrl.IsEnabled = false;

                    //await SetProgressValue(-5, 0);//marquee progress

                    //string posturl = txtUrl.Text.Replace("http:", "https:");
                    //await Task.Run(() => imgDownloader(posturl, true));//.AsAsyncAction();

                    //txtUrl.Text = "";
                }

                else
                {
                    txtUrl.Focus(FocusState.Programmatic);
                    txtUrl.SelectAll();
                    await  new MessageDialog(res.GetString("InvalidInstagramUrl")).ShowAsync();
                
                }

            }
            catch (Exception ex)
            {
                await new MessageDialog(res.GetString("UnhandledException")).ShowAsync();
                await ErrorLogger.ErrorLog.WriteError("Error in pgHome_btnDownload_Click\r\n" + ex.ToString());
                          
            }

        }

        public static ToastNotification CreateFailureToast(string FileName)
        {
            string title = ResourceLoader.GetForViewIndependentUse().GetString("DownloadFailedToast");
            string name = FileName;
            return CreateToast(title, name);
        }
        public static ToastNotification CreateSuccessToast(string FileName, string path, bool Isvideo = false)
        {
            string title = ResourceLoader.GetForViewIndependentUse().GetString("DownloadCompleteToast");
            string name = FileName;
            if (Isvideo)
            {
                return CreateVideoToast(title, name, path);
            }
            else
            {
                return CreateToast(title, name, path);
            }

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

            ToastNotification toastDlComplete = new ToastNotification(toastXml);


            return toastDlComplete;
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

        private async void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtUrl.IsEnabled)
                {
                    DataPackageView dataPackageView = Clipboard.GetContent();
                    if (dataPackageView.Contains(StandardDataFormats.Text))
                    {

                        string text = await dataPackageView.GetTextAsync();

                        txtUrl.Text = text;
                    }
                }

            }
            catch (Exception)
            {


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
        public string FetchLinksFromSource(string htmlSource, out bool isVideo, bool IsProfile = false)
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




        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }



        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(pgHelp));
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void btnViewFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync();
                var res = await Windows.System.Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(btnCommandViewFile.Tag.ToString()));

            }
            catch (Exception ex)
            {

                MessageDialog msg = new MessageDialog(res.GetString("UnhandledException") + ex.Message);

                var result = msg.ShowAsync();
            }

        }

        private void btnQuickShare_Click(object sender, RoutedEventArgs e)
        {
            RegisterForShare();
        }
        private void RegisterForShare()
        {
            try
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += new Windows.Foundation.TypedEventHandler<DataTransferManager,
                    DataRequestedEventArgs>(this.ShareImageHandler);
                DataTransferManager.ShowShareUI();
            }
            catch (Exception ex)
            {

                MessageDialog msg = new MessageDialog(res.GetString("UnhandledException") + ex.Message);

                var result = msg.ShowAsync();
            }

        }

        private async void ShareImageHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            try
            {
                DataRequest request = e.Request;
                request.Data.Properties.Title = res.GetString("ShareUITitle");
                // request.Data.Properties.Description = "Demonstrates how to share an image.";

                // Because we are making async calls in the DataRequested event handler,
                //  we need to get the deferral first.
                DataRequestDeferral deferral = request.GetDeferral();

                // Make sure we always call Complete on the deferral.
                try
                {
                    string newUri = Uri.UnescapeDataString(btnCommandViewFile.Tag.ToString());


                    StorageFile thumbnailFile = await StorageFile.GetFileFromPathAsync(newUri);
                    request.Data.Properties.Thumbnail = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(thumbnailFile);
                    StorageFile imageFile = await StorageFile.GetFileFromPathAsync(newUri);

                    request.Data.SetBitmap(Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(imageFile));

                }
                finally
                {
                    deferral.Complete();
                }
            }
            catch (Exception)
            {


            }

        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(pgHistory));
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void txtUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtUrl.Text.Length == 0)
                {
                    if (IsAutomaticallyPaste)
                    {
                        DataPackageView dataPackageView = Clipboard.GetContent();
                        if (dataPackageView.Contains(StandardDataFormats.Text))
                        {
                            string clipboradtext = await dataPackageView.GetTextAsync();

                            Regex regPost = new Regex(@"((http:\/\/(instagr\.am\/p\/.*|instagram\.com\/p\/.*|www\.instagram\.com\/p\/.*))|(https:\/\/(www\.instagram\.com\/p\/.*))|(https:\/\/(instagram\.com\/p\/.*)))");
                            Regex regProfile = new Regex(@"((http:\/\/(instagr\.am/.*|instagram\.com/.*|www\.instagram\.com/.*))|(https:\/\/(www\.instagram\.com/.*))|(https:\/\/(instagram\.com/.*)))");

                            if (regPost.IsMatch(clipboradtext) || regProfile.IsMatch(clipboradtext))
                            {
                                txtUrl.Text = clipboradtext;
                                txtUrl.SelectAll();
                            }

                        }
                    }
                }

            }
            catch (Exception)
            {


            }
        }



        private void txtUrl_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                btnDownload_Click(null, null);
            }
        }

        private async void btnPickUrlFromList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                contentPostponeList cntpostpone = new contentPostponeList();
                cntpostpone.LstAlbumPost = null;
                //if (await cntpostpone.ShowAsync() == ContentDialogResult.Primary)
                await cntpostpone.ShowAsync();
                if (!cntpostpone.IsBrowseselected)
                {
                    txtUrl.Text = cntpostpone.SelectedUrl;
                }
                else
                {
                    (this.Frame)?.Navigate(typeof(pgBrowser), cntpostpone.SelectedUrl);
                }

            }
            catch (Exception)
            {


            }
        }


        private async void chkFullSizeImage_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (licenseInformation.ProductLicenses["FullSizeImageDownload"].IsActive) //bought
                {
                    IsFullSizeImageBought = true;

                }
                else //not bought yet
                {
                    ShowProgressBarStatus(true);

                    try
                    {
                        // The customer doesn't own this feature, so
                        // show the purchase dialog.
                        var result = await CurrentApp.RequestProductPurchaseAsync("FullSizeImageDownload");
                        if (result.Status == ProductPurchaseStatus.Succeeded || result.Status == ProductPurchaseStatus.AlreadyPurchased)
                        {
                            IsFullSizeImageBought = true;
                            chkFullSizeImage.IsChecked = true;
                            AppSettings.Values["fullsizeimage"] = result.TransactionId.ToString();
                            await new MessageDialog("Thank you! Now you can download original images resolution.").ShowAsync();// Transaction ID: " + result.TransactionId.ToString()).ShowAsync();
                        }
                        else
                        {
                            IsFullSizeImageBought = false;
                            chkFullSizeImage.IsChecked = false;
                            AppSettings.Values["fullsizeimage"] = "0";

                        }

                        //Check the license state to determine if the in-app purchase was successful.
                    }
                    catch (Exception ex)
                    {
                        IsFullSizeImageBought = false;
                        chkFullSizeImage.IsChecked = false;
                        AppSettings.Values["fullsizeimage"] = "0";
                        await new MessageDialog(ex.Message).ShowAsync();
                    }

                    ShowProgressBarStatus(false);
                }
            }
            catch (Exception)
            {

               
            }
          
        

        }

        async void ShowProgressBarStatus(bool show)
        {
            try
            {
               
                btnDownload.IsEnabled = btnPickUrlFromList.IsEnabled = btnPaste.IsEnabled = txtUrl.IsEnabled = !show;

                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    var statusBar = StatusBar.GetForCurrentView();
                    if (show)
                    {
                       
                        statusBar.BackgroundOpacity = 0;
                        statusBar.ProgressIndicator.ProgressValue = null;
                        await statusBar.ProgressIndicator.ShowAsync();

                    }
                    else
                    {
                     
                        statusBar.BackgroundOpacity = 100;
                        await statusBar.ProgressIndicator.HideAsync();
                        statusBar.ProgressIndicator.ProgressValue = null;
                    }
                }
            }
            catch (Exception)
            {

              
            }
           

        }

        private void chkFullSizeImage_Unchecked(object sender, RoutedEventArgs e)
        {
            if (licenseInformation.ProductLicenses["FullSizeImageDownload"].IsActive) //bought
            {
                IsFullSizeImageBought = false;
               
            }
        }

        private async void btnChoosefromAlbum_Click(object sender, RoutedEventArgs e)
        {
            var ress =await dlgLoadalbum.ShowAsync();
            if (ress==ContentDialogResult.Primary)
            {
                string posturl = "http://instagram.com/p/" + dlgLoadalbum.PostId;
              await  imgDownloader(posturl, true);
            }
        }

        private void MyCommandBar_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                MyCommandBar.IsOpen = false;
            }
            catch (Exception)
            {

             
            }
        }

     

       
    }
}
