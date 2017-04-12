using ImageProcessor.Imaging;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Sitecore.Resources.Media
{
    /// <summary>
    /// Utilizes the additional custom focus crop parameters to crop images.
    /// </summary>
    public class CropProcessor
    {
        private static readonly string[] IMAGE_EXTENSIONS = { "bmp", "jpeg", "jpg", "png", "gif" };

        public void Process(GetMediaStreamPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            
            var outputStream = args.OutputStream;
            if (outputStream == null)
            {
                return;
            }

            if (!IMAGE_EXTENSIONS.Any(i => i.Equals(args.MediaData.Extension, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var cx = args.Options.CustomOptions["cx"];
            var cy = args.Options.CustomOptions["cy"];
            var width = args.Options.CustomOptions["cw"];
            var height = args.Options.CustomOptions["ch"];
            float x, y;
            int w, h;

            if (!string.IsNullOrEmpty(cx) && !string.IsNullOrEmpty(cy) && float.TryParse(cx, out x) && float.TryParse(cy, out y) &&
                !string.IsNullOrEmpty(width) && Int32.TryParse(width, out w) && !string.IsNullOrEmpty(height) && Int32.TryParse(height, out h))
            {
                var outputStrm = Stream.Synchronized(GetCroppedImage(args.MediaData.Extension, w, h, x, y, outputStream.MediaItem));
                args.OutputStream = new MediaStream(outputStrm, args.MediaData.Extension, outputStream.MediaItem);
            }
            else if (args.Options.Thumbnail)
            {
                TransformationOptions transformationOptions = args.Options.GetTransformationOptions();
                MediaStream thumbnailStream = args.MediaData.GetThumbnailStream(transformationOptions);

                if (thumbnailStream != null)
                {
                    args.OutputStream = thumbnailStream;
                }
            }
        }

        private Stream GetCroppedImage(string extension, int width, int height, float cx, float cy, MediaItem mediaItem)
        {
            var outputStrm = new MemoryStream();
            var mediaStrm = mediaItem.GetMediaStream();
            var img = Image.FromStream(mediaStrm);
            var proc = new ImageProcessor.ImageFactory();
            proc.Load(img);

            var axis = new float[] { cy, cx };
            proc = proc.Resize(new ResizeLayer(new Size(width, height), ResizeMode.Crop, AnchorPosition.Center, true, centerCoordinates: axis));
            proc.Save(outputStrm);

            return outputStrm;
        }
    }
}