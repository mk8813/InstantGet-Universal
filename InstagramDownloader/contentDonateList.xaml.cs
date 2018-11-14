
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InstagramDownloader
{
    public sealed partial class contentDonateList : ContentDialog
    {
        Donate[] mylist;
        private LicenseInformation licenseInformation;

        public contentDonateList()
        {
            this.InitializeComponent();
            /////////////////////////
          ///*  Uncomment */the following line in the release version of your app.
               licenseInformation = CurrentApp.LicenseInformation;

           // Initialize the license info for testing.
           // Comment the following line in the release version of your app.
           //licenseInformation = CurrentAppSimulator.LicenseInformation;
            ///////////////////////


            mylist = new[]{
                new Donate() {Emoji="🙂 Donate $1.99",Price="$1.99",ProductID="JustSayThanks" },
                  new Donate() {Emoji="😉 Donate $4.99",Price="$4.99",ProductID="Donate499" },
                   new Donate() {Emoji="😇 Donate $9.99",Price="$9.99",ProductID="Donate999" },
                      new Donate() {Emoji="😎 Donate $14.99",Price="$14.99",ProductID="Donate1499" },
                         new Donate() {Emoji="😍 Donate $19.99",Price="$19.99",ProductID="Donate1999" },

          };
            populateDonateList();
        }

       void populateDonateList()
        {
            if (mylist.Any())
            {
                lstDonation.ItemsSource = mylist.ToArray();
            }
         
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
        public class Donate
        {
            string emoji;
            string price;
            string productID;

            public string Emoji
            {
                get
                {
                    return emoji;
                }

                set
                {
                    emoji = value;
                }
            }

            public string Price
            {
                get
                {
                    return price;
                }

                set
                {
                    price = value;
                }
            }

            public string ProductID
            {
                get
                {
                    return productID;
                }

                set
                {
                    productID = value;
                }
            }
        }

        private async void lstDonation_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                this.Hide();
                ShowProgressBarStatus(true);

                var item = ((FrameworkElement)sender).DataContext as Donate;
                if (item != null)
                {
                    try
                    {
                        if (!licenseInformation.ProductLicenses[item.ProductID].IsActive) //bought
                        {
                            //begin donation
                            var result = await CurrentApp.RequestProductPurchaseAsync(item.ProductID);
                            if (result.Status == ProductPurchaseStatus.Succeeded || result.Status == ProductPurchaseStatus.AlreadyPurchased)
                            {
                                await ShowMessageDialog("Thank you! ");// Transaction ID: " + result.TransactionId.ToString()).ShowAsync();
                            }
                            else
                            {

                                await ShowMessageDialog("Oops! Failed.");
                            }

                        }
                        else
                        {
                            await  ShowMessageDialog("Already Donated! Thank you.");
                        }



                    }
                    catch (Exception ex)
                    {

                        await ShowMessageDialog(ex.Message);
                    }
                }
                ShowProgressBarStatus(false);
            }
            catch (Exception ex)
            {
                await ShowMessageDialog(ex.Message);

            }
           
        }

       
         private async System.Threading.Tasks.Task ShowMessageDialog(string message)
        {
            try
            {
                ContentDialog msg = new ContentDialog()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsPrimaryButtonEnabled = true,
                    Title= "InstantGet Universal",
                    PrimaryButtonText = "OK",

                    Content = message,

                };

                await msg.ShowAsync();
            }
            catch (Exception)
            {

               
            }
          
        }
        async void ShowProgressBarStatus(bool show)
        {
            try
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    var statusBar = StatusBar.GetForCurrentView();
                    if (show)
                    {
                        statusBar.ProgressIndicator.ProgressValue = null;
                        await statusBar.ProgressIndicator.ShowAsync();

                    }
                    else
                    {
                        await statusBar.ProgressIndicator.HideAsync();
                        statusBar.ProgressIndicator.ProgressValue = null;
                    }
                }
            }
            catch (Exception)
            {


            }


        }



    }


}
