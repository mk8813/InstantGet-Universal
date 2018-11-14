using System;
using System.Linq;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InstagramDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgSettings : Page
    {
        ResourceLoader res = ResourceLoader.GetForCurrentView();

        public pgSettings()
        {
            this.InitializeComponent();
            Loaded += PgSettings_Loaded;
            this.SizeChanged += PgSettings_SizeChanged;
          
        }

        private void PgSettings_SizeChanged(object sender, SizeChangedEventArgs e)
        {
         //   txtSavePath.Width = this.Width-btnSelectPath.Width+5;
        }

        private async void PgSettings_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //////////////
                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                object val = AppSettings.Values["bgStatus"];
                object theme = AppSettings.Values["theme"];
                object clipboard = AppSettings.Values["autoPaste"];
                object PictureSavePath = AppSettings.Values["picture"];
                object VideoSavePath = AppSettings.Values["video"];

                #region backtask
                if (val != null)
                {
                    if (val.ToString() == "1")
                    {
                        chkBackTask.IsOn = true;
                    }
                    else
                    {
                        chkBackTask.IsOn = false;
                    }


                }
                #endregion backtask

                #region theme
                if (theme != null)
                {
                    if (theme.ToString() == "1")
                    {
                        cmbTheme.SelectedIndex = 1;
                        //this.RequestedTheme = ElementTheme.Dark;
                       // chkTheme.IsOn = true;
                    }
                    else if (theme.ToString() == "0")
                    {
                        cmbTheme.SelectedIndex = 0;
                        //this.RequestedTheme = ElementTheme.Light;
                     //   chkTheme.IsOn = false;
                    }
                    else
                    {
                        cmbTheme.SelectedIndex =2;
                    }
                }
                else
                {
                    if (RequestedTheme == ElementTheme.Dark)
                    {
                        cmbTheme.SelectedIndex = 1;
                        //chkTheme.IsOn = true;
                    }
                    else if (RequestedTheme == ElementTheme.Light)
                    {
                        cmbTheme.SelectedIndex =0;
                    }
                }
                #endregion theme


                #region clipboard
                if (clipboard != null)
                {
                    if (clipboard.ToString() == "1")
                    {
                        
                        chkClipboardAutoPaste.IsOn = true;
                    }
                    else if (clipboard.ToString() == "0")
                    {
                      
                        chkClipboardAutoPaste.IsOn = false;
                    }
                }
                #endregion clipboard

                /////language
                object lang = AppSettings.Values["localLang"];
                if (lang != null)
                {
                    try
                    {
                        foreach (var item in cmbLanguage.Items)
                        {
                            var i = item as ComboBoxItem;
                            if (i.DataContext.ToString()==lang.ToString())
                            {
                                cmbLanguage.SelectedItem = i;
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        cmbLanguage.SelectedIndex = 0;

                    }
                }
                else
                {
                    cmbLanguage.SelectedIndex = 0;

                }
                //////////////////

                /////////////////picture save location
                if (PictureSavePath != null)
                {
                    txtSavePath.Text = PictureSavePath.ToString();
                }
                else
                {
                    var PictureLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);

                    AppSettings.Values["picture"] = PictureLibrary.SaveFolder.Path;// KnownFolders.SavedPictures.Path;
                    txtSavePath.Text = PictureLibrary.SaveFolder.Path; //KnownFolders.SavedPictures.Path;

                }


                ///videos location
                if (VideoSavePath != null)
                {
                    txtVideoPath.Text = VideoSavePath.ToString();

                }
                else
                {
                    var VideosLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
                    AppSettings.Values["video"] = VideosLibrary.SaveFolder.Path;   // KnownFolders.VideosLibrary.Path;
                    txtVideoPath.Text = VideosLibrary.SaveFolder.Path;// KnownFolders.VideosLibrary.Path;
                }

                ToolTipService.SetToolTip(txtVideoPath, txtVideoPath.Text);
                ToolTipService.SetToolTip(txtSavePath, txtSavePath.Text);



                /////
            }
            catch (Exception)
            {


            }
        }

        private async void chkBackTask_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {


                if (chkBackTask.IsOn)
                {

                    ///////////////////////////begin task check
                    var isAlreadyRegistered = BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == "UrlListener");
                    if (isAlreadyRegistered)
                    {
                        foreach (var tsk in BackgroundTaskRegistration.AllTasks)
                        {
                            if (tsk.Value.Name == "UrlListener")
                            {
                                tsk.Value.Unregister(true);
                                break;
                            }
                        }
                    }
                    var isToastAlreadyRegistered = BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == "ToastManager");
                    if (isToastAlreadyRegistered)
                    {
                        foreach (var tsk in BackgroundTaskRegistration.AllTasks)
                        {
                            if (tsk.Value.Name == "ToastManager")
                            {
                                tsk.Value.Unregister(true);

                                break;
                            }
                        }
                    }
                    ////////////////////////////


                    BackgroundExecutionManager.RemoveAccess();
                    var hasAccess = await BackgroundExecutionManager.RequestAccessAsync();
                    if (hasAccess == BackgroundAccessStatus.DeniedBySystemPolicy || hasAccess == BackgroundAccessStatus.DeniedByUser || hasAccess == BackgroundAccessStatus.Unspecified)
                    {
                        ShowNotification(res.GetString("bgPermissionFailed"));
                        return;
                    }

                    /////////////////////begin registering Url listener ttask
                    var task = new BackgroundTaskBuilder
                    {
                        Name = "UrlListener",
                        TaskEntryPoint = typeof(UrlListenerComponent.BackgroundTask).ToString()
                    };

                    ToastNotificationActionTrigger actiontrigger = new ToastNotificationActionTrigger();

                    task.SetTrigger(actiontrigger);
                    //var condition = new SystemCondition(SystemConditionType.InternetAvailable);

                    //task.AddCondition(condition);//condition
                    BackgroundTaskRegistration registration = task.Register();

                    /////////////////////begin register Toast manager task
                    var bgtaskToastManager = new BackgroundTaskBuilder
                    {
                        Name = "ToastManager",
                        TaskEntryPoint = typeof(ActionCenterToastHandler.ActionCenterIntractiveToastCheck).ToString()
                    };
                    ToastNotificationHistoryChangedTrigger toastTrigger = new ToastNotificationHistoryChangedTrigger();

                    bgtaskToastManager.SetTrigger(toastTrigger);
                    //var cndt = new SystemCondition(SystemConditionType.InternetAvailable);
                    //bgtaskToastManager.AddCondition(cndt);//condition
                    bgtaskToastManager.Register();

                    ToastNotificationHistory th = ToastNotificationManager.History;
                    if (th.GetHistory().Where(t => t.Tag == "AgentToast").Count() == 0)
                    {

                        Sendnotidication();
                    }

                    ///////////////////// begin saving setting
                    ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                    AppSettings.Values["bgStatus"] = "1";

                }
                else
                {
                    var isToastAlreadyRegistered = BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == "ToastManager");
                    if (isToastAlreadyRegistered)
                    {
                        foreach (var tsk in BackgroundTaskRegistration.AllTasks)
                        {
                            if (tsk.Value.Name == "ToastManager")
                            {
                                tsk.Value.Unregister(true);

                                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                                AppSettings.Values["bgStatus"] = "0";

                                ToastNotificationHistory th = ToastNotificationManager.History;
                                th.Remove("AgentToast");

                                break;
                            }
                        }
                    }


                    var isAlreadyRegistered = BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == "UrlListener");
                    if (isAlreadyRegistered)
                    {
                        foreach (var tsk in BackgroundTaskRegistration.AllTasks)
                        {
                            if (tsk.Value.Name == "UrlListener")
                            {
                                tsk.Value.Unregister(true);
                                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                                AppSettings.Values["bgStatus"] = "0";

                                break;
                            }
                        }
                    }


                }
            }
            catch (Exception)
            {

                ShowNotification(res.GetString("FailedToEnableActionCenter"));
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


        private async void btnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPicker = new Windows.Storage.Pickers.FolderPicker();
                folderPicker.FileTypeFilter.Add("*");
                folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;

                Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    // Application now has read/write access to all contents in the picked folder
                    // (including other sub-folder contents)
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                    try
                    {
                        await StorageFolder.GetFolderFromPathAsync(folder.Path);
                        txtSavePath.Text = folder.Path;
                        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                        AppSettings.Values["picture"] = folder.Path;
                        ToolTipService.SetToolTip(txtSavePath, folder.Path);
                    }
                    catch (Exception)
                    {

                        MessageDialog msg = new MessageDialog(res.GetString("AccessDenied"));
                        await msg.ShowAsync();
                    }
                  
                   
                }
               
            }
            catch (Exception)
            {

               
            }
        }

        private async void btnSelectVideoPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPicker = new Windows.Storage.Pickers.FolderPicker();
                folderPicker.FileTypeFilter.Add("*");
                folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;

                Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    // Application now has read/write access to all contents in the picked folder
                    // (including other sub-folder contents)
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                    try
                    {
                       await StorageFolder.GetFolderFromPathAsync(folder.Path);
                        txtVideoPath.Text = folder.Path;
                        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                        AppSettings.Values["video"] = folder.Path;

                        ToolTipService.SetToolTip(txtVideoPath, folder.Path);
                    }
                    catch (Exception)
                    {

                        MessageDialog msg = new MessageDialog(res.GetString("AccessDenied"));
                        await msg.ShowAsync();
                    }
                

                }

            }
            catch (Exception)
            {
              
               
            }
        }

      

      
        private  void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
              
                var val = cmbLanguage.SelectedItem as ComboBoxItem;
                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                AppSettings.Values["localLang"] = val.DataContext.ToString();
            

            }
            catch (Exception)
            {
               
               
            }
         
        }

        private void cmbLanguage_DropDownClosed(object sender, object e)
        {
            if (cmbLanguage.SelectedIndex!=DropLangCurrentIndex)
            {
                blckSettingNote.Visibility = Visibility.Visible;
            }
          

        }

        int DropLangCurrentIndex = -1;
        private void cmbLanguage_DropDownOpened(object sender, object e)
        {
            DropLangCurrentIndex = cmbLanguage.SelectedIndex;
        }

        private void chkClipboardAutoPaste_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;
                if (chkClipboardAutoPaste.IsOn)
                {
                    AppSettings.Values["autoPaste"] = "1";
                }
                else
                {
                    AppSettings.Values["autoPaste"] = "0";
                }
              
               
            }
            catch (Exception)
            {

               
            }
        }

        private void chkClipboardAutoPaste_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                blckSettingNote.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {

               
            }
        }

        private void cmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;

            // 0 light
            // 1 dark
            // 2 instagram background

            if (AppSettings.Values["theme"].ToString()!= cmbTheme.SelectedIndex.ToString())
            {
                blckSettingNote.Visibility = Visibility.Visible;
            }
            AppSettings.Values["theme"] = cmbTheme.SelectedIndex.ToString();
        }
    }



}
