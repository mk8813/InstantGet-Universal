using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.Storage;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace InstagramDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainPage : Page
    {

        ResourceLoader res = ResourceLoader.GetForCurrentView();
        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;


        public MainPage()
        {
            InitializeComponent();
        
            if (AppSettings.Values["theme"]!=null && AppSettings.Values["theme"].ToString()=="2")//picture background
            {
                contentFrame.Background = new ImageBrush() { Stretch=Stretch.UniformToFill
                    , ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/bg.png"))
                , Opacity=0.5f};
            }
           
            try
            {
                rdoHome_Click(null, null);
                contentFrame.Navigated += myFrame_Navigated;

                // register BackRequested event to handle myFrame's GoBack
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            }
            catch (Exception)
            {

              
            }
        
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (e.Parameter.ToString() == "viewBrowser")
                {
                    contentFrame.Navigate(typeof(pgBrowser));
                    rdoBrowser.IsChecked = true;
                    lblMainHeader.Text = res.GetString("Browser");

                }
            }
            catch (Exception)
            {


            }
          
        }
        private void myFrame_Navigated(object sender, NavigationEventArgs e)
        {
            try
            {
              
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
               ((Frame)sender).CanGoBack ?
               AppViewBackButtonVisibility.Visible :
               AppViewBackButtonVisibility.Collapsed;

                if (e.SourcePageType == typeof(pgHome))
                {
                    rdoHome.IsChecked = true;
                    lblMainHeader.Text =res.GetString("Download");

                }
                else if (e.SourcePageType == typeof(pgHistory))
                {
                    rdoHistory.IsChecked = true;
                    lblMainHeader.Text = res.GetString("History");
                }
                else if (e.SourcePageType == typeof(pgHelp))
                {
                    rdoAbout.IsChecked = true;
                    lblMainHeader.Text = res.GetString("About");
                }
                else if (e.SourcePageType == typeof(pgSettings))
                {
                    rdoSettings.IsChecked = true;
                    lblMainHeader.Text = res.GetString("Settings");
                }
                else if (e.SourcePageType == typeof(pgBrowser))
                {
                    rdoBrowser.IsChecked = true;
                    lblMainHeader.Text = res.GetString("Browser");
                }
            }
            catch (Exception)
            {

               
            }
           
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            try
            {
                if (contentFrame.CanGoBack)
                {
                    e.Handled = true;
                    contentFrame.GoBack();
                }
            }
            catch (Exception)
            {

             
            }
           
        }
        
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnHamburger_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShellSplitView.IsPaneOpen = !ShellSplitView.IsPaneOpen;
                ((RadioButton)sender).IsChecked = false;
               
            }
            catch (Exception)
            {

            
            }
        }

        private void rdoHome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShellSplitView.IsPaneOpen = false;
                contentFrame.Navigate(typeof(pgHome));
                lblMainHeader.Text = res.GetString("Download");
            
            }
            catch (Exception)
            {

            
            }
        
        }

        private void rdoHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShellSplitView.IsPaneOpen = false;
                contentFrame.Navigate(typeof(pgHistory));
                lblMainHeader.Text = res.GetString("History");
            }
            catch (Exception)
            {


            }
        }

        private void rdoSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShellSplitView.IsPaneOpen = false;
             
                contentFrame.Navigate(typeof(pgSettings));
                lblMainHeader.Text = res.GetString("Settings");
            }
            catch (Exception)
            {


            }
        }

        private void rdoAbout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShellSplitView.IsPaneOpen = false;

                contentFrame.Navigate(typeof(pgHelp));

                lblMainHeader.Text = res.GetString("About");
            }
            catch (Exception)
            {


            }
        }

        private void rdoBrowser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShellSplitView.IsPaneOpen = false;

                contentFrame.Navigate(typeof(pgBrowser));
                lblMainHeader.Text = res.GetString("Browser");
            }
            catch (Exception)
            {

               
            }
        }

      

        private void ImageBrush_ImageOpened_1(object sender, RoutedEventArgs e)
        {
            ////////////////////////
          //grdMainPage.Background= contentFrame.Background = new  ImageBrush() { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/bg.png")) };
           // new Windows.UI.Xaml.Media.ImageBrush() { ImageSource = await BlurElement.BlurThisUI(grdBlurBackg) };
            //grdBlurBackg.Visibility = Visibility.Collapsed;

            ///////////////////////////////////////////////////
        }

        private void SplitViewPane_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X < -50)
            {
                ShellSplitView.IsPaneOpen = false;

            }

            if (e.Cumulative.Translation.X > 50)
            {
                ShellSplitView.IsPaneOpen = true;
            }
        }

        private void SplitViewOpener_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X > 50)
            {
                ShellSplitView.IsPaneOpen = true;
            }
        }

    }
}
