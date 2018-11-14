using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DbHelper;
using QuickType;
// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InstagramDownloader
{
    public sealed partial class contentPostponeList : ContentDialog
    {
        private List<StickyNode> __lstAlbumPost;

        private string selectedUrl = "";
        private bool isBrowseselected = false;
        public string SelectedUrl
        {
            get
            {
                return selectedUrl;
            }

            set
            {
                selectedUrl = value;
            }
        }

        public List<StickyNode> LstAlbumPost
        {
            get
            {
                return __lstAlbumPost;
            }

            set
            {
                __lstAlbumPost = value;
            }
        }

        public bool IsBrowseselected
        {
            get
            {
                return isBrowseselected;
            }

            set
            {
                isBrowseselected = value;
            }
        }

        public contentPostponeList()
        {
            this.InitializeComponent();
            Loaded += ContentPostponeList_Loaded;
        }

        private async void ContentPostponeList_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Loaded -= ContentPostponeList_Loaded;


                using (dbHelperConnection db = new dbHelperConnection())
                {
                    var urls = db.tbl_History.Where(h => h.IsDownloaded == "0").OrderByDescending(h => h.DateInserted).Select(h => h);
                    if (urls.Any())
                    {
                        lstPostponedUrls.Visibility = Visibility.Visible;
                        lstPostponedUrls.ItemsSource = urls.ToArray();

                    }
                    else
                    {
                        // lstPostponedUrls.Visibility = Visibility.Collapsed;
                        lstPostponedUrls.ItemsSource = null;
                        txtLblNothingPopUp.Visibility = Visibility.Visible;
                    }
                }


            }
            catch (Exception ex)
            {
                await ErrorLogger.ErrorLog.WriteError("Error in ContentPostponeList_Loaded > LoadList\n" + ex.Message + ex.StackTrace);

            }

        }


        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (lstPostponedUrls.SelectedIndex != -1)
            {
                selectedUrl = (lstPostponedUrls.SelectedValue as tbl_History).Url;

            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void lstPostponedUrls_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            try
            {
                if (lstPostponedUrls.SelectedIndex != -1)
                {
                    ContentDialog_PrimaryButtonClick(null, null);
                    isBrowseselected = false;
                    Hide();


                }
            }
            catch (Exception)
            {


            }
        }

        private async void btnPopupDeletefromList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var SelectedRecord = ((FrameworkElement)e.OriginalSource).DataContext as tbl_History;
                if (SelectedRecord != null)
                {
                    using (dbHelperConnection db = new dbHelperConnection())
                    {
                        db.tbl_History.Remove(SelectedRecord);
                        if (db.SaveChanges() > 0)
                        {
                            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
                            {
                                Windows.Phone.Devices.Notification.VibrationDevice v = Windows.Phone.Devices.Notification.VibrationDevice.GetDefault();
                                v.Vibrate(TimeSpan.FromMilliseconds(25));
                            }

                            ContentPostponeList_Loaded(null, null);

                        }

                    }
                }


            }
            catch (Exception ex)
            {
                await ErrorLogger.ErrorLog.WriteError("Error in btnPopupDeletefromList_Click" + ex.Message + ex.StackTrace);

            }
        }

        private void btnPopUpCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var SelectedRecord = ((FrameworkElement)e.OriginalSource).DataContext as tbl_History;
                if (SelectedRecord != null)
                {
                    var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
                    dataPackage.SetText(SelectedRecord.Url);
                    Clipboard.SetContent(dataPackage);
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
                    {
                        Windows.Phone.Devices.Notification.VibrationDevice v = Windows.Phone.Devices.Notification.VibrationDevice.GetDefault();
                        v.Vibrate(TimeSpan.FromMilliseconds(25));
                    }
                }


            }
            catch (Exception)
            {


            }

        }

        private void btnPopUpBrowseUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var SelectedRecord = (((FrameworkElement)e.OriginalSource).DataContext as tbl_History);
                if (SelectedRecord != null)
                {
                    lstPostponedUrls.SelectedItem = SelectedRecord;
                    /* (Window.Current.Content as Frame)?.Navigate(typeof(pgBrowser), SelectedRecord);*/
                    selectedUrl = SelectedRecord.Url;
                    isBrowseselected = true;
                  
                    ContentDialog_PrimaryButtonClick(null, null);
                    Hide();
                }

            }
            catch (Exception)
            {


            }
        }

        private void ContentDialog_LostFocus(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }

}
