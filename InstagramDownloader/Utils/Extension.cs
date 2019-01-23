using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;

namespace InstagramDownloader
{
    public static class Extension
    {
        public static async Task<IRandomAccessStream> RenderToRandomAccessStream(this UIElement element)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(element);

            var pixelBuffer = await rtb.GetPixelsAsync();
            //System.Diagnostics.Debug.WriteLine($"Capacity = {pixelBuffer.Capacity}, Length={pixelBuffer.Length}");
            //////////////////////////
            var buffer = await rtb.GetPixelsAsync();
            DataReader reader = DataReader.FromBuffer(buffer);
            byte[] pixels = new byte[buffer.Length];
            reader.ReadBytes(pixels);
            /////////////////////
            //var pixels = pixelBuffer.ToArray();
            var displayInformation = DisplayInformation.GetForCurrentView();

            var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                 BitmapAlphaMode.Premultiplied,
                                 (uint)rtb.PixelWidth,
                                 (uint)rtb.PixelHeight,
                                 displayInformation.RawDpiX,
                                 displayInformation.RawDpiY,
                                 pixels);

            await encoder.FlushAsync();
            stream.Seek(0);

            return stream;
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            items.ToList().ForEach(collection.Add);
        }
    }

    public static class BlurElement
    {
        public static async Task<BitmapImage> BlurThisUI(UIElement sourceElement)
        {

            ////////////////////////
           
            ///////////////////////
            using (var stream = await sourceElement.RenderToRandomAccessStream())
            {
                var device = new CanvasDevice();
                var bitmap = await CanvasBitmap.LoadAsync(device, stream);

                var renderer = new CanvasRenderTarget(device,
                                                      bitmap.SizeInPixels.Width,
                                                      bitmap.SizeInPixels.Height,
                                                      bitmap.Dpi);

                using (var ds = renderer.CreateDrawingSession())
                {
                    var blur = new GaussianBlurEffect();
                    blur.BlurAmount = 5.0f;

                    blur.Source = bitmap;
                    ds.DrawImage(blur);
                }

                stream.Seek(0);
                await renderer.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                Windows.UI.Xaml.Media.Imaging.BitmapImage image = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                image.SetSource(stream);

                ///////////////////////save bitmap
                //BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                //SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                //Windows.Storage.StorageFile file_Save = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync("app-bg.png", Windows.Storage.CreationCollisionOption.ReplaceExisting);

                //BitmapEncoder encoder = await BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, await file_Save.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite));

                //encoder.SetSoftwareBitmap(softwareBitmap);

                //await encoder.FlushAsync();


                ///////////////////////////
                return image;
       
            }
        }

    }
}
