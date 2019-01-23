using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using Windows.ApplicationModel.DataTransfer;
using System.Text.RegularExpressions;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using QuickType;
using Windows.UI.ViewManagement;
using BackgroundDownloadHandler;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InstagramDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgHome : Page
    {
        
        ResourceLoader res = ResourceLoader.GetForCurrentView();
       
        private bool IsRatingOpened = false;
        private bool IsDonateBoxViewed = false;
        private LicenseInformation licenseInformation;
        private bool IsFullSizeImageBought = false;
        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
        private bool IsAutomaticallyPaste = false;
        bool isVideoLink = false;
        string postIdOrUsername = "";
        double _downloadedImageWidth = 0;
        double _downloadedImageHeight = 0;

        public dlgAlbumPost dlgLoadAlbum;

        public pgHome()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
           
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


                btnDownload.IsEnabled = txtUrl.Text.Length >= 10;

                object clipboard = AppSettings.Values["autoPaste"];
                if (clipboard == null) return;
                if (clipboard.ToString() == "1")
                    IsAutomaticallyPaste = true;
                else if (clipboard.ToString() == "0") IsAutomaticallyPaste = false;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            try
            {
                if (!isVideoLink)
                {
                    if (Window.Current.Bounds.Height < _downloadedImageHeight)
                    {
                        imgDownloaded.Height = Window.Current.Bounds.Height - 50;
                    }
                    else
                    {
                        imgDownloaded.Height = _downloadedImageHeight - 50;
                    }
                    if (Window.Current.Bounds.Width < _downloadedImageWidth)
                    {
                        imgDownloaded.Width = Window.Current.Bounds.Width - 50;
                    }
                    else
                    {
                        imgDownloaded.Width = _downloadedImageWidth - 50;
                    }
                }
                else
                {
                    if (Window.Current.Bounds.Height < _downloadedImageHeight)
                    {
                       plyVideo.Height= plyVideo.MinHeight = Window.Current.Bounds.Height - 50;
                    }
                    else
                    {
                        plyVideo.Height = plyVideo.MaxHeight = _downloadedImageHeight - 50;
                    }
                    if (Window.Current.Bounds.Width < _downloadedImageWidth)
                    {
                      plyVideo.Width=  plyVideo.MinWidth = Window.Current.Bounds.Width - 50;
                    }
                    else
                    {
                        plyVideo.Width = plyVideo.MaxWidth = _downloadedImageWidth - 50;
                    }
                }
               

               

            }
            catch (Exception)
            {


            }
        }

        private  void MainPage_Loaded(object sender, RoutedEventArgs e)
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
                //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                //{

                //    var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                //    if (statusBar != null)
                //    {
                //        if (this.RequestedTheme == ElementTheme.Dark)
                //        {
                //            statusBar.BackgroundColor = Windows.UI.Colors.Black;
                //            statusBar.ForegroundColor = Windows.UI.Colors.White;
                //            await statusBar.ShowAsync();
                //        }
                //        else if (this.RequestedTheme == ElementTheme.Light)
                //        {
                //            statusBar.BackgroundOpacity = 100;
                //            statusBar.BackgroundColor = Windows.UI.Colors.White;
                //            statusBar.ForegroundColor = Windows.UI.Colors.Black;
                //            await statusBar.ShowAsync();
                //        }

                //    }
                //}
                /////////////////// rate msg
                try
                {

                    int rnd = new Random().Next(1, 15);

                    if (rnd != 3) return;
                    if (!IsRatingOpened)
                    {
                        RateApp();
                    }
                    ///////////////////////////////
                }
                catch
                {
                    // ignored
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

        private void SetDownloadStatusText(string text)
        {

              txtDownloadStatus.Text = text;

        }
        private async Task SetMsgDialogText(string text)
        {

            await new MessageDialog(text).ShowAsync();

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

        private void progressChanged(DownloadOperation downloadOperation)
        {

            double br = downloadOperation.Progress.BytesReceived;
            double totalbytesReceived = downloadOperation.Progress.TotalBytesToReceive;
            var result = br / totalbytesReceived * 100;

            // await SetProgressValue(br, TotalbytesReceived);
            prgDownload.Maximum = totalbytesReceived;
            prgDownload.Value = br;

            //await SetDownloadStatusText(String.Format(res.GetString("Downloading"), ((int)(br / 1024)).ToString(), ((int)(TotalbytesReceived / 1024)).ToString()));
            txtDownloadStatus.Text = string.Format(res.GetString("Downloading"), ((int)(br / 1024)).ToString(), ((int)(totalbytesReceived / 1024)).ToString());

            switch (downloadOperation.Progress.Status)
            {
            
                case BackgroundTransferStatus.PausedByApplication:
                    {
                     
                        txtDownloadStatus.Text = res.GetString("DownloadPaused");
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                     
                        txtDownloadStatus.Text = res.GetString("PausedCostedNetwork");
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                   
                        txtDownloadStatus.Text = res.GetString("PausedNoNetwork");
                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                     
                        txtDownloadStatus.Text = res.GetString("DownloadError");
                        break;
                    }
                case BackgroundTransferStatus.Completed:
                    {
                    
                        txtDownloadStatus.Text = res.GetString("DownloadCompleted");
                        break;
                    }

            }
            if (result >= 100)
            {
               // await SetDownloadStatusText(res.GetString("DownloadCompleted"));
                txtDownloadStatus.Text = res.GetString("DownloadCompleted");
                flyoutDownloadProgress.Visibility=  prgDownload.Visibility =prgIntermediate.Visibility= Visibility.Collapsed;



            }
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

  

      
        private async Task Downloader(string url, bool isSelectedFromAlbum = false)
        {
            try
            {
                string strDurl=url;

                ///////////////////
                if (!string.IsNullOrEmpty(strDurl))
                {
                    //SetProgressValue(-10, 0);
                    BackgroundDownloaderManager bgDownloader = new BackgroundDownloaderManager();
                    Progress<DownloadOperation> progress = new Progress<DownloadOperation>(progressChanged);
                    if (isSelectedFromAlbum)//user selected a album post item
                    {
                        bgDownloader.PostDetailsForCurrentDownload = dlgLoadAlbum.SelectedNode;
                    }
                    var isSuccess = await bgDownloader.DownloadPost(strDurl, progress);
                    PostDetails curPostDetails = bgDownloader.PostDetailsForCurrentDownload;
                    ////////////////////////////////////////
                    isVideoLink = curPostDetails.IsVideo;
                    _downloadedImageHeight = curPostDetails.PostDimensions.Height;
                    _downloadedImageWidth = curPostDetails.PostDimensions.Width;
                    if (curPostDetails.isAlbumPost)
                    {
                        dlgLoadAlbum = new dlgAlbumPost(curPostDetails.AlbumNodes);
                        btnChoosefromAlbum.Visibility = Visibility.Visible;
                    }
                    ////////////////////////////////////

                    if (isSuccess)
                    {
                        //curDownload.IsDownloaded = "1";
                        if (isVideoLink)
                        {
                            try
                            {

                                imgDownloaded.Visibility = Visibility.Collapsed;
                                plyVideo.Visibility = Visibility.Visible;


                                StorageFile videoFile = await StorageFile.GetFileFromPathAsync(bgDownloader.DownloadedFilePath);

                                plyVideo.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(videoFile);
                                // try to set video player size by current window size
                                CoreWindow_SizeChanged(null, null);

                                btnCommandViewFile.Tag = bgDownloader.DownloadedFilePath;// outputfile.Path;
                                btnCommandViewFile.Visibility = Visibility.Visible;
                                btnCommandShareFile.Visibility = Visibility.Collapsed;
                                prgDownload.Visibility = Visibility.Collapsed;



                                /////////////save video thumbnail
                                //ThumbnailVideos tb = new ThumbnailVideos();

                                //await tb.LoadVideo(outputfile.Path);


                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        else // picture
                        {
                            try
                            {
                                var outfileStorage = await StorageFile.GetFileFromPathAsync(bgDownloader.DownloadedFilePath);
                                var stream = await outfileStorage.OpenReadAsync();
                                var imageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                                await imageSource.SetSourceAsync(stream);



                                imgDownloaded.Source = imageSource;
                                imgDownloaded.Visibility = Visibility.Visible;
                                plyVideo.Visibility = Visibility.Collapsed;


                                // try to set image size by current window size
                                CoreWindow_SizeChanged(null, null);


                                btnCommandViewFile.Tag = bgDownloader.DownloadedFilePath;
                                btnCommandViewFile.Visibility = Visibility.Visible;
                                btnCommandShareFile.Visibility = Visibility.Visible;
                                prgDownload.Visibility = Visibility.Collapsed;


                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }




                    }
                    else
                    {
                        if (isVideoLink)
                        {
                            SetDownloadStatusText(res.GetString("ErrorDownloadVideo"));
                            await SetMsgDialogText(res.GetString("ErrorDownloadVideo"));
                        }
                        else
                        {
                            SetDownloadStatusText(res.GetString("ErrorDownloadPicture"));
                            await SetMsgDialogText(res.GetString("ErrorDownloadPicture"));
                        }

                    }
                }
                else
                {
                    //imgsrc is null
                    //curDownload.IsDownloaded = "0";
                    SetDownloadStatusText(res.GetString("TargetFileNotFound"));
                    await SetMsgDialogText(res.GetString("TargetFileNotFound"));

                }




            }
            
            catch (Exception ex)
            {

                SetDownloadStatusText(res.GetString("ErrorWhenInitialize"));
                await SetMsgDialogText(ex.Message);
                await ErrorLogger.ErrorLog.WriteError("Error in pgHome_imgDownloader_tryBlockOne_Initialize\r\n" + ex.ToString());


            }
            finally
            {
      
                    txtUrl.Text = "";
                    prgDownload.Visibility =prgIntermediate.Visibility= Visibility.Collapsed;
                    btnDownload.IsEnabled = chkFullSizeImage.IsEnabled = btnPickUrlFromList.IsEnabled = txtUrl.IsEnabled = true;

                    txtstatusFadeAnimation.Storyboard?.Stop();
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
 
                GC.Collect();

            }

        }
        //private async Task<PostDetails> ShowAlbumListDialog(List<StickyNode> lstnode)
        //{
        //    var tcs = new TaskCompletionSource<PostDetails>();
        //    var dialogTask = tcs.Task;


        //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {


        //        btnChoosefromAlbum.Visibility = Visibility.Visible;
        //        //dlgAlbumPost loadalbum = new dlgAlbumPost();
        //        dlgLoadAlbum.IsFullSized = IsFullSizeImageBought;
        //        dlgLoadAlbum.LstAlbumPost = lstnode;
        //        var ress = await dlgLoadAlbum.ShowAsync();
        //        if (ress == ContentDialogResult.Primary)
        //        {
        //            isVideoLink = dlgLoadAlbum.IsVideo;
        //            postIdOrUsername = dlgLoadAlbum.PostId;
        //            tcs.SetResult(dlgLoadAlbum.SelectedNode);
        //        }
        //        else
        //        {
        //            tcs.SetResult(new PostDetails() { PostShortCode=""});
        //        };

        //    });

        //    var result = await dialogTask;

        //    return result;


        //}

        private async void Donate()
        {
            try
            {
                ///////////////////// begin read setting
                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                object donated = AppSettings.Values["IsDonated"];
                if (licenseInformation.ProductLicenses["FullSizeImageDownload"].IsActive) return;
                if (donated != null)
                {
                    if (donated.ToString() != "0") return;
                    ContentDialog donateDialog = new ContentDialog()
                    {
                        Title = res.GetString("DonatepopupTitle"),
                        Content = res.GetString("DonatepopupContent"),
                        PrimaryButtonText = res.GetString("DonatepopupPrimaryButtonText"),
                        SecondaryButtonText = res.GetString("DonatepopupSecondaryButtonText")
                    };

                    IsRatingOpened = true;
                    ContentDialogResult result = await donateDialog.ShowAsync();


                    if (result != ContentDialogResult.Primary) return;
                    try
                    {
                        AppSettings.Values["IsDonated"] = "1";
                        await new contentDonateList().ShowAsync();
                        //  await Windows.System.Launcher.LaunchUriAsync(new Uri(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SR6U393C95FT6"));
                    }
                    catch
                    {
                        // ignored
                    }
                }
                else
                {
                    AppSettings.Values["IsDonated"] = "0";
                }



            }
            catch (Exception)
            {
                // ignored
            }
        }
        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
          await PrepareForDownload(txtUrl.Text);
        }

        private async Task PrepareForDownload(string shareUrl, bool IsAlbumItem=false)
        {
            try
            {
                Regex regPost = new Regex(@"((http:\/\/(instagr\.am\/p\/.*|instagram\.com\/p\/.*|www\.instagram\.com\/p\/.*))|(https:\/\/(www\.instagram\.com\/p\/.*))|(https:\/\/(instagram\.com\/p\/.*)))");
                Regex regIgTv = new Regex(@"((http:\/\/(instagr\.am\/tv\/.*|instagram\.com\/tv\/.*|www\.instagram\.com\/tv\/.*))|(https:\/\/(www\.instagram\.com\/tv\/.*))|(https:\/\/(instagram\.com\/tv\/.*)))");

                Regex regProfile = new Regex(@"((http:\/\/(instagr\.am/.*|instagram\.com/.*|www\.instagram\.com/.*))|(https:\/\/(www\.instagram\.com/.*))|(https:\/\/(instagram\.com/.*)))");

                txtstatusFadeAnimation.Storyboard?.Begin();

                if (regPost.IsMatch(shareUrl) || regIgTv.IsMatch(shareUrl))
                {
                    imgDownloaded.Source = null;
                    imgDownloaded.Visibility = Visibility.Collapsed;
                    btnCommandViewFile.Visibility = Visibility.Collapsed;
                    btnCommandShareFile.Visibility = Visibility.Collapsed;
                    plyVideo.Visibility = Visibility.Collapsed;
                    plyVideo.Source = null;
                    btnChoosefromAlbum.Visibility = Visibility.Collapsed;
                    prgDownload.Value = 0;

                    flyoutDownloadProgress.Visibility = Visibility.Visible;

                    flyoutDownloadProgress.Visibility = prgDownload.Visibility = prgIntermediate.Visibility = Visibility.Visible;
                    btnDownload.IsEnabled = chkFullSizeImage.IsEnabled = btnPickUrlFromList.IsEnabled = txtUrl.IsEnabled = false;

                    Uri posturl = new Uri(shareUrl.Replace("http:", "https:"));


                    txtDownloadStatus.Text = res.GetString("DownloadInitialize");

                    if (!IsAlbumItem)
                    {
                        await Downloader(posturl.ToString()); 

                    }
                    else
                    {
                        await Downloader(posturl.ToString(),true);
                    }
                  //  txtUrl.Text = "";
                }
                else if (regProfile.IsMatch(txtUrl.Text))
                {
                    SetDownloadStatusText(res.GetString("ErrorProfilePicture"));
                    await new MessageDialog(res.GetString("ErrorProfilePicture")).ShowAsync();

                }

                else
                {
                    txtUrl.Focus(FocusState.Programmatic);
                    txtUrl.SelectAll();
                    await new MessageDialog(res.GetString("InvalidInstagramUrl")).ShowAsync();

                }

            }
            catch (Exception ex)
            {
                await new MessageDialog(res.GetString("UnhandledException")).ShowAsync();
                await ErrorLogger.ErrorLog.WriteError("Error in pgHome_btnDownload_Click\r\n" + ex.ToString());

            }
        }

        private async void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!txtUrl.IsEnabled) return;
                DataPackageView dataPackageView = Clipboard.GetContent();
                if (!dataPackageView.Contains(StandardDataFormats.Text)) return;
                string text = await dataPackageView.GetTextAsync();

                txtUrl.Text = text;

            }
            catch (Exception)
            {
                // ignored
            }
        }


        private async void btnViewFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync();
                await Windows.System.Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(btnCommandViewFile.Tag.ToString()));

            }
            catch (Exception ex)
            {

                MessageDialog msg = new MessageDialog(res.GetString("UnhandledException") + ex.Message);
                await  msg.ShowAsync();
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

        private async void txtUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtUrl.Text.Length != 0) return;
                if (!IsAutomaticallyPaste) return;
                DataPackageView dataPackageView = Clipboard.GetContent();
                if (!dataPackageView.Contains(StandardDataFormats.Text)) return;
                string clipboradtext = await dataPackageView.GetTextAsync();

                Regex regPost = new Regex(@"((http:\/\/(instagr\.am\/p\/.*|instagram\.com\/p\/.*|www\.instagram\.com\/p\/.*))|(https:\/\/(www\.instagram\.com\/p\/.*))|(https:\/\/(instagram\.com\/p\/.*)))");
                Regex regProfile = new Regex(@"((http:\/\/(instagr\.am/.*|instagram\.com/.*|www\.instagram\.com/.*))|(https:\/\/(www\.instagram\.com/.*))|(https:\/\/(instagram\.com/.*)))");
                Regex regIgTv = new Regex(@"((http:\/\/(instagr\.am\/tv\/.*|instagram\.com\/tv\/.*|www\.instagram\.com\/tv\/.*))|(https:\/\/(www\.instagram\.com\/tv\/.*))|(https:\/\/(instagram\.com\/tv\/.*)))");

                if (regPost.IsMatch(clipboradtext) || regProfile.IsMatch(clipboradtext) || regIgTv.IsMatch(clipboradtext))
                {
                    txtUrl.Text = clipboradtext;
                    txtUrl.SelectAll();
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
                            await new MessageDialog(res.GetString("FullSizeImageMsg")).ShowAsync();// Transaction ID: " + result.TransactionId.ToString()).ShowAsync();
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
            try
            {
                if (dlgLoadAlbum!=null)
                {
                    var ress = await dlgLoadAlbum.ShowAsync();
                    if (ress == ContentDialogResult.Primary)
                    {
                        string posturl = "https://instagram.com/p/" + dlgLoadAlbum.PostId;
                        await PrepareForDownload(posturl, true);
                    }
                }
             
            }
            catch (Exception)
            {

               
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
