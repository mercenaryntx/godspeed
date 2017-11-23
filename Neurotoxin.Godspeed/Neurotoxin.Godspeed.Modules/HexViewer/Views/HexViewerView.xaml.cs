using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Neurotoxin.Godspeed.Modules.HexViewer.Controls;
using Neurotoxin.Godspeed.Modules.HexViewer.ViewModels;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Neurotoxin.Godspeed.Modules.HexViewer.Views
{
    /// <summary>
    /// Interaction logic for HexViewerView.xaml
    /// </summary>

    public partial class HexViewerView : ModuleViewBase
    {

        public HexViewerView(HexViewerViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void HexView_OnClearOverlay(object sender, ClearOverlayEventArgs args)
        {
            Rect1.Visibility = Visibility.Collapsed;
            Rect2.Visibility = Visibility.Collapsed;
            Rect3.Visibility = Visibility.Collapsed;
            Tooltip.Visibility = Visibility.Collapsed;
        }

        private void HexView_OnDrawOverlay(object sender, DrawOverlayEventArgs args)
        {
            Rectangle rect;
            switch (args.Id)
            {
                case 1:
                    rect = Rect1;
                    break;
                case 2:
                    rect = Rect2;
                    break;
                case 3:
                    rect = Rect3;
                    break;
                default:
                    throw new Exception("no rect: " + args.Id);
            }
            rect.Width = args.Rect.Width;
            rect.Height = args.Rect.Height;
            rect.SetValue(Canvas.LeftProperty, args.Rect.Left);
            rect.SetValue(Canvas.TopProperty, args.Rect.Top);
            rect.Visibility = Visibility.Visible;
        }

        private void HexView_OnTooltipOverlay(object sender, TooltipOverlayEventArgs args)
        {
            Tooltip.Visibility = Visibility.Visible;
            var s = (args.Entry.PropertyName != String.Empty) ? args.Entry.PropertyName + " - " : String.Empty;
            TooltipText.Text = s + args.Entry.ClassName;

            var top = (double) Rect1.GetValue(Canvas.TopProperty);
            top += top > 0 ? -1*Tooltip.ActualHeight : Rect1.ActualHeight;
            Tooltip.SetValue(Canvas.TopProperty, top);
            Tooltip.SetValue(Canvas.LeftProperty, Rect1.GetValue(Canvas.LeftProperty));
        }
    }
}