using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using Neurotoxin.Godspeed.Core.Models;

namespace Neurotoxin.Godspeed.Modules.HexViewer.Controls
{
	/// <summary>
	/// Hexadecimal data rendering element
	/// </summary>
	/// <remarks>
	/// Used internally by HexViewer control
	/// 
	/// FontStyle and FontStretch properties are omitted as not appropriate for use with Hex Viewer.
	/// </remarks>
	internal class HexView : FrameworkElement
	{
		// total input data byte count
		private int m_byteCount = 0;
		// HexView line length
		private const int m_lineLength = 78;
		// Characte size
		private Size m_charSize;
		// Defaul Font Family
		private const string m_fontFamilyName = "GlobalMonospace.CompositeFont, Courier New, Courier, Lucida Console";

		static HexView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(HexView), new FrameworkPropertyMetadata(typeof(HexView)));

			DataProperty = DependencyProperty.Register("Data", typeof(IEnumerable<byte>), typeof(HexView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnDataChanged));
            MapProperty = DependencyProperty.Register("Map", typeof(BinMap), typeof(HexView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnMapChanged));
            PercentageProperty = DependencyProperty.Register("Percentage", typeof(float), typeof(HexView));
            ResultProperty = DependencyProperty.Register("Result", typeof(string), typeof(HexView));

			BackgroundProperty = Control.BackgroundProperty.AddOwner(typeof(HexView));
			ForegroundProperty = Control.ForegroundProperty.AddOwner(typeof(HexView));
			PaddingProperty = Control.PaddingProperty.AddOwner(typeof(HexView));
			FontFamilyProperty = Control.FontFamilyProperty.AddOwner(typeof(HexView), new FrameworkPropertyMetadata(OnFontFamilyChanged, OnFontFamilyCoerce));
			FontSizeProperty = Control.FontSizeProperty.AddOwner(typeof(HexView));
			FontWeightProperty = Control.FontWeightProperty.AddOwner(typeof(HexView));
		}

		#region Dependency properties

		public static readonly DependencyProperty DataProperty;
		public IEnumerable<byte> Data
		{
			get { return (IEnumerable<byte>)GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}
		private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			HexView ths = d as HexView;

			IEnumerable<byte> data = ths.Data;
			if (data == null)
				ths.m_byteCount = 0;
			else
			{ // get total byte count
				ICollection<byte> coll = data as ICollection<byte>;
				if (coll != null)
					ths.m_byteCount = coll.Count;
				else
				{
					IEnumerator<byte> en = data.GetEnumerator();
					ths.m_byteCount = 0;
					while (en.MoveNext())
						++ths.m_byteCount;
				}
			}
		}

        public static readonly DependencyProperty MapProperty;
        public BinMap Map
        {
            get { return (BinMap)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }
        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty PercentageProperty;
        public float Percentage
        {
            get { return (float)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty ResultProperty;
        public string Result
        {
            get { return (string)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

		public static readonly DependencyProperty BackgroundProperty;
		public Brush Background
		{
			get { return (Brush)GetValue(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}

		public static readonly DependencyProperty ForegroundProperty;
		public Brush Foreground
		{
			get { return (Brush)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); }
		}

		public static readonly DependencyProperty PaddingProperty;
		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingProperty); }
			set { SetValue(PaddingProperty, value); }
		}

		public static readonly DependencyProperty FontFamilyProperty;
		public FontFamily FontFamily
		{
			get { return (FontFamily)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}
		private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Trace.WriteLine("HexView.OnFontFamilyChanged");
		}
		private static object OnFontFamilyCoerce(DependencyObject d, Object baseValue)
		{
			HexView hv = d as HexView;
			FontFamily ff = baseValue as FontFamily;
			if (hv.IsFixedPitch(ff))
				return baseValue;// proposed FontFamily ok
			else
				return new FontFamily(m_fontFamilyName);// return default font family
		}

		public static readonly DependencyProperty FontSizeProperty;
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public static readonly DependencyProperty FontWeightProperty;
		public FontWeight FontWeight
		{
			get { return (FontWeight)GetValue(FontWeightProperty); }
			set { SetValue(FontWeightProperty, value); }
		}

		#endregion Dependency properties

		protected override void OnInitialized(EventArgs e)
		{
			if (!IsFixedPitch(FontFamily))
				// Set Fixed Pitch Font Family
				FontFamily = new FontFamily(m_fontFamilyName);
            MouseMove += HexViewMouseMove;
		}

	    public event ClearOverlayEvent ClearOverlay;
        public event DrawOverlayEvent DrawOverlay;
	    public event TooltipOverlayEvent TooltipOverlay;

        private void OnClearOverlay()
        {
            var e = ClearOverlay;
            if (e != null) e.Invoke(this, new ClearOverlayEventArgs());
        }

        private void OnDrawOverlay(int id, Rect rect)
        {
            var e = DrawOverlay;
            if (e != null) e.Invoke(this, new DrawOverlayEventArgs(id, rect));
        }

        private void OnTooltipOverlay(BinMapEntry entry)
        {
            var e = TooltipOverlay;
            if (e != null) e.Invoke(this, new TooltipOverlayEventArgs(entry));
        }

        private void HexViewMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (pos.X >= _leftBound && pos.X <= _rightBound)
            {
                var x = pos.X - _leftBound;
                var col = (int)(Math.Floor(Math.Floor(x / _boundWidth * 47)/3));
                var row = (int)Math.Floor(pos.Y / ActualHeight * _lineCount);
                var entry = Map.Get(row*16 + col);
                
                OnClearOverlay();

                if (entry == null) return;

                var start = GetPos(entry.Item1);
                var end = GetPos(entry.Item1 + entry.Item2.Length.Value, true);

                if (end.Y == start.Y)
                {
                    var rect = new Rect(GetPoint(start), GetPoint(end, true));
                    OnDrawOverlay(1, rect);
                } 
                else
                {
                    var rect = new Rect(GetPoint(start), GetPoint(16, start.Y, true));
                    OnDrawOverlay(1, rect);
                    if (end.Y > start.Y + 1)
                    {
                        var rect2 = new Rect(GetPoint(0, start.Y + 1), GetPoint(16, end.Y - 1, true));
                        OnDrawOverlay(2, rect2);
                    }
                    var rect3 = new Rect(GetPoint(0, end.Y), GetPoint(end, true));
                    OnDrawOverlay(3, rect3);
                }

                OnTooltipOverlay(entry.Item2);
            }
        }

        private Point GetPos(int offset, bool isEnd = false)
        {
            var row = offset/16;
            var col = offset%16;
            if (isEnd && col == 0)
            {
                col = 16;
                row -= 1;
            }

            return new Point(col, row);
        }

        private Point GetPoint(Point point, bool isEnd = false)
        {
            return GetPoint(point.X, point.Y, isEnd);
        }

        private Point GetPoint(double x, double y, bool isEnd = false)
        {
            x *= 3;
            if (isEnd) x -= 1;
            if (isEnd) y += 1;

            return new Point(Math.Floor(_leftBound + x * (_boundWidth / 47)), Math.Floor(y * ActualHeight / _lineCount));
        }

        private static DrawingVisual CreateRectangle(Rect rect)
        {
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            drawingContext.DrawRectangle(Brushes.Red, null, rect);
            drawingContext.Close();

            return drawingVisual;
        }

	    protected override Size MeasureOverride(Size sizeAvailable)
		{
			if (Data == null)
				return new Size(4, 4);// nothing to render

			// total data line count
			int lineCount = m_byteCount / 16;
			if (m_byteCount % 16 > 0)
				lineCount++;

			// Find out size of character
			Typeface tf = new Typeface(FontFamily, FontStyles.Normal, FontWeight, FontStretches.Normal);
			FormattedText txt = new FormattedText("M"
				, CultureInfo.CurrentCulture
				, FlowDirection.LeftToRight
				, tf
				, FontSize
				, Foreground);
			m_charSize = new Size(txt.Width, txt.Height);
			double width = txt.Width * m_lineLength + Padding.Left + Padding.Right;
			double height = txt.Height * lineCount + Padding.Top + Padding.Bottom;

			return new Size(width + 4, height + 4);
		}

	    private double _leftBound;
	    private double _rightBound;
	    private double _boundWidth;
	    private int _lineCount;

		/// <summary>
		/// Render all input data as hex-formatted text lines
		/// </summary>
		/// <param name="dc"></param>
		protected override void OnRender(DrawingContext dc)
		{
            Stopwatch watch = new Stopwatch();
            watch.Start();
			// Draw background
			object objBackgroundBrush = ReadLocalValue(BackgroundProperty);
			if (!object.ReferenceEquals(objBackgroundBrush, DependencyProperty.UnsetValue))
			{
				dc.DrawRectangle(objBackgroundBrush as Brush, null, new Rect(new Point(0, 0), RenderSize));
			}

			if (Data == null)
				return;// nothing to render

			// total data line count
			_lineCount = m_byteCount / 16;
			if (m_byteCount % 16 > 0)
				_lineCount++;

			// loop by byte collection
			IEnumerator<byte> en = Data.GetEnumerator();
			double y = Padding.Top;
			Typeface tf = new Typeface(FontFamily, FontStyles.Normal, FontWeight, FontStretches.Normal);
		    float percentage = 0;
			for (int i = 0; i < _lineCount; ++i)
			{
			    var point = new Point(Padding.Left, (int) Math.Ceiling(y));
				var txt = new FormattedText(NextHexLine(i, en), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, FontSize, Foreground);
                if (i == 0)
                {
                    var tmp = txt.BuildHighlightGeometry(point, 14, 47);
                    _leftBound = tmp.Bounds.Left;
                    _rightBound = tmp.Bounds.Right;
                    _boundWidth = _rightBound - _leftBound;
                }

			    var b = Map.Highlight(i * 16, (i + 1) * 16);
                percentage += b.Count(x => x > 0);
                int? gon = null;
                var start = 0;
                var end = 0;
                for (var j = 0; j < 16; j++)
                {
                    if (gon != null && (b[j] != gon || j == 15))
                    {
                        var c = gon.Value;
                        gon = null;
                        end = b[j] == 0 ? j - 1 : 15;
                        end = 16 + (end * 3);
                        var g = txt.BuildHighlightGeometry(point, start, end - start);
                        dc.DrawGeometry(c == 1 ? Brushes.Aquamarine : Brushes.DarkSalmon, null, g);
                    }
                    if (gon == null && b[j] > 0)
                    {
                        gon = b[j];
                        start = 14 + (j*3);
                    }
                }

				dc.DrawText(txt, point);
				y += txt.Height;
			}
            percentage = percentage * 100 / m_byteCount;
            Percentage = percentage;
            watch.Stop();
            Result = string.Format("Bytes: {0}, Rows: {1}, Load time: {3} ({2}%)", m_byteCount, _lineCount, percentage, watch.Elapsed);
		}
		/// <summary>
		/// Returns next formatted HexView line
		/// </summary>
		/// <param name="line_index">data buffer line index (0...); used to calculate line start address</param>
		/// <param name="en">data buffer enumerator</param>
		/// <returns>formatted line (length is asserted against m_lineLength)</returns>
		private string NextHexLine(int line_index, IEnumerator<byte> en)
		{
			StringBuilder sbHex = new StringBuilder(48);
			StringBuilder sbChar = new StringBuilder(16);
			int i = 0;
			while(i < 16 && en.MoveNext())
			{
				// Hexadecimal view
				sbHex.Append(String.Format("{0:X2}", en.Current));
				sbHex.Append(i == 7 ? "-" : " ");
				// Text view
				char ch = Convert.ToChar(en.Current);
				sbChar.Append(Char.IsControl(ch) ? "." : ch.ToString());

				++i;
			}
			if (i == 0)
				return string.Empty;

			// address pane
			int first = line_index * 16;
			StringBuilder sb = new StringBuilder(String.Format("{0:X6}-{1:X6} ", first, first + i - 1));

			for (; i < 16; ++i)
			{
				sbHex.Append("   ");
				sbChar.Append(" ");
			}

			sb.Append(sbHex);
			sb.Append(sbChar);

			Debug.Assert(sb.Length == m_lineLength);
			return sb.ToString();
		}
		/// <summary>
		/// Check if the FontFamily presents the Fixed Pitch Font.
		/// </summary>
		/// <remarks>Check is based on comarison of sizes of two characters: 'W' and 'I'</remarks>
		/// <param name="ff"></param>
		/// <returns>true if sizes of 'W' and 'I' are equal</returns>
		private bool IsFixedPitch(FontFamily ff)
		{
			Typeface tf = new Typeface(ff, FontStyles.Normal, FontWeight, FontStretches.Normal);
			FormattedText W = new FormattedText("W", CultureInfo.CurrentCulture
				, FlowDirection.LeftToRight, tf, FontSize, Foreground);
			FormattedText I = new FormattedText("I", CultureInfo.CurrentCulture
				, FlowDirection.LeftToRight, tf, FontSize, Foreground);

			return ((I.Width == W.Width) && (I.Height == W.Height));
		}
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			Point pt = e.GetPosition(this);
			pt.X -= Padding.Left;
			pt.Y -= Padding.Top;
			// line index
			int line = (int)(pt.Y / m_charSize.Height);
			// character index (all panes and their separators contributes here)
			int pos = (int)(pt.X / m_charSize.Width);
			Trace.WriteLine(String.Format("Character {1} at line {0} clicked", line, pos));

			// HexView screen layout
			// address pane:	pos = 0, len=9
			// separator:		pos = 9, len=1
			// hex pane:		pos = 10, len=47
			// separator:		pos = 57, len=1
			// text pane:		pos = 58, len=16
		}
	}

    internal delegate void TooltipOverlayEvent(object sender, TooltipOverlayEventArgs args);

    internal class TooltipOverlayEventArgs
    {
        public BinMapEntry Entry { get; set; }

        public TooltipOverlayEventArgs(BinMapEntry entry)
        {
            Entry = entry;
        }
    }

    internal delegate void ClearOverlayEvent(object sender, ClearOverlayEventArgs args);

    internal class ClearOverlayEventArgs : EventArgs
    {
    }

    internal delegate void DrawOverlayEvent(object sender, DrawOverlayEventArgs args);

    internal class DrawOverlayEventArgs : EventArgs
    {
        public int Id { get; private set; }
        public Rect Rect { get; private set; }

        public DrawOverlayEventArgs(int id, Rect rect)
        {
            Id = id;
            Rect = rect;
        }
    }
}
