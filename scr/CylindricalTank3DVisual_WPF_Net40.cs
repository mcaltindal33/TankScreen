//-----------------------------------------------------------------------
// <copyright file="CylindricalTank3DVisual_WPF_Net40.cs" company="PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.">
//     Author: MUSTAFA CAGRI ALTINDAL
//     Copyright (c) PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DXApplication1
{
    public class CylindricalTank3DVisual : FrameworkElement
    {
        public static readonly DependencyProperty FillPercentProperty =
            DependencyProperty.Register(
                "FillPercent", typeof(double), typeof(CylindricalTank3DVisual),
                new FrameworkPropertyMetadata(60.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoercePercent));

        public static readonly DependencyProperty FuelColorProperty =
            DependencyProperty.Register(
                "FuelColor", typeof(Color), typeof(CylindricalTank3DVisual),
                new FrameworkPropertyMetadata(Color.FromRgb(180, 180, 50), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty WaterPercentProperty =
            DependencyProperty.Register(
                "WaterPercent", typeof(double), typeof(CylindricalTank3DVisual),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoercePercent));

        public static readonly DependencyProperty ShowPercentLabelProperty =
            DependencyProperty.Register(
                "ShowPercentLabel", typeof(bool), typeof(CylindricalTank3DVisual),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowLevelDotProperty =
            DependencyProperty.Register(
                "ShowLevelDot", typeof(bool), typeof(CylindricalTank3DVisual),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowWaterDotProperty =
            DependencyProperty.Register(
                "ShowWaterDot", typeof(bool), typeof(CylindricalTank3DVisual),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowReferenceLineProperty =
            DependencyProperty.Register(
                "ShowReferenceLine", typeof(bool), typeof(CylindricalTank3DVisual),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowProbeProperty =
            DependencyProperty.Register(
                "ShowProbe", typeof(bool), typeof(CylindricalTank3DVisual),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TankPaddingProperty =
            DependencyProperty.Register(
                "TankPadding", typeof(double), typeof(CylindricalTank3DVisual),
                new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsRender));

        private static readonly Color ColWater = Color.FromRgb(85, 128, 198);

        [Category("Tank")]
        public double FillPercent
        {
            get { return (double)GetValue(FillPercentProperty); }
            set { SetValue(FillPercentProperty, value); }
        }

        [Category("Tank")]
        public Color FuelColor
        {
            get { return (Color)GetValue(FuelColorProperty); }
            set { SetValue(FuelColorProperty, value); }
        }

        [Category("Tank")]
        public double WaterPercent
        {
            get { return (double)GetValue(WaterPercentProperty); }
            set { SetValue(WaterPercentProperty, value); }
        }

        [Category("Tank")]
        public bool ShowPercentLabel
        {
            get { return (bool)GetValue(ShowPercentLabelProperty); }
            set { SetValue(ShowPercentLabelProperty, value); }
        }

        [Category("Tank")]
        public bool ShowLevelDot
        {
            get { return (bool)GetValue(ShowLevelDotProperty); }
            set { SetValue(ShowLevelDotProperty, value); }
        }

        [Category("Tank")]
        public bool ShowWaterDot
        {
            get { return (bool)GetValue(ShowWaterDotProperty); }
            set { SetValue(ShowWaterDotProperty, value); }
        }

        [Category("Tank")]
        public bool ShowReferenceLine
        {
            get { return (bool)GetValue(ShowReferenceLineProperty); }
            set { SetValue(ShowReferenceLineProperty, value); }
        }

        [Category("Tank")]
        public bool ShowProbe
        {
            get { return (bool)GetValue(ShowProbeProperty); }
            set { SetValue(ShowProbeProperty, value); }
        }

        [Category("Tank")]
        public double TankPadding
        {
            get { return (double)GetValue(TankPaddingProperty); }
            set { SetValue(TankPaddingProperty, value); }
        }

        public CylindricalTank3DVisual()
        {
            Width = 300;
            Height = 160;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double pad = Math.Max(0.0, TankPadding);
            double x = pad;
            double y = pad;
            double w = ActualWidth - pad * 2.0;
            double h = ActualHeight - pad * 2.0;
            if (w < 24 || h < 24)
                return;

            TankGeometry geo = BuildGeometry(x, y, w, h);
            double fill = FillPercent / 100.0;
            double water = WaterPercent / 100.0;

            // 1. Drop shadow
            DrawShadow(dc, geo);

            // 2. 3D body
            Geometry body = BuildBodyGeometry(geo);
            LinearGradientBrush bodyBrush = new LinearGradientBrush(
                Color.FromRgb(105, 108, 114),
                Color.FromRgb(52, 55, 61),
                new Point(0.5, 0), new Point(0.5, 1));
            dc.DrawGeometry(bodyBrush, null, body);

            // 3. Rear dome
            DrawDomeCap(dc, geo.RearCap, true);

            Geometry tankGeo = BuildTankGeometry(geo, body);
            dc.PushClip(tankGeo);

            double minY = Math.Min(geo.RearCap.Y, geo.FrontCap.Y);
            double maxY = Math.Max(geo.RearCap.Y + geo.RearCap.Height, geo.FrontCap.Y + geo.FrontCap.Height);
            double levelY = maxY - (maxY - minY) * fill;

            // 4. Fuel
            if (fill > 0.0)
            {
                Color fLight = LightenColor(FuelColor, 35);
                Color fDark = DarkenColor(FuelColor, 35);
                LinearGradientBrush fuelBrush = new LinearGradientBrush(
                    Color.FromArgb(220, fLight.R, fLight.G, fLight.B),
                    Color.FromArgb(225, fDark.R, fDark.G, fDark.B),
                    new Point(0.5, 0), new Point(0.5, 1));

                dc.DrawRectangle(fuelBrush, null, new Rect(geo.TankBounds.X - 4, levelY, geo.TankBounds.Width + 8, maxY - levelY + 4));
                dc.DrawLine(
                    new Pen(new SolidColorBrush(Color.FromArgb(130, 255, 255, 255)), 1.3),
                    new Point(geo.RearCenter.X, levelY),
                    new Point(geo.FrontCenter.X, levelY + geo.PerspectiveY * 0.16));
            }

            // 5. Water
            double waterTop = maxY;
            if (water > 0.0)
            {
                double waterHeight = Math.Max(2.0, (maxY - minY) * water);
                waterTop = maxY - waterHeight;
                Color wcLight = LightenColor(ColWater, 20);
                Color wcDark = DarkenColor(ColWater, 25);
                LinearGradientBrush waterBrush = new LinearGradientBrush(
                    Color.FromArgb(170, wcLight.R, wcLight.G, wcLight.B),
                    Color.FromArgb(205, wcDark.R, wcDark.G, wcDark.B),
                    new Point(0.5, 0), new Point(0.5, 1));
                dc.DrawRectangle(waterBrush, null, new Rect(geo.TankBounds.X - 4, waterTop, geo.TankBounds.Width + 8, maxY - waterTop + 4));
            }

            dc.Pop();

            // 6. Transparent front section
            LinearGradientBrush frontGlass = new LinearGradientBrush(
                Color.FromArgb(118, 240, 245, 255),
                Color.FromArgb(45, 240, 245, 255),
                new Point(0, 0), new Point(1, 1));
            dc.DrawEllipse(frontGlass, null,
                new Point(geo.FrontCap.X + geo.FrontCap.Width * 0.5, geo.FrontCap.Y + geo.FrontCap.Height * 0.5),
                geo.FrontCap.Width * 0.5, geo.FrontCap.Height * 0.5);

            // 7. Highlight
            dc.PushClip(body);
            LinearGradientBrush highlightBrush = new LinearGradientBrush(
                Color.FromArgb(140, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                new Point(0.5, 0), new Point(0.5, 1));
            dc.DrawRectangle(highlightBrush, null,
                new Rect(geo.BodyBounds.X, geo.BodyBounds.Y + 2, geo.BodyBounds.Width, Math.Max(8.0, geo.BodyBounds.Height * 0.42)));
            dc.Pop();

            // 8. Front dome
            DrawDomeCap(dc, geo.FrontCap, false);

            // 9. Outline
            Pen outlinePen = new Pen(new SolidColorBrush(Color.FromArgb(155, 30, 34, 40)), 1.4);
            dc.DrawGeometry(null, outlinePen, body);
            Pen capPen = new Pen(new SolidColorBrush(Color.FromArgb(170, 42, 46, 52)), 1.2);
            dc.DrawEllipse(null, capPen,
                new Point(geo.RearCap.X + geo.RearCap.Width * 0.5, geo.RearCap.Y + geo.RearCap.Height * 0.5),
                geo.RearCap.Width * 0.5, geo.RearCap.Height * 0.5);
            dc.DrawEllipse(null, capPen,
                new Point(geo.FrontCap.X + geo.FrontCap.Width * 0.5, geo.FrontCap.Y + geo.FrontCap.Height * 0.5),
                geo.FrontCap.Width * 0.5, geo.FrontCap.Height * 0.5);

            // 10. Probe
            if (ShowProbe)
            {
                double probeX = geo.FrontCenter.X - geo.CapWidth * 0.18;
                double probeTop = geo.RearCap.Y - geo.CapHeight * 0.32;
                double probeBottom = geo.FrontCap.Y + geo.FrontCap.Height - geo.CapHeight * 0.18;
                double floatY = maxY - (maxY - minY) * fill;
                floatY = Clamp(floatY, geo.FrontCap.Y + 5, geo.FrontCap.Y + geo.FrontCap.Height - 5);

                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(185, 72, 76, 84)), 2.0),
                    new Point(probeX, probeTop), new Point(probeX, probeBottom));
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(205, 64, 68, 74)), null,
                    new Rect(probeX - 7, probeTop - 5, 14, 8));
                Rect floatRect = new Rect(probeX - 9, floatY - 3.5, 18, 7);
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(220, 78, 83, 90)),
                    new Pen(new SolidColorBrush(Color.FromArgb(190, 40, 43, 49)), 1.0),
                    floatRect);
            }

            // 11. Reference line
            if (ShowReferenceLine)
            {
                double refX = (geo.RearCenter.X + geo.FrontCenter.X) * 0.5;
                Pen refPen = new Pen(new SolidColorBrush(Color.FromArgb(90, 12, 14, 18)), 1.0);
                refPen.DashStyle = DashStyles.Dash;
                dc.DrawLine(refPen,
                    new Point(refX, geo.BodyBounds.Y + 6),
                    new Point(refX + geo.PerspectiveX * 0.04, geo.BodyBounds.Y + geo.BodyBounds.Height - 6));
            }

            // 12. Level dots
            double dotX = geo.FrontCap.X + geo.FrontCap.Width - geo.CapWidth * 0.30;
            if (ShowLevelDot)
            {
                double dotY = maxY - (maxY - minY) * fill + geo.PerspectiveY * 0.16;
                dotY = Clamp(dotY, geo.FrontCap.Y + 5, geo.FrontCap.Y + geo.FrontCap.Height - 5);
                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(220, 64, 68, 72)), null,
                    new Point(dotX, dotY), 4, 4);
            }

            if (ShowWaterDot && water > 0.0)
            {
                double waterDotY = Clamp(waterTop + geo.PerspectiveY * 0.10, geo.FrontCap.Y + 5, geo.FrontCap.Y + geo.FrontCap.Height - 5);
                Color wb = DarkenColor(ColWater, 32);
                Color wo = DarkenColor(ColWater, 48);
                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(225, wb.R, wb.G, wb.B)),
                    new Pen(new SolidColorBrush(Color.FromArgb(205, wo.R, wo.G, wo.B)), 1.0),
                    new Point(dotX + 10, waterDotY), 4, 4);
            }

            // 13. Percent label
            if (ShowPercentLabel)
            {
                double fontSize = Clamp(geo.CapHeight * 0.32, 9, 17);
                string text = string.Format("{0:F0}%", FillPercent);

                FormattedText ft = new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                    fontSize,
                    new SolidColorBrush(Color.FromRgb(45, 48, 54)));

                double tx = geo.BodyBounds.X + geo.BodyBounds.Width * 0.48 - ft.Width * 0.5;
                double ty = geo.BodyBounds.Y + geo.BodyBounds.Height * 0.47 - ft.Height * 0.5;

                dc.DrawRoundedRectangle(
                    new SolidColorBrush(Color.FromArgb(165, 255, 255, 255)),
                    null,
                    new Rect(tx - 5, ty - 2, ft.Width + 10, ft.Height + 4),
                    5, 5);
                dc.DrawText(ft, new Point(tx, ty));
            }
        }

        private static object CoercePercent(DependencyObject d, object baseValue)
        {
            return Clamp((double)baseValue, 0.0, 100.0);
        }

        private static void DrawShadow(DrawingContext dc, TankGeometry geo)
        {
            Geometry body = BuildBodyGeometry(geo);
            GeometryGroup gg = new GeometryGroup();
            gg.Children.Add(body);
            gg.Children.Add(new EllipseGeometry(new Rect(geo.RearCap.X, geo.RearCap.Y, geo.RearCap.Width, geo.RearCap.Height)));
            gg.Children.Add(new EllipseGeometry(new Rect(geo.FrontCap.X, geo.FrontCap.Y, geo.FrontCap.Width, geo.FrontCap.Height)));

            for (int i = 5; i >= 1; i--)
            {
                TranslateTransform shift = new TranslateTransform(i * 1.8, i * 1.25);
                dc.PushTransform(shift);
                dc.DrawGeometry(
                    new SolidColorBrush(Color.FromArgb((byte)(12 + i * 9), 0, 0, 0)),
                    null,
                    gg);
                dc.Pop();
            }
        }

        private static void DrawDomeCap(DrawingContext dc, Rect cap, bool rear)
        {
            EllipseGeometry e = new EllipseGeometry(new Point(cap.X + cap.Width * 0.5, cap.Y + cap.Height * 0.5), cap.Width * 0.5, cap.Height * 0.5);
            RadialGradientBrush rg = new RadialGradientBrush();
            rg.Center = new Point(rear ? 0.35 : 0.40, 0.35);
            rg.GradientOrigin = rg.Center;
            rg.RadiusX = 0.75;
            rg.RadiusY = 0.75;
            rg.GradientStops.Add(new GradientStop(rear ? Color.FromArgb(145, 135, 139, 146) : Color.FromArgb(195, 178, 183, 191), 0));
            rg.GradientStops.Add(new GradientStop(rear ? Color.FromArgb(110, 52, 56, 64) : Color.FromArgb(165, 72, 76, 84), 1));
            dc.DrawGeometry(rg, new Pen(new SolidColorBrush(Color.FromArgb(rear ? (byte)120 : (byte)165, 33, 36, 43)), rear ? 1.0 : 1.2), e);
        }

        private static Geometry BuildBodyGeometry(TankGeometry geo)
        {
            StreamGeometry sg = new StreamGeometry();
            using (StreamGeometryContext ctx = sg.Open())
            {
                ctx.BeginFigure(new Point(geo.RearCenter.X, geo.RearCap.Y), true, true);
                ctx.LineTo(new Point(geo.FrontCenter.X, geo.FrontCap.Y), true, false);
                ctx.LineTo(new Point(geo.FrontCenter.X, geo.FrontCap.Y + geo.FrontCap.Height), true, false);
                ctx.LineTo(new Point(geo.RearCenter.X, geo.RearCap.Y + geo.RearCap.Height), true, false);
            }
            sg.Freeze();
            return sg;
        }

        private static Geometry BuildTankGeometry(TankGeometry geo, Geometry body)
        {
            GeometryGroup g = new GeometryGroup();
            g.Children.Add(body);
            g.Children.Add(new EllipseGeometry(new Rect(geo.RearCap.X, geo.RearCap.Y, geo.RearCap.Width, geo.RearCap.Height)));
            g.Children.Add(new EllipseGeometry(new Rect(geo.FrontCap.X, geo.FrontCap.Y, geo.FrontCap.Width, geo.FrontCap.Height)));
            return g;
        }

        private static TankGeometry BuildGeometry(double x, double y, double w, double h)
        {
            double capWidth = Math.Max(18.0, w * 0.24);
            double capHeight = Math.Max(14.0, h * 0.62);
            double rearX = x + w * 0.07;
            double frontX = x + w - capWidth - w * 0.08;
            double rearY = y + h * 0.13;
            double frontY = rearY + h * 0.10;

            Rect rear = new Rect(rearX, rearY, capWidth, capHeight);
            Rect front = new Rect(frontX, frontY, capWidth, capHeight);

            Rect tankBounds = Rect.Union(rear, front);
            double bodyX = rear.X + rear.Width * 0.5;
            double bodyW = (front.X + front.Width * 0.5) - bodyX;
            double bodyH = Math.Max(rear.Bottom, front.Bottom) - rear.Y;

            TankGeometry geo = new TankGeometry();
            geo.RearCap = rear;
            geo.FrontCap = front;
            geo.RearCenter = new Point(rear.X + rear.Width * 0.5, rear.Y + rear.Height * 0.5);
            geo.FrontCenter = new Point(front.X + front.Width * 0.5, front.Y + front.Height * 0.5);
            geo.PerspectiveX = front.X - rear.X;
            geo.PerspectiveY = front.Y - rear.Y;
            geo.BodyBounds = new Rect(bodyX, rear.Y, bodyW, bodyH);
            geo.TankBounds = tankBounds;
            geo.CapWidth = capWidth;
            geo.CapHeight = capHeight;
            return geo;
        }

        private static Color LightenColor(Color c, int amount)
        {
            return Color.FromArgb(c.A,
                (byte)Math.Min(255, c.R + amount),
                (byte)Math.Min(255, c.G + amount),
                (byte)Math.Min(255, c.B + amount));
        }

        private static Color DarkenColor(Color c, int amount)
        {
            return Color.FromArgb(c.A,
                (byte)Math.Max(0, c.R - amount),
                (byte)Math.Max(0, c.G - amount),
                (byte)Math.Max(0, c.B - amount));
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private struct TankGeometry
        {
            public Rect RearCap;
            public Rect FrontCap;
            public Point RearCenter;
            public Point FrontCenter;
            public double PerspectiveX;
            public double PerspectiveY;
            public Rect BodyBounds;
            public Rect TankBounds;
            public double CapWidth;
            public double CapHeight;
        }
    }
}
