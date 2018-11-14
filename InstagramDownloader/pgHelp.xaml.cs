using System;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InstagramDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgHelp : Page
    {
        ResourceLoader res = ResourceLoader.GetForCurrentView();

        public pgHelp()
        {
            this.InitializeComponent();
            Loaded += PgHelp_Loaded;
            lblVersion.Text =res.GetString("Version")+ " "+ GetAppVersion();
            lblCopyright.Text = "Copyright © "+DateTime.Now.Year.ToString()+" Mehdi Kheirandish";
        }

        public  string GetAppVersion()
        {

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);

        }

        private bool IsDonateMessageShow = false;
        private void PgHelp_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string curTimeZone = TimeZoneInfo.Local.Id;
                int rnd = new Random().Next(1, 12);

                if (curTimeZone.ToLower().Contains("iran"))
                {
                    btnDonate.Visibility = Visibility.Visible;

                    if (!IsDonateMessageShow)
                    {

                        if (rnd <= 2)
                        {
                            DonateIran();
                        }


                    }
                   
                }
                else
                {
                    if (!IsDonateMessageShow)
                    {

                        if (rnd <= 2)
                        {
                            Donate();
                        }


                    }
                }

               
        
            }
            catch (Exception)
            {
               
            

            }
        }

        private async void Donate()
        {
            try
            {
                ///////////////////// begin read setting
                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                object donated = AppSettings.Values["IsDonated"];

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

                        IsDonateMessageShow = true;
                        ContentDialogResult result = await donateDialog.ShowAsync();


                        if (result == ContentDialogResult.Primary)
                        {
                            try
                            {
                                AppSettings.Values["IsDonated"] = "1";
                                await new contentDonateList().ShowAsync();
                               // await Windows.System.Launcher.LaunchUriAsync(new Uri(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SR6U393C95FT6"));

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
            catch (Exception)
            {


            }
        }


        private async void DonateIran()
        {
            try
            {
                ///////////////////// begin read setting
                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                object donated = AppSettings.Values["IsDonated"];

                if (donated != null)
                {
                    if (donated.ToString() == "0")
                    {
                        ContentDialog rateDialog = new ContentDialog()
                        {
                            Title = ("تقدیم هدیه"),
                            Content =("برای حمایت از برنامه نویس و توسعه اپلیکیشن های بیشتر میتونین مبلغی رو هدیه کنین"),
                            PrimaryButtonText = ("🙂 باشه"),
                            SecondaryButtonText = ("🙁 نه")
                        };

                        IsDonateMessageShow = true;
                        ContentDialogResult result = await rateDialog.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            try
                            {
                                AppSettings.Values["IsDonated"] = "1";
                                btnDonate_Click(null, null);
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
            catch (Exception)
            {


            }
        }

        private void ShowNotification(string text)
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
            var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notifi.Show(toast);
        }


        private async void btnRate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?PFN={Package.Current.Id.FamilyName}"));
            }
            catch (Exception)
            {

              
            }
        }

        async System.Threading.Tasks.Task<bool> CheckErrorFileExist()
        {
            try
            {
                StorageFile storageFile = await ApplicationData.Current.LocalCacheFolder.GetFileAsync("errorlog.txt");
                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }
        private async void btnFeedback_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
                emailMessage.Subject = "InstantGet Universal Feedback";
              
                var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient("mk8813@hotmail.com");

                if (await CheckErrorFileExist())
                {
                    StorageFile storageFile = await ApplicationData.Current.LocalCacheFolder.GetFileAsync("errorlog.txt");
                    var stream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(storageFile);
                    Windows.ApplicationModel.Email.EmailAttachment emailattach = new Windows.ApplicationModel.Email.EmailAttachment("Report.log",stream);

                    emailMessage.Attachments.Add(emailattach);
                }
                /////////////////////////

                emailMessage.To.Add(emailRecipient);
                await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
            }
            catch (Exception)
            {
                             
            }
        }

        private async void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {

                    await Windows.System.Launcher.LaunchUriAsync(new Uri("https://idpay.ir/mk8813"));
                }
                catch (Exception)
                {


                }

                //System.Text.StringBuilder sb = new System.Text.StringBuilder();
                //sb.Append("شماره کارت :"+Environment.NewLine);
                //sb.Append("۶۰۳۷-۹۹۷۲-۳۷۲۰-۱۰۳۴" + Environment.NewLine);
                //sb.Append("بانک ملی ایران" + Environment.NewLine);
                //sb.Append("به نام مهدی خیراندیش" + Environment.NewLine);

                //ShowNotification(sb.ToString());
            }
            catch (Exception)
            {

              
            }
        }

        private async void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {

                await Windows.System.Launcher.LaunchUriAsync(new Uri($"https://www.instagram.com/mk8813"));
            }
            catch (Exception)
            {


            }
        }

        private async void TextBlock_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();

                emailMessage.Subject = "Translate InstantGet Universal";
                var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient("mk8813@hotmail.com");
                emailMessage.To.Add(emailRecipient);
            
                await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
            }
            catch (Exception)
            {


            }
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }



        private void Image_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            try
            {
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
            }
            catch (Exception)
            {

             
            }
         
        }

        private void Image_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            try
            {
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);

            }
            catch (Exception)
            {

            
            }
        }

        private async void imgDonateBtn_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {

                await new contentDonateList().ShowAsync();
                // await Windows.System.Launcher.LaunchUriAsync(new Uri(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SR6U393C95FT6"));

            }
            catch (Exception)
            {


            }
        }
        private void txtMoreApps_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);


        }

        private void txtMoreApps_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);

        }

        private async void txtMoreApps_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"ms-windows-store://publisher/?name=Mehdi Kheirandish"));
            }
            catch (Exception)
            {


            }
        }


    }
}
