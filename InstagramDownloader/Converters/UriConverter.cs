using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace InstagramDownloader.Converters
{

    public class BitmapConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,string culture)
        {
            try
            {
                string fileName = value as string;
                var file = StorageFile.GetFileFromPathAsync(fileName).AsTask().Result;
                var stream = file.OpenReadAsync().AsTask().Result;
                var bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                bitmapImage.SetSource(stream);
                return bitmapImage;
            }
            catch (Exception)
            {
                return null;
            }
           
        }
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }



    }



    public class GetAlbumItemType : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            try
            {
                string posttype = value as string;
                StorageFile file = null;

                if (posttype==QuickType.Instagram__Typename.GraphImage.ToString())
                {
                    file = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/photo-thumb.png")).AsTask().Result;
                }
               else if (posttype == QuickType.Instagram__Typename.GraphVideo.ToString())
                {
                    file = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/video-thumb.png")).AsTask().Result;
                }
                var stream = file.OpenReadAsync().AsTask().Result;
                var bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                bitmapImage.SetSource(stream);
                return bitmapImage;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }



    }


    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public class ThumbnailVideos : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string FileName = value as string;
            var bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();

            try
            {
                try
                {
                    //string path = ApplicationData.Current.LocalCacheFolder.Path + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".jpg";
                    var file = ApplicationData.Current.LocalCacheFolder.GetFileAsync(Path.GetFileNameWithoutExtension(FileName) + ".jpg").AsTask().Result;
                   
                    var stream = file.OpenReadAsync().AsTask().Result;

                    bitmapImage.SetSource(stream);
                    if (bitmapImage != null)
                    {
                        return bitmapImage;
                    }
                    return null;
                }
                catch (Exception)
                {

                    var task =Task.Run(async () =>
                    {

                        await LoadVideo(FileName);
                 
                    });

                    return null;
                }


                
             

            }
            catch (Exception)
            {

                GC.Collect();
                return null;
            }


            //else
            //{



            //}
            //return null;

        }

     
        public async  Task LoadVideo(string videopath)
        {
            try
            {
          
                SoftwareBitmap softwareBitmap = null;

                //StorageFile imagefile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/logo1.png"));
                StorageFile videofile = await StorageFile.GetFileFromPathAsync(videopath);

                var thumbnail = await GetThumbnailAsync(videofile);

                Windows.Storage.Streams.InMemoryRandomAccessStream randomAccessStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                await Windows.Storage.Streams.RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);

                //using ( await Windows.Storage.Streams.RandomAccessStream.CopyAsync(thumbnail, randomAccessStream))
                //{
                //    // Create the decoder from the stream
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                //}
               
                StorageFile outputFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(Path.GetFileNameWithoutExtension(videopath) + ".jpg");
                using (Windows.Storage.Streams.IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Create an encoder with the desired format
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                    // Set the software bitmap
                    encoder.SetSoftwareBitmap(softwareBitmap);

                    // Set additional encoding parameters, if needed
                    encoder.BitmapTransform.ScaledWidth = 92;
                    encoder.BitmapTransform.ScaledHeight = 92;
                    //encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
                    //encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                    encoder.IsThumbnailGenerated = true;

                    try
                    {
                        await encoder.FlushAsync();
                        if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||  softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                        {
                            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        }

                        var source = new SoftwareBitmapSource();
                        await source.SetBitmapAsync(softwareBitmap);

                        // Set the source of the Image control
                        return;

                    }
                    catch (Exception err)
                    {
                        switch (err.HResult)
                        {
                            case unchecked((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
                                                             // If the encoder does not support writing a thumbnail, then try again
                                                             // but disable thumbnail generation.
                                encoder.IsThumbnailGenerated = false;
                                break;
                            default:
                                throw err;
                        }
                    }

                    if (encoder.IsThumbnailGenerated == false)
                    {
                        await encoder.FlushAsync();
                    }
                    //image.SetSource(fileStream);

                }
                return;
            }
            //    return image;
            //});
            //     return image;

            catch (Exception)
            {
                return;
             
            }
           finally
            {
                
                
                GC.Collect();
            }
           

        }
        public async Task<Windows.Storage.Streams.IInputStream> GetThumbnailAsync(StorageFile file)
        {
            try
            {
                var mediaClip = await Windows.Media.Editing.MediaClip.CreateFromFileAsync(file);
                var mediaComposition = new Windows.Media.Editing.MediaComposition();
                mediaComposition.Clips.Add(mediaClip);
                return await mediaComposition.GetThumbnailAsync(
                    TimeSpan.Zero, 0, 0, Windows.Media.Editing.VideoFramePrecision.NearestFrame);
            }
            catch (Exception)
            {

                return null;
            }
          
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

      
    }

}
