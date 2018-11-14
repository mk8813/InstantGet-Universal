﻿using QuickType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class dlgAlbumPost : ContentDialog
    {
        bool __isVideo = false;
        string selectedNode;
        string postId;
        bool isFullSized;
        private List<StickyNode> __lstAlbumPost;
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

        public string SelectedNode
        {
            get
            {
                return selectedNode;
            }

            set
            {
                selectedNode = value;
            }
        }

        public bool IsFullSized
        {
            get
            {
                return isFullSized;
            }

            set
            {
                isFullSized = value;
            }
        }

        public bool IsVideo
        {
            get
            {
                return __isVideo;
            }

            set
            {
                __isVideo = value;
            }
        }

        public string PostId
        {
            get
            {
                return postId;
            }

            set
            {
                postId = value;
            }
        }

        public dlgAlbumPost()
        {
            this.InitializeComponent();
            this.Loaded += DlgAlbumPost_Loaded;
        }

        private void DlgAlbumPost_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= DlgAlbumPost_Loaded;
            try
            {
                if (__lstAlbumPost!=null)
                {
                    flpAlbumlist.ItemsSource = __lstAlbumPost;
                    flpAlbumlist.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                if (flpAlbumlist.SelectedIndex != -1)
                {
                    var SelectedRecord = (flpAlbumlist.SelectedItem) as StickyNode;
                    if (SelectedRecord != null)
                    {
                        postId = SelectedRecord.Shortcode;
                        if (SelectedRecord.Typename==Instagram__Typename.GraphImage.ToString())
                        {
                            IsVideo = false;
                            if (isFullSized)//hd image url
                            {
                                selectedNode = SelectedRecord.DisplayUrl;
                            }
                            else // sd image url
                            {
                                selectedNode = SelectedRecord.DisplayResources[0].Src;
                            }
                        }
                        else if(SelectedRecord.Typename == Instagram__Typename.GraphVideo.ToString())//video
                        {
                            IsVideo = true;
                            selectedNode = SelectedRecord.VideoUrl;
                        }
                       

                    }
                    else
                    {
                        selectedNode = "";

                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void flpAlbumlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                lblItemNum.Text =flpAlbumlist.SelectedIndex+1 +"/" +flpAlbumlist.Items.Count;
            }
            catch (Exception)
            {

              
            }
        }

    

        private async void btnBrowsePostItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var SelectedRecord = (flpAlbumlist.SelectedItem) as StickyNode;
            if (SelectedRecord.Shortcode!=null)
            {
                // (Window.Current.Content as Frame)?.Navigate(typeof(pgBrowser), "https://instagram.com/p/"+SelectedRecord.Shortcode);
                if (SelectedRecord.IsVideo)
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(SelectedRecord.VideoUrl));
                }
                else
                {
                    if (isFullSized)
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri(SelectedRecord.DisplayUrl));
                    }
                    else
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri(SelectedRecord.DisplayResources[0].Src));
                    }
                }
               
            }
           
        }
    }
}
