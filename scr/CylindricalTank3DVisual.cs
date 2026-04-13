//-----------------------------------------------------------------------
// <copyright file="CylindricalTank3DVisual.cs" company="PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.">
//     Author: MUSTAFA CAGRI ALTINDAL
//     Copyright (c) PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DXApplication1
{
    [ToolboxItem(true)]
    public class CylindricalTank3DVisual : UserControl
    {
        private double _fillPercent = 60.0;
        private Color _fuelColor = Color.FromArgb(180, 180, 50);
        private double _waterPercent = 0.0;
        private bool _showPercentLabel = true;
        private bool _showLevelDot = true;
        private bool _showWaterDot = true;
        private bool _showReferenceLine = true;
        private bool _showProbe = true;
        private int _padding = 6;

        private static readonly Color ColWater = Color.FromArgb(85, 128, 198);

        public CylindricalTank3DVisual()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            Size = new Size(300, 160);
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }

        [Category("Tank")]
        [DefaultValue(60.0)]
        public double FillPercent
        {
            get { return _fillPercent; }
            set { _fillPercent = Clamp(value, 0.0, 100.0); Invalidate(); }
        }

        [Category("Tank")]
        public Color FuelColor
        {
            get { return _fuelColor; }
            set { _fuelColor = value; Invalidate(); }
        }

        [Category("Tank")]
        [DefaultValue(0.0)]
        public double WaterPercent
        {
            get { return _waterPercent; }
            set { _waterPercent = Clamp(value, 0.0, 100.0); Invalidate(); }
        }

        [Category("Tank")]
        [DefaultValue(true)]
        public bool ShowPercentLabel
        {
            get { return _showPercentLabel; }
            set { _showPercentLabel = value; Invalidate(); }
        }

        [Category("Tank")]
        [DefaultValue(true)]
        public bool ShowLevelDot
        {
            get { return _showLevelDot; }
            set { _showLevelDot = value; Invalidate(); }
        }

        [Category("Tank")]
        [DefaultValue(true)]
        public bool ShowWaterDot
        {
            get { return _showWaterDot; }
            set { _showWaterDot = value; Invalidate(); }
        }

        [Category("Tank")]
        [DefaultValue(true)]
        public bool ShowReferenceLine
        {
            get { return _showReferenceLine; }
            set { _showReferenceLine = value; Invalidate(); }
        }

        [Category("Tank")]
        [DefaultValue(true)]
        public bool ShowProbe
        {
            get { return _showProbe; }
            set { _showProbe = value; Invalidate(); }
        }

        [Category("Tank")]
        [DefaultValue(6)]
        public new int Padding
        {
            get { return _padding; }
            set { _padding = Math.Max(0, value); Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int x = _padding;
            int y = _padding;
            int w = ClientSize.Width - _padding * 2;
            int h = ClientSize.Height - _padding * 2;
            if (w < 24 || h < 24)
                return;

            Draw3DTank(g, x, y, w, h);
        }

        private void Draw3DTank(Graphics g, int x, int y, int w, int h)
        {
            TankGeometry geo = BuildGeometry(x, y, w, h);
            float fill = (float)(FillPercent / 100.0);
            float water = (float)(WaterPercent / 100.0);

            // 1. Drop shadow
            DrawShadow(g, geo);

            // 2. 3D metal body
            using (GraphicsPath body = BuildBodyPath(geo))
            using (LinearGradientBrush bodyBrush = new LinearGradientBrush(
                new PointF(geo.BodyBounds.Left, geo.BodyBounds.Top),
                new PointF(geo.BodyBounds.Left, geo.BodyBounds.Bottom),
                Color.FromArgb(105, 108, 114),
                Color.FromArgb(52, 55, 61)))
            {
                g.FillPath(bodyBrush, body);
            }

            // 3. Rear dome cap
            DrawDomeCap(g, geo.RearCap, true);

            Region oldClip = g.Clip;
            using (Region tankClip = BuildTankRegion(geo))
            {
                g.SetClip(tankClip, CombineMode.Replace);

                // 4. Fuel fill
                float minY = Math.Min(geo.RearCap.Top, geo.FrontCap.Top);
                float maxY = Math.Max(geo.RearCap.Bottom, geo.FrontCap.Bottom);
                float levelY = maxY - (maxY - minY) * fill;
                Color fLight = LightenColor(FuelColor, 35);
                Color fDark = DarkenColor(FuelColor, 35);

                if (fill > 0f)
                {
                    using (LinearGradientBrush fuelBrush = new LinearGradientBrush(
                        new PointF(0, levelY),
                        new PointF(0, maxY),
                        Color.FromArgb(220, fLight),
                        Color.FromArgb(225, fDark)))
                    {
                        g.FillRectangle(fuelBrush, geo.TankBounds.Left - 4, levelY, geo.TankBounds.Width + 8, maxY - levelY + 4);
                    }

                    using (Pen surfacePen = new Pen(Color.FromArgb(130, 255, 255, 255), 1.4f))
                    {
                        g.DrawLine(surfacePen, geo.RearCenter.X, levelY, geo.FrontCenter.X, levelY + geo.PerspectiveY * 0.16f);
                    }
                }

                // 5. Water layer
                float waterTop = maxY;
                if (water > 0f)
                {
                    float waterHeight = Math.Max(2f, (maxY - minY) * water);
                    waterTop = maxY - waterHeight;

                    using (LinearGradientBrush waterBrush = new LinearGradientBrush(
                        new PointF(0, waterTop),
                        new PointF(0, maxY),
                        Color.FromArgb(170, LightenColor(ColWater, 20)),
                        Color.FromArgb(205, DarkenColor(ColWater, 25))))
                    {
                        g.FillRectangle(waterBrush, geo.TankBounds.Left - 4, waterTop, geo.TankBounds.Width + 8, maxY - waterTop + 4);
                    }
                }

                g.SetClip(oldClip, CombineMode.Replace);

                // 6. Transparent front section
                using (LinearGradientBrush frontGlass = new LinearGradientBrush(
                    new PointF(geo.FrontCap.Left, geo.FrontCap.Top),
                    new PointF(geo.FrontCap.Right, geo.FrontCap.Bottom),
                    Color.FromArgb(118, 240, 245, 255),
                    Color.FromArgb(45, 240, 245, 255)))
                {
                    g.FillEllipse(frontGlass, geo.FrontCap);
                }

                // 7. Top metallic highlight
                using (GraphicsPath body = BuildBodyPath(geo))
                using (LinearGradientBrush highlightBrush = new LinearGradientBrush(
                    new PointF(geo.BodyBounds.Left, geo.BodyBounds.Top),
                    new PointF(geo.BodyBounds.Left, geo.BodyBounds.Bottom),
                    Color.FromArgb(140, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255)))
                {
                    Region bodyClip = new Region(body);
                    g.SetClip(bodyClip, CombineMode.Replace);
                    g.FillRectangle(highlightBrush,
                        geo.BodyBounds.Left,
                        geo.BodyBounds.Top + 2,
                        geo.BodyBounds.Width,
                        Math.Max(8f, geo.BodyBounds.Height * 0.42f));
                    bodyClip.Dispose();
                    g.SetClip(oldClip, CombineMode.Replace);
                }

                // 8. Front dome cap
                DrawDomeCap(g, geo.FrontCap, false);

                // 9. Outer outline
                using (GraphicsPath bodyOutline = BuildBodyPath(geo))
                using (Pen outlinePen = new Pen(Color.FromArgb(155, 30, 34, 40), 1.5f))
                using (Pen capPen = new Pen(Color.FromArgb(170, 42, 46, 52), 1.2f))
                {
                    g.DrawPath(outlinePen, bodyOutline);
                    g.DrawEllipse(capPen, geo.RearCap);
                    g.DrawEllipse(capPen, geo.FrontCap);
                }

                // 10. Probe
                if (ShowProbe)
                {
                    float probeX = geo.FrontCenter.X - geo.CapWidth * 0.18f;
                    float probeTop = geo.RearCap.Top - geo.CapHeight * 0.32f;
                    float probeBottom = geo.FrontCap.Bottom - geo.CapHeight * 0.18f;
                    float floatY = maxY - (maxY - minY) * fill;
                    floatY = Math.Max(geo.FrontCap.Top + 5, Math.Min(geo.FrontCap.Bottom - 5, floatY));

                    using (Pen probePen = new Pen(Color.FromArgb(185, 72, 76, 84), 2.0f))
                    using (SolidBrush connector = new SolidBrush(Color.FromArgb(205, 64, 68, 74)))
                    using (SolidBrush floatBrush = new SolidBrush(Color.FromArgb(220, 78, 83, 90)))
                    using (Pen floatBorder = new Pen(Color.FromArgb(190, 40, 43, 49), 1f))
                    {
                        g.DrawLine(probePen, probeX, probeTop, probeX, probeBottom);
                        g.FillRectangle(connector, probeX - 7, probeTop - 5, 14, 8);

                        RectangleF floatRect = new RectangleF(probeX - 9, floatY - 3.5f, 18, 7);
                        g.FillRectangle(floatBrush, floatRect);
                        g.DrawRectangle(floatBorder, floatRect.X, floatRect.Y, floatRect.Width, floatRect.Height);
                    }
                }

                // 11. Reference line
                if (ShowReferenceLine)
                {
                    float refX = (geo.RearCenter.X + geo.FrontCenter.X) * 0.5f;
                    using (Pen refPen = new Pen(Color.FromArgb(90, 12, 14, 18), 1f))
                    {
                        refPen.DashStyle = DashStyle.Dash;
                        g.DrawLine(refPen,
                            refX,
                            geo.BodyBounds.Top + 6,
                            refX + geo.PerspectiveX * 0.04f,
                            geo.BodyBounds.Bottom - 6);
                    }
                }

                // 12. Level dots
                if (ShowLevelDot || (ShowWaterDot && water > 0f))
                {
                    float dotX = geo.FrontCap.Right - geo.CapWidth * 0.30f;
                    if (ShowLevelDot)
                    {
                        float dotY = maxY - (maxY - minY) * fill + geo.PerspectiveY * 0.16f;
                        dotY = Math.Max(geo.FrontCap.Top + 5, Math.Min(geo.FrontCap.Bottom - 5, dotY));
                        using (SolidBrush dot = new SolidBrush(Color.FromArgb(220, 64, 68, 72)))
                        {
                            g.FillEllipse(dot, dotX - 4, dotY - 4, 8, 8);
                        }
                    }

                    if (ShowWaterDot && water > 0f)
                    {
                        float waterDotY = waterTop + geo.PerspectiveY * 0.1f;
                        waterDotY = Math.Max(geo.FrontCap.Top + 5, Math.Min(geo.FrontCap.Bottom - 5, waterDotY));
                        using (SolidBrush wDot = new SolidBrush(Color.FromArgb(225, DarkenColor(ColWater, 32))))
                        using (Pen wDotPen = new Pen(Color.FromArgb(205, DarkenColor(ColWater, 48)), 1f))
                        {
                            g.FillEllipse(wDot, dotX + 10 - 4, waterDotY - 4, 8, 8);
                            g.DrawEllipse(wDotPen, dotX + 10 - 4, waterDotY - 4, 8, 8);
                        }
                    }
                }

                // 13. Percent label
                if (ShowPercentLabel)
                {
                    float textSize = Math.Max(9f, Math.Min(17f, geo.CapHeight * 0.32f));
                    using (Font font = new Font("Segoe UI", textSize, FontStyle.Bold))
                    {
                        string text = string.Format("{0:F0}%", FillPercent);
                        SizeF size = g.MeasureString(text, font);
                        float tx = geo.BodyBounds.Left + geo.BodyBounds.Width * 0.48f - size.Width * 0.5f;
                        float ty = geo.BodyBounds.Top + geo.BodyBounds.Height * 0.47f - size.Height * 0.5f;

                        using (GraphicsPath bg = BuildRoundedRectangle(new RectangleF(tx - 5, ty - 2, size.Width + 10, size.Height + 4), 5f))
                        using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(165, 255, 255, 255)))
                        using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(45, 48, 54)))
                        {
                            g.FillPath(bgBrush, bg);
                            g.DrawString(text, font, textBrush, tx, ty);
                        }
                    }
                }
            }

            g.SetClip(oldClip, CombineMode.Replace);
            oldClip.Dispose();
        }

        private static void DrawDomeCap(Graphics g, RectangleF cap, bool rear)
        {
            using (GraphicsPath capPath = new GraphicsPath())
            {
                capPath.AddEllipse(cap);
                using (PathGradientBrush dome = new PathGradientBrush(capPath))
                {
                    dome.CenterPoint = new PointF(cap.X + cap.Width * (rear ? 0.35f : 0.40f), cap.Y + cap.Height * 0.35f);
                    dome.CenterColor = rear ? Color.FromArgb(145, 135, 139, 146) : Color.FromArgb(195, 178, 183, 191);
                    dome.SurroundColors = new[] { rear ? Color.FromArgb(110, 52, 56, 64) : Color.FromArgb(165, 72, 76, 84) };
                    g.FillEllipse(dome, cap);
                }

                using (Pen ring = new Pen(Color.FromArgb(rear ? 120 : 165, 33, 36, 43), rear ? 1.0f : 1.2f))
                {
                    g.DrawEllipse(ring, cap);
                }
            }
        }

        private static void DrawShadow(Graphics g, TankGeometry geo)
        {
            using (Region shadow = BuildTankRegion(geo))
            {
                for (int i = 5; i >= 1; i--)
                {
                    using (Region layer = shadow.Clone())
                    using (Matrix m = new Matrix())
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(12 + i * 9, 0, 0, 0)))
                    {
                        m.Translate(i * 1.8f, i * 1.25f);
                        layer.Transform(m);
                        g.FillRegion(sb, layer);
                    }
                }
            }
        }

        private static GraphicsPath BuildBodyPath(TankGeometry geo)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(new[]
            {
                new PointF(geo.RearCenter.X, geo.RearCap.Top),
                new PointF(geo.FrontCenter.X, geo.FrontCap.Top),
                new PointF(geo.FrontCenter.X, geo.FrontCap.Bottom),
                new PointF(geo.RearCenter.X, geo.RearCap.Bottom)
            });
            return path;
        }

        private static Region BuildTankRegion(TankGeometry geo)
        {
            Region region = new Region(BuildBodyPath(geo));
            region.Union(geo.RearCap);
            region.Union(geo.FrontCap);
            return region;
        }

        private static GraphicsPath BuildRoundedRectangle(RectangleF rect, float radius)
        {
            float d = radius * 2f;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static TankGeometry BuildGeometry(int x, int y, int w, int h)
        {
            float fx = x;
            float fy = y;
            float fw = w;
            float fh = h;

            float capWidth = Math.Max(18f, fw * 0.24f);
            float capHeight = Math.Max(14f, fh * 0.62f);
            float rearX = fx + fw * 0.07f;
            float frontX = fx + fw - capWidth - fw * 0.08f;
            float rearY = fy + fh * 0.13f;
            float frontY = rearY + fh * 0.10f;

            RectangleF rear = new RectangleF(rearX, rearY, capWidth, capHeight);
            RectangleF front = new RectangleF(frontX, frontY, capWidth, capHeight);

            float perspectiveX = front.X - rear.X;
            float perspectiveY = front.Y - rear.Y;

            RectangleF bounds = RectangleF.Union(rear, front);

            return new TankGeometry
            {
                RearCap = rear,
                FrontCap = front,
                RearCenter = new PointF(rear.X + rear.Width * 0.5f, rear.Y + rear.Height * 0.5f),
                FrontCenter = new PointF(front.X + front.Width * 0.5f, front.Y + front.Height * 0.5f),
                PerspectiveX = perspectiveX,
                PerspectiveY = perspectiveY,
                BodyBounds = new RectangleF(rear.X + rear.Width * 0.5f, rear.Y, (front.X + front.Width * 0.5f) - (rear.X + rear.Width * 0.5f), Math.Max(rear.Bottom, front.Bottom) - rear.Y),
                TankBounds = bounds,
                CapWidth = capWidth,
                CapHeight = capHeight
            };
        }

        private static Color LightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount));
        }

        private static Color DarkenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, color.R - amount),
                Math.Max(0, color.G - amount),
                Math.Max(0, color.B - amount));
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private struct TankGeometry
        {
            public RectangleF RearCap;
            public RectangleF FrontCap;
            public PointF RearCenter;
            public PointF FrontCenter;
            public float PerspectiveX;
            public float PerspectiveY;
            public RectangleF BodyBounds;
            public RectangleF TankBounds;
            public float CapWidth;
            public float CapHeight;
        }
    }
}
