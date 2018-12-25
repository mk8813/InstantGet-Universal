using System;
//using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.UI.Notifications;
using DbHelper;
using BackgroundDownloadHandler;
using Windows.Networking.BackgroundTransfer;

namespace UrlListenerComponent
{
    public sealed class BackgroundTask : IBackgroundTask
    {
      
        BackgroundTaskDeferral _deferral = null;

        private BackgroundTaskCancellationReason cancelReason = BackgroundTaskCancellationReason.Abort;
        private volatile bool cancelRequested = false;
        ResourceLoader res = ResourceLoader.GetForViewIndependentUse();
        private LicenseInformation licenseInformation;
        private bool IsFullSizeImageBought = false;

        BackgroundDownloaderManager bgDownloader;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {

                taskInstance.Canceled += OnCanceled;
                _deferral = taskInstance.GetDeferral();

                if (cancelRequested == false)
                {


                    var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                    if (details != null)
                    {

                        bgDownloader = new BackgroundDownloaderManager();
                        
                        string arguments = details.Argument;
                        var userInput = details.UserInput;
                        string inputurl = userInput["inputurl"].ToString(); // textbox value
                                                                            /////////////////////////////////////////////////////
                        Regex regPost = new Regex(@"((http:\/\/(instagr\.am\/p\/.*|instagram\.com\/p\/.*|www\.instagram\.com\/p\/.*))|(https:\/\/(www\.instagram\.com\/p\/.*))|(https:\/\/(instagram\.com\/p\/.*)))");
                        Regex regIGTv = new Regex(@"((http:\/\/(instagr\.am\/tv\/.*|instagram\.com\/tv\/.*|www\.instagram\.com\/tv\/.*))|(https:\/\/(www\.instagram\.com\/tv\/.*))|(https:\/\/(instagram\.com\/tv\/.*)))");

                       // Regex regProfile = new Regex(@"((http:\/\/(instagr\.am/.*|instagram\.com/.*|www\.instagram\.com/.*))|(https:\/\/(www\.instagram\.com/.*))|(https:\/\/(instagram\.com/.*)))");
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
                                    Progress<DownloadOperation> dlProgress = new Progress<DownloadOperation>(progressChanged);

                                    if (regPost.IsMatch(inputurl) || regIGTv.IsMatch(inputurl))
                                    {
                                       
                                        string posturl = inputurl.Replace("http:", "https:");
                                        await bgDownloader.DownloadPost(posturl, dlProgress);//.ConfigureAwait(false);

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
                                    if (regPost.IsMatch(inputurl) || regIGTv.IsMatch(inputurl))
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
                                                int result = await db.SaveChangesAsync();//.ConfigureAwait(false); ;
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




    }



}
