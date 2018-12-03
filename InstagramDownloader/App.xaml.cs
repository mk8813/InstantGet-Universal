using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace InstagramDownloader
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        ResourceLoader res = ResourceLoader.GetForViewIndependentUse();
        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
        ToastNotificationHistory th = ToastNotificationManager.History;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {

            this.InitializeComponent();
            this.Suspending += OnSuspending;
            
            ClearToastNotifications();//clear toasts

            /////////////////////language/////////////////
            ApplyLanguage();
            //////////////////////end language////////////////

            CheckBackgroundTaskActivation();//check background task

            ApplyTheme();//apply user selected theme

            ////////////////////////



        }

        private async void ClearToastNotifications()
        {
            try
            {
                if (th.GetHistory().Any())
                {
                    th.Clear();
                }

            }
            catch (Exception ex)
            {

              await  ErrorLogger.ErrorLog.WriteError("Error in ClearToastNotifications Method App.cs "+ex.Message+ex.StackTrace);
            }
           
        }

        private async void ApplyTheme()
        {
            try
            {
                object theme = AppSettings.Values["theme"];
                if (theme != null)
                {
                    if (theme.ToString() == "1")
                    {
                        this.RequestedTheme = ApplicationTheme.Dark;
                    }
                    else if (theme.ToString() == "0")
                    {
                        this.RequestedTheme = ApplicationTheme.Light;
                    }
                    else if (theme.ToString() == "2")
                    {
                        var uiSettings = new UISettings();
                        var color = uiSettings.GetColorValue(
                                                Windows.UI.ViewManagement.UIColorType.Background
                                               );

                        if (color == Windows.UI.Colors.Black)
                        {
                            this.RequestedTheme = ApplicationTheme.Dark;
                        }
                        else if (color == Windows.UI.Colors.White)
                        {
                            this.RequestedTheme = ApplicationTheme.Light;
                        }
                    }

                }
                else
                {
                    if (this.RequestedTheme == ApplicationTheme.Dark)
                    {

                        AppSettings.Values["theme"] = "1";
                    }
                    else if (this.RequestedTheme == ApplicationTheme.Light)
                    {
                        AppSettings.Values["theme"] = "0";
                    }

                }
            }
            catch (Exception ex)
            {

                await ErrorLogger.ErrorLog.WriteError("Error in ApplyTheme Method App.cs " + ex.Message + ex.StackTrace);
            }

        }

        private async void CheckBackgroundTaskActivation()
        {
            try
            {
                object val = AppSettings.Values["bgStatus"];
                if (val != null)
                {
                    if (val.ToString() == "1")
                    {
                        var isAlreadyRegistered = Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == "UrlListener");
                        if (isAlreadyRegistered)
                        {

                            if (th.GetHistory().Where(t => t.Tag == "AgentToast").Count() == 0)
                            {

                                Sendnotidication();
                            }
                        }
                        else
                        {
                            var result = new MessageDialog(res.GetString("ActionCenterIsntRunning")).ShowAsync();

                        }


                    }


                }
            }
            catch (Exception ex)
            {

                await ErrorLogger.ErrorLog.WriteError("Error in CheckBackgroundTaskActivation Method App.cs " + ex.Message + ex.StackTrace);
            }

        }

        private async void ApplyLanguage()
        {
            try
            {
                object lang = AppSettings.Values["localLang"];
                if (lang != null)
                {
                    try
                    {
                        if (lang.ToString().ToLower() != "auto")
                        {
                            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = lang.ToString();

                        }
                        else
                        {

                            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Windows.System.UserProfile.GlobalizationPreferences.Languages[0].ToString();



                        }


                    }
                    catch (Exception ex)
                    {

                        await ErrorLogger.ErrorLog.WriteError("Error in ApplyLanguage Method-TryB1 App.cs " + ex.Message + ex.StackTrace);
                    }
                }
                else
                {
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Windows.System.UserProfile.GlobalizationPreferences.Languages[0].ToString();


                }
            }
            catch (Exception ex)
            {

                await ErrorLogger.ErrorLog.WriteError("Error in ApplyLanguage Method-TryB2 App.cs " + ex.Message + ex.StackTrace);
            }

        }



        private async System.Threading.Tasks.Task<bool>  CopyDatabase()
        {
            bool isDatabaseExisting = false;

            try
            {
                StorageFile storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync("db.sqlite");
                isDatabaseExisting = true;
                return true;
            }
            catch
            {
                isDatabaseExisting = false;
            }

            try
            {
                if (!isDatabaseExisting)
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/db/db.sqlite"));
                    await file.CopyAsync(Windows.Storage.ApplicationData.Current.LocalFolder, "db.sqlite");
                    return true;
                }
            }
            catch (Exception)
            {

                return false;
            }

            return false;
           
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


        private async void ShowStatus()
        {
            try
            {
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationViewTitleBar"))
                {
                   // var mytitle = Application.Current.Resources[0] as UIElement;
                   // Windows.ApplicationModel.Core.CoreApplicationViewTitleBar coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
                    //coreTitleBar.ExtendViewIntoTitleBar = true;

                    //TitleBar.Height = coreTitleBar.Height;
                    //Window.Current.SetTitleBar(mytitle);
                    ///////////////////////////////////////////////
                    //Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                    //ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                    //titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                    //titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;


                }
                //Mobile customization
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    
                    var statusBar = StatusBar.GetForCurrentView();
                    if (statusBar != null)
                    {
                        statusBar.BackgroundOpacity = 100;
                        statusBar.BackgroundColor = (Windows.UI.Color)Current.Resources["SystemAccentColor"] ;//Windows.UI.Colors.Transparent; //Windows.UI.Colors.Black;
                        statusBar.ForegroundColor = (Windows.UI.Color)(Current.Resources["ApplicationForegroundThemeBrush"] as Windows.UI.Xaml.Media.SolidColorBrush).Color;
                        await statusBar.ShowAsync();

                      
                    }
                }
            }
            catch (Exception ex)
            {
                MessageDialog msg = new MessageDialog(ex.Message);
                await msg.ShowAsync();

            }
           

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


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            try
            {

                Frame rootFrame = Window.Current.Content as Frame;
                
                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;
                    rootFrame.Navigated += OnNavigated;
                    

                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Load state from previously suspended application
                    }

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }
               
                if (e.PrelaunchActivated == false)
                {
                    if (rootFrame.Content == null)
                    {
                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation
                        // parameter
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                    // Ensure the current window is active
                    Window.Current.Activate();
                }
                ////////////////////

                if (e.Kind == ActivationKind.Launch)
                {
                    if (e.Arguments.Equals("viewBrowser"))
                    {
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                }

                //////////////
                //Register a handler for BackRequested events and set the

                //visibility of the Back button

                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
             
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    rootFrame.CanGoBack ?
                    Windows.UI.Core.AppViewBackButtonVisibility.Visible :
                    Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;


              
                //try
                //{
                //    ShowStatus();
                //}
                //catch (Exception)
                //{


                //}
                 ///////////////load db
                ////////////////////////
             
                if (!(await CopyDatabase()))
                {
                    await new MessageDialog(ResourceLoader.GetForViewIndependentUse().GetString("FailedToLoadData")).ShowAsync();
                 
                }
               



            }
            catch (Exception)
            {

              
            }

        }

        

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            try
            {
                // Each time a navigation event occurs, update the Back button's visibility
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    ((Frame)sender).CanGoBack ?
                    Windows.UI.Core.AppViewBackButtonVisibility.Visible :
                    Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
            }
            catch (Exception)
            {

            }
           
        }
        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            try
            {

                Frame rootFrame = Window.Current.Content as Frame;

                if (rootFrame.CanGoBack)
                {
                    e.Handled = true;
                    rootFrame.GoBack();
                }
                else if (rootFrame.GetType() == (typeof(pgHelp)) || rootFrame.GetType() == (typeof(pgHistory)) || rootFrame.GetType() == (typeof(pgSettings)) || rootFrame.GetType() == (typeof(pgBrowser)))
                {
                    e.Handled = true;
                    rootFrame.Navigate(typeof(MainPage));
                }
            }
            catch (Exception)
            {

               
            }
          
        }
        private void OnHardwareButtonsBackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            try
            {
                Frame rootFrame = Window.Current.Content as Frame;

                if (rootFrame.CanGoBack)
                {
                    e.Handled = true;
                    rootFrame.GoBack();
                }
                else if (rootFrame.GetType() == (typeof(pgHelp)) || rootFrame.GetType() == (typeof(pgHistory)))
                {
                    e.Handled = true;
                    rootFrame.Navigate(typeof(MainPage));
                }
            }
            catch (Exception)
            {

              
            }
           
        }
        protected async override void OnActivated(IActivatedEventArgs args)
        {
            try
            {
                if (args.Kind == ActivationKind.ToastNotification)
                {
                    var toastArgs = args as ToastNotificationActivatedEventArgs;
                    var arguments = toastArgs.Argument;

                    if (!string.IsNullOrEmpty(arguments))
                    {
                        if (Path.HasExtension(arguments))
                        {
                            try
                            {
                                Frame rootFrame = Window.Current.Content as Frame;
                                if (rootFrame == null)
                                {
                                    rootFrame = new Frame();
                                    Window.Current.Content = rootFrame;
                                }
                                rootFrame.Navigate(typeof(MainPage));
                                Window.Current.Activate();

                                var res = await Windows.System.Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(arguments));

                            }
                            catch (Exception)
                            {


                            }
                        }
                        else
                        {
                            Frame rootFrame = Window.Current.Content as Frame;
                            if (rootFrame == null)
                            {
                                rootFrame = new Frame();
                                Window.Current.Content = rootFrame;
                            }
                            rootFrame.Navigate(typeof(MainPage));
                            Window.Current.Activate();
                        }

                    }
                }
               
            }
            catch (Exception)
            {

             
            }
           
        }

   

    }
}
