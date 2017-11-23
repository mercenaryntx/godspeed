using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Neurotoxin.Godspeed.Presentation.Controls
{
    /// <summary>
    /// An image that supports multiple sizes depending on its own rendering size.
    /// </summary>
    /// <remarks>
    /// Extends the standard <see cref="Image"/> control with the functionality to 
    /// load all the available frames in its <see cref="Image.Source"/> and render the one
    /// that best matches the current rendering size of the control.
    /// 
    /// For example, if the Source is a TIFF file containing frames of sizes 24x24, 48x48 and 128x128
    /// and the image is rendered at size 40x40, the frame with resolution 48x48 is used.
    /// The same control with the same source rendered at 24x24 would use the 24x24 frame.
    /// 
    /// <para>Written by Isak Savo - isak.savo@gmail.com, (c) 2011-2012. Licensed under the Code Project  </para>
    /// </remarks>
    public class MultiSizeImage : Image
    {
        static MultiSizeImage()
        {
            // Tell WPF to inform us whenever the Source dependency property is changed
            SourceProperty.OverrideMetadata(typeof(MultiSizeImage), new FrameworkPropertyMetadata(HandleSourceChanged));
        }

        private static void HandleSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        { 
            MultiSizeImage img = (MultiSizeImage)sender;
            img.UpdateAvailableFrames();
        }
        /// <summary>
        /// List containing one frame of every size available in the original file. The frame 
        /// stored in this list is the one with the highest pixel depth for that size.
        /// </summary>
        private List<BitmapSource> _availableFrames = new List<BitmapSource>();

        /// <summary>
        /// Gets the pixel depth (in bits per pixel, bpp) of the specified frame
        /// </summary>
        /// <param name="frame">The frame to get BPP for</param>
        /// <returns>The number of bits per pixel in the frame</returns>
        private int GetFramePixelDepth(BitmapFrame frame)
        {
            if (frame.Decoder.CodecInfo.ContainerFormat == new Guid("{a3a860c4-338f-4c17-919a-fba4b5628f21}")
                && frame.Thumbnail != null)
            {
                // Windows Icon format, original pixel depth is in the thumbnail
                return frame.Thumbnail.Format.BitsPerPixel;
            }
            else
            {
                // Other formats, just assume the frame has the correct BPP info
                return frame.Format.BitsPerPixel;
            }
        }

        /// <summary>
        /// Scans the ImageSource for available frames and stores 
        /// them as individual bitmap sources. This is done once, 
        /// when the <see cref="Image.Source"/> property is set (or changed)
        /// </summary>
        private void UpdateAvailableFrames()
        {
            _availableFrames.Clear();
            BitmapFrame bmFrame = Source as BitmapFrame;
            if (bmFrame == null)
                return;
            
            var decoder = bmFrame.Decoder;
            if (decoder != null && decoder.Frames != null)
            {
                var framesInSizeOrder = from frame in decoder.Frames
                                        group frame by frame.PixelHeight * frame.PixelWidth into g
                                        orderby g.Key
                                        select new
                                            {
                                                Size = g.Key,
                                                Frames = g.OrderByDescending(GetFramePixelDepth)
                                            };
                _availableFrames.AddRange(framesInSizeOrder.Select(group => group.Frames.First()));
            }
        }

        /// <summary>
        /// Renders the contents of the image
        /// </summary>
        /// <param name="dc">An instance of <see cref="T:System.Windows.Media.DrawingContext"/> used to render the control.</param>
        protected override void OnRender(DrawingContext dc)
        {
            if (Source == null)
            {
                base.OnRender(dc);
                return;
            }
            ImageSource src = Source;
            var ourSize = RenderSize.Width * RenderSize.Height;
            foreach (var frame in _availableFrames)
            {
                src = frame;
                if (frame.PixelWidth * frame.PixelHeight >= ourSize)
                    break;
            }
            if (src is BitmapSource)
            {
                var bs = (BitmapSource)src;
                Console.WriteLine("Rendering frame of size {0}x{1}", bs.PixelWidth, bs.PixelHeight);
            }
            dc.DrawImage(src, new Rect(new Point(0, 0), RenderSize));
        }
    }
}
