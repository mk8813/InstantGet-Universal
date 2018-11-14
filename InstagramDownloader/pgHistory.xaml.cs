using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Popups;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.ApplicationModel.Resources;
using DbHelper;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InstagramDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgHistory : Page
    {
        ObservableCollection<tbl_History> LstPictures=new ObservableCollection<tbl_History>();
        ObservableCollection<tbl_History> LstVideos = new ObservableCollection<tbl_History>();
        ResourceLoader res = ResourceLoader.GetForCurrentView();

        public pgHistory()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            Loaded += PgHistory_Loaded;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += OnHardwareButtonsBackPressed;
            }

        }



        private void OnHardwareButtonsBackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            try
            {
                e.Handled = true;

                if (grdPicturesHistory.SelectionMode==ListViewSelectionMode.Multiple || grdVideosHistory.SelectionMode==ListViewSelectionMode.Multiple)
                {
                    btnClearSElection_Click(null, null);
                }
                else if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
            catch (Exception)
            {

              
            }

        }
        private async void PgHistory_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
          
                bool result = false;
                prgLoadingPictures.Visibility = Visibility.Visible;
                result = false;
                prgLoadingVideos.Visibility = Visibility.Visible;

                ////////////////////////////////////////////// load pictures
                await System.Threading.Tasks.Task.Run(async () => result = await Loadimages());

                if (result)
                {
                    grdPicturesHistory.ItemsSource = LstPictures;
                }
                prgLoadingPictures.Visibility = Visibility.Collapsed;


                ///////////////////////////////////////////// load videos

                await System.Threading.Tasks.Task.Run(async () => result = await LoadVideos());
                if (result)
                {
                    grdVideosHistory.ItemsSource = LstVideos;
                }
                prgLoadingVideos.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message + ex.StackTrace).ShowAsync();

            }


        }
    

        private async Task<bool> Loadimages()
        {
            try
            {
                using (dbHelperConnection db = new dbHelperConnection())
                {
                  
                    var res = db.tbl_History.Where(p => p.Type == "picture").Select(p => p).OrderByDescending(p => p.Id);

                    if (res.Count() > 0)
                    {
                        LstPictures.Clear();
                        LstPictures.AddRange(res);
                        //foreach (var item in res)
                        //{

                        //    LstPictures.Add(item);

                        //}

                        return true;


                    }
                    else
                    {

                        return false;
                    }
                }
            }
            catch (Exception)
            {
             //  await new MessageDialog(ex.Message + ex.StackTrace).ShowAsync();
                return false;
            }
           
        }


        private async Task<bool> LoadVideos()
        {
            try
            {
                using (dbHelperConnection db = new dbHelperConnection())
                {
                 
                    var res = db.tbl_History.Where(p => p.Type == "video").Select(p => p).OrderByDescending(p => p.Id);
                    if (res.Count() > 0)
                    {
                        LstVideos.Clear();
                        LstVideos.AddRange(res);
                        //foreach (var item in res)
                        //{

                        //    LstVideos.Add(item);

                        //}

                        return true;


                    }
                    else
                    {

                        return false;
                    }
                }
            }
            catch (Exception)
            {

                return false;
            }

        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pivotHistory.SelectedIndex==0)
                {
                    grdPicturesHistory.SelectionMode = ListViewSelectionMode.Multiple;
                    grdPicturesHistory.IsMultiSelectCheckBoxEnabled = true;
                    btnSelectGrid.Visibility = Visibility.Collapsed;
                    btnDeleteGrid.Visibility = Visibility.Visible;
                    btnDeleteGrid.IsEnabled = false;
                    btnClearSElection.Visibility = Visibility.Visible;
                }
                else if (pivotHistory.SelectedIndex==1)
                {
                    grdVideosHistory.SelectionMode = ListViewSelectionMode.Multiple;
                    grdVideosHistory.IsMultiSelectCheckBoxEnabled = true;
                    btnSelectGrid.Visibility = Visibility.Collapsed;
                    btnDeleteGrid.Visibility = Visibility.Visible;
                    btnDeleteGrid.IsEnabled = false;
                    btnClearSElection.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {

                
            }
        }


        private string ClickedFilePath = "";
     

        private void Grid_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
           
           
        }

        private void grdPicturesHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (grdPicturesHistory.SelectedItems.Count()>0)
            {
                btnDeleteGrid.IsEnabled = true;
            }
            else
            {
                btnDeleteGrid.IsEnabled = false;
            }
        }

        private void btnClearSElection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pivotHistory.SelectedIndex == 0)
                {
                    grdPicturesHistory.SelectionMode = ListViewSelectionMode.Single;
                    grdPicturesHistory.IsMultiSelectCheckBoxEnabled = false;
                    btnSelectGrid.Visibility = Visibility.Visible;
                    btnDeleteGrid.Visibility = Visibility.Collapsed;
               
                    btnClearSElection.Visibility = Visibility.Collapsed;
                }
                else if (pivotHistory.SelectedIndex == 1)
                {
                    grdVideosHistory.SelectionMode = ListViewSelectionMode.Single;
                    grdVideosHistory.IsMultiSelectCheckBoxEnabled = false;
                    btnSelectGrid.Visibility = Visibility.Visible;
                    btnDeleteGrid.Visibility = Visibility.Collapsed;

                    btnClearSElection.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {

               
            }
        }

        private void grdPicturesHistory_ItemClick_1(object sender, ItemClickEventArgs e)
        {

        }

        private void grdPicturesHistory_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
           
        }

        private void grdPicturesHistory_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            //try
            //{

            //    if (grdVideosHistory.SelectionMode== ListViewSelectionMode.Multiple || grdPicturesHistory.SelectionMode== ListViewSelectionMode.Multiple)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        var SelectedRecord = ((FrameworkElement)e.OriginalSource).DataContext as tbl_History;
            //        if (SelectedRecord != null)
            //        {

            //            ClickedFilePath = SelectedRecord.SavePath;
            //            try
            //            {
            //                if (!string.IsNullOrEmpty(ClickedFilePath))
            //                {

            //                    await Windows.System.Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(ClickedFilePath));

            //                }

            //            }
            //            catch (Exception)
            //            {


            //            }
            //        }
            //        else
            //        {

            //            ClickedFilePath = "";
            //        }

            //    }


            //}
            //catch (Exception)
            //{


            //}
        }

        private void grdPicturesHistory_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
           
        }

        private void grdPicturesHistory_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
          
        }


       
        private void grdPicturesHistory_PointerReleased_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
           
          
        }

        private void grdPicturesHistory_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
           
        }

        private string ID;
        private  void mnuCopyShareUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ID.Trim()))
                {
                    
                    using (dbHelperConnection db=new dbHelperConnection())
                    {
                        int id = int.Parse(ID);
                        var res = db.tbl_History.Where(p => p.Id == id).Select(p => p.Url);
                        if (res.Count()>0)
                        {
                            var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
                            dataPackage.SetText(res.First());
                            Clipboard.SetContent(dataPackage);

                        }
                    }
                }

            }
            catch (Exception)
            {


            }
        }

        private async void mnuDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ID.Trim()))
                {
                 
                    ContentDialog deleteFileDialog = new ContentDialog()
                    {

                        Content = res.GetString("DeleteThis"),
                        PrimaryButtonText = res.GetString("DeleteBtn"),
                        SecondaryButtonText = res.GetString("CancelBtn")
                    };

                    ContentDialogResult result = await deleteFileDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        using (dbHelperConnection db = new dbHelperConnection())
                        {
                            int id = int.Parse(ID);
                            var res = db.tbl_History.Where(p => p.Id == id).Select(p => p);

                            if (res.Count() > 0)
                            {
                                string imagepath = res.First().SavePath;
                                db.tbl_History.Remove(res.First());

                                if (db.SaveChanges() > 0)
                                {
                                    try
                                    {
                                        StorageFile fs = await StorageFile.GetFileFromPathAsync(imagepath);
                                        if (fs != null)
                                        {
                                            await fs.DeleteAsync();


                                        }
                                    }
                                    catch (Exception)
                                    {


                                    }
                                    if (pivotHistory.SelectedIndex==0)
                                    {
                                        await Loadimages();
                                    }
                                    else if (pivotHistory.SelectedIndex == 1)
                                    {
                                       await LoadVideos();
                                    }
                                 
                                }
                                else
                                {
                                    ///save db  failed

                                }

                            }
                        }
                    }
                        
                }

            }
            catch (Exception)
            {

              
            }
        }

      
        private async void btnDeleteGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
              
                #region grdpictures
                if (pivotHistory.SelectedIndex == 0)
                {
                    if (grdPicturesHistory.SelectedItems.Count != 0)
                    {
                        string caption = "";
                        if (grdPicturesHistory.SelectedItems.Count() > 1)
                        {
                            caption = res.GetString("DeleteThese");
                        }
                        else
                        {
                            caption =res.GetString("DeleteThis");
                        }
                        ContentDialog deleteFileDialog = new ContentDialog()
                        {

                            Content = caption,
                            PrimaryButtonText = res.GetString("DeleteBtn"),
                            SecondaryButtonText = res.GetString("CancelBtn")
                        };

                        ContentDialogResult result = await deleteFileDialog.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            foreach (var item in grdPicturesHistory.SelectedItems)
                            {
                                tbl_History tempItem = (tbl_History)item;
                                string path = tempItem.SavePath;
                                using (dbHelperConnection db = new dbHelperConnection())
                                {
                                    db.tbl_History.Remove(tempItem);
                                    if (db.SaveChanges() > 0)
                                    {
                                        try
                                        {
                                            StorageFile fs = await StorageFile.GetFileFromPathAsync(path);
                                            if (fs != null)
                                            {
                                                await fs.DeleteAsync();


                                            }
                                        }
                                        catch (Exception)
                                        {


                                        }


                                    }
                                }
                            }


                            btnReloadList_Click(null, null);



                        }


                    }
                }
                #endregion grdpictures
                #region grdvideos
                else if (pivotHistory.SelectedIndex == 1)
                {
                    if (grdVideosHistory.SelectedItems.Count != 0)
                    {
                        string caption = "";
                        if (grdVideosHistory.SelectedItems.Count() > 1)
                        {
                            caption = res.GetString("DeleteThese");
                        }
                        else
                        {
                            caption = res.GetString("DeleteThis");
                        }
                        ContentDialog deleteFileDialog = new ContentDialog()
                        {

                            Content = caption,
                            PrimaryButtonText = res.GetString("DeleteBtn"),
                            SecondaryButtonText = res.GetString("CancelBtn")
                        };

                        ContentDialogResult result = await deleteFileDialog.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            foreach (var item in grdVideosHistory.SelectedItems)
                            {
                                tbl_History tempItem = (tbl_History)item;
                                string path = tempItem.SavePath;
                                using (dbHelperConnection db = new dbHelperConnection())
                                {
                                    db.tbl_History.Remove(tempItem);
                                    if (db.SaveChanges() > 0)
                                    {
                                        try
                                        {
                                            StorageFile fs = await StorageFile.GetFileFromPathAsync(path);
                                            if (fs != null)
                                            {
                                                await fs.DeleteAsync();


                                            }
                                        }
                                        catch (Exception)
                                        {


                                        }


                                    }
                                }
                            }


                            btnReloadList_Click(null, null);



                        }

                    }
                }
                #endregion grdvideos
               
            }
            catch (Exception)
            {

                
            }
        }

        private void grdVideosHistory_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
           

        }

        private void grdVideosHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (grdVideosHistory.SelectedItems.Count() > 0)
                {
                    btnDeleteGrid.IsEnabled = true;
                }
                else
                {
                    btnDeleteGrid.IsEnabled = false;
                }
            }
            catch (Exception)
            {

             
            }
        }

        private void Image_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            try
            {
                if (grdVideosHistory.SelectionMode == ListViewSelectionMode.Multiple || grdPicturesHistory.SelectionMode == ListViewSelectionMode.Multiple)
                {
                    return;
                }

                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                {
                    var properties = e.GetCurrentPoint(this).Properties;

                    if (properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.RightButtonReleased)
                    {
                        if (pivotHistory.SelectedIndex==0)
                        {
                            mnuGridViewPictures.ShowAt(grdPicturesHistory, e.GetCurrentPoint(grdPicturesHistory).Position);
                        }
                        else if(pivotHistory.SelectedIndex==1)
                        {
                            mnuGridViewPictures.ShowAt(grdVideosHistory, e.GetCurrentPoint(grdVideosHistory).Position);
                        }
                     
                        var SelectedRecord = ((FrameworkElement)e.OriginalSource).DataContext as tbl_History;
                        if (SelectedRecord != null)
                        {
                            ID = SelectedRecord.Id.ToString();

                        }
                        else
                        {
                            ID = "";

                        }


                    }
                }


                //////////////touch


                else if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                {


                    // mnuGridViewPictures.ShowAt(grdPicturesHistory, e.GetCurrentPoint(grdPicturesHistory).Position);
                    var SelectedRecord = ((FrameworkElement)e.OriginalSource).DataContext as tbl_History;
                    if (SelectedRecord != null)
                    {
                        ID = SelectedRecord.Id.ToString();

                    }
                    else
                    {
                        ID = "";

                    }

                }
            }

            catch (Exception)
            {


            }

        }

        private async void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {

                if (grdVideosHistory.SelectionMode == ListViewSelectionMode.Multiple || grdPicturesHistory.SelectionMode == ListViewSelectionMode.Multiple)
                {
                    return;
                }
                else
                {
                    var SelectedRecord = ((FrameworkElement)e.OriginalSource).DataContext as tbl_History;
                    if (SelectedRecord != null)
                    {

                        ClickedFilePath = SelectedRecord.SavePath;
                        try
                        {
                            if (!string.IsNullOrEmpty(ClickedFilePath))
                            {

                                await Windows.System.Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(ClickedFilePath));
                                ClickedFilePath = "";

                            }

                        }
                        catch (Exception)
                        {


                        }
                    }
                    else
                    {

                        ClickedFilePath = "";
                    }

                }


            }
            catch (Exception)
            {


            }
        }

        private void Image_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            try
            {
                if (grdPicturesHistory.SelectionMode == ListViewSelectionMode.Multiple)
                {
                    return;
                }
                if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
                {
                    mnuGridViewPictures.ShowAt(grdPicturesHistory, e.GetPosition(grdPicturesHistory));
                    var SelectedRecord = ((FrameworkElement)e.OriginalSource).DataContext as tbl_History;
                    if (SelectedRecord != null)
                    {
                        ID = SelectedRecord.Id.ToString();

                    }
                    else
                    {
                        ID = "";

                    }
                }
            }
            catch (Exception)
            {

              
            }
         
        }

        private void ImageGridVideo_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            try
            {
                if (grdVideosHistory.SelectionMode == ListViewSelectionMode.Multiple)
                {
                    return;
                }
                if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
                {
                    mnuGridViewPictures.ShowAt(grdVideosHistory, e.GetPosition(grdVideosHistory));
                    var SelectedRecord = ((FrameworkElement)e.OriginalSource).DataContext as tbl_History;
                    if (SelectedRecord != null)
                    {
                        ID = SelectedRecord.Id.ToString();

                    }
                    else
                    {
                        ID = "";

                    }
                }
            }
            catch (Exception)
            {

            
            }
           
        }

        private void pivotHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivotHistory.SelectedIndex==0)//go to pictures
            {
                //disable multiselect on grid videos
                grdVideosHistory.SelectionMode = ListViewSelectionMode.Single;
                btnClearSElection_Click(null, null);

            }
            else if(pivotHistory.SelectedIndex==1)
            {
                grdPicturesHistory.SelectionMode = ListViewSelectionMode.Single;
                btnClearSElection_Click(null, null);
            }
        }

        private async void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ID.Trim()))
                {


                    using (dbHelperConnection db = new dbHelperConnection())
                    {
                        int id = int.Parse(ID);
                        var res = db.tbl_History.Where(p => p.Id == id).Select(p => p);
                        if (res.Count() > 0)
                        {
                            string loc = res.First().SavePath;
                            if (!string.IsNullOrEmpty(loc))
                            {
                                StorageFile file = await StorageFile.GetFileFromPathAsync(loc);

                                await Windows.System.Launcher.LaunchFolderAsync(await file.GetParentAsync());

                                /////////////////reload list
                                if (pivotHistory.SelectedIndex == 0)
                                {
                                  await  Loadimages();
                                }
                                else if (pivotHistory.SelectedIndex == 1)
                                {
                                   await LoadVideos();
                                }
                            }

                        }


                    }


                }
            }
            catch (Exception)
            {


            }
        }

     
        private async void btnReloadList_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (pivotHistory.SelectedIndex == 0)//reload pics
                {
                    prgLoadingPictures.Visibility = Visibility.Visible;
                    LstPictures.Clear();
                    
                   
                    await Loadimages();
                    prgLoadingPictures.Visibility = Visibility.Collapsed;
                }
                else if (pivotHistory.SelectedIndex == 1)//reload videos
                {
                    prgLoadingVideos.Visibility = Visibility.Visible;
                    LstVideos.Clear();
                 
                    await LoadVideos();
                    prgLoadingVideos.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {


            }
        }

       

      
    }
  
}
