using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Neurotoxin.Godspeed.Presentation.Extensions
{
    public static class UIElementExtensions
    {
        public static MemoryStream CaptureVisual(this UIElement source)
        {
            var h = source.RenderSize.Height;
            var w = source.RenderSize.Width;

            var renderTarget = new RenderTargetBitmap((int) w, (int) h, 96, 96, PixelFormats.Pbgra32);

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var sourceBrush = new VisualBrush(source);
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(w, h)));
            }
            renderTarget.Render(drawingVisual);
            
            var outputStream = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));
            encoder.Save(outputStream);
            outputStream.Flush();
            return outputStream;
        }
    }
}