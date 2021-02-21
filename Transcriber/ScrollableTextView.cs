using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Eliason.Common;
using Eliason.Scrollbar;
using Eliason.TextEditor;
using Eliason.TextEditor.Native;
using Eliason.TextEditor.TextView;

namespace Transcriber
{
    public class ScrollableTextView : TextView, IScrollHost
    {
        private static readonly Dictionary<string, SafeHandleGDI> staticCachedScrollbarImages =
            new Dictionary<string, SafeHandleGDI>();

        private readonly AdvScrollableControl _scrollableControl;

        public ScrollableTextView(ITextDocument textDocument, ISettings settings,
            AdvScrollableControl scrollableControl)
            : base(textDocument, settings)
        {
            _scrollableControl = scrollableControl;
            ScrollHost = this;
        }

        private AdvVScrollbar VerticalScrollbar => _scrollableControl.VerticalScroll;

        private AdvHScrollbar HorizontalScrollbar => _scrollableControl.HorizontalScroll;

        public void OnContentSizeChanged(int width, int height)
        {
            VerticalScrollbar.Maximum = height;
            HorizontalScrollbar.Maximum = width;

            VerticalScrollbar.Invalidate();
            HorizontalScrollbar.Invalidate();
        }

        public void Attach()
        {
            VerticalScrollbar.PaintBackgroundOverlay += VScroll_PaintBackgroundOverlay;
        }

        public void Detach()
        {
        }

        public int ScrollPosH => HorizontalScrollbar.Value;

        public int ScrollPosVIntegral => VerticalScrollbar.ValueIntegral;

        public int HorizontalMax { get; set; }

        public int VerticalMax { get; set; }

        public bool IsScrollingHorizontally => HorizontalScrollbar.IsScrolling;

        public bool IsScrollingVertically => VerticalScrollbar.IsScrolling;

        public event EventHandler<ValueChangedEventArgs> VerticalScrollChanged
        {
            add => VerticalScrollbar.ValueChanged += value;
            remove => VerticalScrollbar.ValueChanged -= value;
        }

        public event EventHandler<ValueChangedEventArgs> HorizontalScrollChanged
        {
            add => HorizontalScrollbar.ValueChanged += value;
            remove => HorizontalScrollbar.ValueChanged -= value;
        }

        /// <summary>
        ///     TODO: Remove
        /// </summary>
        /// <param name="p"></param>
        /// <param name="force"></param>
        /// <param name="ignoreHorizontalMovement"></param>
        /// <param name="cause"></param>
        public void ScrollToPoint(Point p, bool force = false, bool ignoreHorizontalMovement = false,
            ValueChangedBy cause = ValueChangedBy.Unspecified)
        {
            _scrollableControl.ScrollToPoint(p, force, ignoreHorizontalMovement);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (_scrollableControl != null)
                if (VerticalScrollbar != null)
                {
                    VerticalScrollbar.SmallChange = LineHeight;
                    VerticalScrollbar.LargeChange = LineHeight * SystemInformation.MouseWheelScrollLines;
                }

            base.OnLayout(levent);
        }

        private void VScroll_PaintBackgroundOverlay(object sender, PaintScrollbarBackgroundArgs e)
        {
            // TODO: Cache the painting to be painted through BltBit instead.
            // TODO: Add a way of making just a certain set of styles be painted to the scrollbar.

            var vRect = VerticalScrollbar.TrackRectangle; // _textView. VerticalScroll.ClientRectangle;
            vRect.Inflate(-1, 0);

            var ratio = vRect.Height / (float) VerticalMax; // _textView.VerticalScroll.Maximum;
            var yList = new List<int>();

            foreach (var segment in TextDocument.TextSegmentStyledManager.GetStyledTextSegments())
            {
                var rsi = segment.Style.GetNaturalRenderColors(this);
                if (rsi == null) continue;

                var loc = GetVirtualPositionFromCharIndex(segment.IndexGlobal);
                var y = loc.Y * ratio;

                // -1 for the width, since otherwise it paints over the scrollbar border.
                var markerRect = new RectangleF(vRect.Left + 1, vRect.Top + y, vRect.Width - 1,
                    Math.Max(1, LineHeight * ratio));
                var markerBrushRect = new RectangleF(vRect.Left, vRect.Top + y, vRect.Width, markerRect.Height);

                if (yList.Contains((int) markerRect.Y)) continue;

                yList.Add((int) markerRect.Y);

                var hidden = markerRect.IntersectsWith(e.ScrollbarRect);

                if (hidden || staticCachedScrollbarImages.ContainsKey(segment.Style.NameKey) == false)
                {
                    var bmpW = (int) Math.Max(1, markerRect.Width);
                    const int bmpH = 1;

                    Bitmap tempBmp = null;

                    try
                    {
                        if (hidden == false) tempBmp = new Bitmap(bmpW, bmpH);

                        Graphics g = null;

                        try
                        {
                            g = hidden ? Graphics.FromHdc(e.GraphicsHandle) : Graphics.FromImage(tempBmp);
                            g.InterpolationMode = InterpolationMode.NearestNeighbor;

                            var cStart = rsi.BackColor != -1
                                ? ColorTranslator.FromWin32(rsi.BackColor)
                                : Color.Transparent;

                            var cEnd = rsi.ForeColor != -1
                                ? ColorTranslator.FromWin32(rsi.ForeColor)
                                : Color.FromArgb(
                                    175,
                                    new Random(segment.Style.NameKey.GetHashCode() + 1).Next(0, 255),
                                    new Random(segment.Style.NameKey.GetHashCode() + 2).Next(0, 255),
                                    new Random(segment.Style.NameKey.GetHashCode() + 3).Next(0, 255));

                            if (hidden)
                            {
                                cStart = Color.FromArgb(Math.Max(25, cStart.A / 4), cStart);
                                cEnd = Color.FromArgb(Math.Max(25, cEnd.A / 4), cEnd);
                            }

                            var paintMarkerRect = hidden ? markerRect : new RectangleF(0, 0, markerRect.Width, bmpH);
                            var paintBrushMarkerRect =
                                hidden ? markerBrushRect : new RectangleF(0, 0, markerBrushRect.Width, bmpH);

                            using (var b = new LinearGradientBrush(paintBrushMarkerRect, cStart, cEnd, 0f))
                            {
                                g.FillRectangle(b, paintMarkerRect);
                            }
                        }
                        finally
                        {
                            g.Dispose();
                            g = null;
                        }

                        if (hidden == false)
                            staticCachedScrollbarImages.Add(segment.Style.NameKey,
                                new SafeHandleGDI(tempBmp.GetHbitmap()));
                    }
                    finally
                    {
                        if (hidden == false)
                            if (tempBmp != null)
                            {
                                tempBmp.Dispose();
                                tempBmp = null;
                            }
                    }
                }

                if (hidden == false)
                {
                    var bmpHandle = staticCachedScrollbarImages[segment.Style.NameKey];

                    var bmpHdc = SafeNativeMethods.CreateCompatibleDC(e.GraphicsHandle);
                    var previousBmpHandle = SafeNativeMethods.SelectObject(bmpHdc, bmpHandle.DangerousGetHandle());

                    SafeNativeMethods.StretchBlt(
                        e.GraphicsHandle,
                        (int) Math.Round(markerRect.Left), (int) Math.Round(markerRect.Top),
                        (int) Math.Round(markerRect.Width), (int) Math.Round(Math.Max(1, markerRect.Height)),
                        bmpHdc,
                        0, 0, (int) markerRect.Width, 1,
                        NativeConstants.SRCCOPY);

                    SafeNativeMethods.SelectObject(bmpHdc, previousBmpHandle);
                    SafeNativeMethods.DeleteDC(bmpHdc);
                }
            }
        }
    }
}