//-----------------------------------------------------------------------
// <copyright file="CylindricalTank3DVisual_WPF_Net8.cs" company="PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.">
//     Author: MUSTAFA CAGRI ALTINDAL
//     Copyright (c) PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DXApplication1;

public class CylindricalTank3DVisual : FrameworkElement
{
    public static readonly DependencyProperty FillPercentProperty =
        DependencyProperty.Register(
            nameof(FillPercent), typeof(double), typeof(CylindricalTank3DVisual),
            new FrameworkPropertyMetadata(60.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoercePercent));

    public static readonly DependencyProperty FuelColorProperty =
        DependencyProperty.Register(
            nameof(FuelColor), typeof(Color), typeof(CylindricalTank3DVisual),
            new FrameworkPropertyMetadata(Color.FromRgb(180, 180, 50), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty WaterPercentProperty =
        DependencyProperty.Register(
            nameof(WaterPercent), typeof(double), typeof(CylindricalTank3DVisual),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoercePercent));

    public static readonly DependencyProperty ShowPercentLabelProperty =
        DependencyProperty.Register(
            nameof(ShowPercentLabel), typeof(bool), typeof(CylindricalTank3DVisual),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowLevelDotProperty =
        DependencyProperty.Register(
            nameof(ShowLevelDot), typeof(bool), typeof(CylindricalTank3DVisual),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowWaterDotProperty =
        DependencyProperty.Register(
            nameof(ShowWaterDot), typeof(bool), typeof(CylindricalTank3DVisual),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowReferenceLineProperty =
        DependencyProperty.Register(
            nameof(ShowReferenceLine), typeof(bool), typeof(CylindricalTank3DVisual),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowProbeProperty =
        DependencyProperty.Register(
            nameof(ShowProbe), typeof(bool), typeof(CylindricalTank3DVisual),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TankPaddingProperty =
        DependencyProperty.Register(
            nameof(TankPadding), typeof(double), typeof(CylindricalTank3DVisual),
            new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly Color ColWater = Color.FromRgb(85, 128, 198);

    [Category("Tank")]
    public double FillPercent
    {
        get => (double)GetValue(FillPercentProperty);
        set => SetValue(FillPercentProperty, value);
    }

    [Category("Tank")]
    public Color FuelColor
    {
        get => (Color)GetValue(FuelColorProperty);
        set => SetValue(FuelColorProperty, value);
    }

    [Category("Tank")]
    public double WaterPercent
    {
        get => (double)GetValue(WaterPercentProperty);
        set => SetValue(WaterPercentProperty, value);
    }

    [Category("Tank")]
    public bool ShowPercentLabel
    {
        get => (bool)GetValue(ShowPercentLabelProperty);
        set => SetValue(ShowPercentLabelProperty, value);
    }

    [Category("Tank")]
    public bool ShowLevelDot
    {
        get => (bool)GetValue(ShowLevelDotProperty);
        set => SetValue(ShowLevelDotProperty, value);
    }

    [Category("Tank")]
    public bool ShowWaterDot
    {
        get => (bool)GetValue(ShowWaterDotProperty);
        set => SetValue(ShowWaterDotProperty, value);
    }

    [Category("Tank")]
    public bool ShowReferenceLine
    {
        get => (bool)GetValue(ShowReferenceLineProperty);
        set => SetValue(ShowReferenceLineProperty, value);
    }

    [Category("Tank")]
    public bool ShowProbe
    {
        get => (bool)GetValue(ShowProbeProperty);
        set => SetValue(ShowProbeProperty, value);
    }

    [Category("Tank")]
    public double TankPadding
    {
        get => (double)GetValue(TankPaddingProperty);
        set => SetValue(TankPaddingProperty, value);
    }

    public CylindricalTank3DVisual()
    {
        Width = 300;
        Height = 160;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var pad = Math.Max(0.0, TankPadding);
        var x = pad;
        var y = pad;
        var w = ActualWidth - pad * 2.0;
        var h = ActualHeight - pad * 2.0;
        if (w < 24 || h < 24)
            return;

        var geo = BuildGeometry(x, y, w, h);
        var fill = FillPercent / 100.0;
        var water = WaterPercent / 100.0;

        // 1. Drop shadow
        DrawShadow(dc, geo);

        // 2. 3D body
        var body = BuildBodyGeometry(geo);
        var bodyBrush = new LinearGradientBrush(
            Color.FromRgb(105, 108, 114),
            Color.FromRgb(52, 55, 61),
            new Point(0.5, 0), new Point(0.5, 1));
        dc.DrawGeometry(bodyBrush, null, body);

        // 3. Rear dome
        DrawDomeCap(dc, geo.RearCap, true);

        var tankGeo = BuildTankGeometry(geo, body);
        dc.PushClip(tankGeo);

        var minY = Math.Min(geo.RearCap.Y, geo.FrontCap.Y);
        var maxY = Math.Max(geo.RearCap.Y + geo.RearCap.Height, geo.FrontCap.Y + geo.FrontCap.Height);
        var levelY = maxY - (maxY - minY) * fill;

        // 4. Fuel
        if (fill > 0.0)
        {
            var fLight = LightenColor(FuelColor, 35);
            var fDark = DarkenColor(FuelColor, 35);
            var fuelBrush = new LinearGradientBrush(
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
        var waterTop = maxY;
        if (water > 0.0)
        {
            var waterHeight = Math.Max(2.0, (maxY - minY) * water);
            waterTop = maxY - waterHeight;
            var wcLight = LightenColor(ColWater, 20);
            var wcDark = DarkenColor(ColWater, 25);
            var waterBrush = new LinearGradientBrush(
                Color.FromArgb(170, wcLight.R, wcLight.G, wcLight.B),
                Color.FromArgb(205, wcDark.R, wcDark.G, wcDark.B),
                new Point(0.5, 0), new Point(0.5, 1));
            dc.DrawRectangle(waterBrush, null, new Rect(geo.TankBounds.X - 4, waterTop, geo.TankBounds.Width + 8, maxY - waterTop + 4));
        }

        dc.Pop();

        // 6. Transparent front section
        var frontGlass = new LinearGradientBrush(
            Color.FromArgb(118, 240, 245, 255),
            Color.FromArgb(45, 240, 245, 255),
            new Point(0, 0), new Point(1, 1));
        dc.DrawEllipse(frontGlass, null,
            new Point(geo.FrontCap.X + geo.FrontCap.Width * 0.5, geo.FrontCap.Y + geo.FrontCap.Height * 0.5),
            geo.FrontCap.Width * 0.5, geo.FrontCap.Height * 0.5);

        // 7. Highlight
        dc.PushClip(body);
        var highlightBrush = new LinearGradientBrush(
            Color.FromArgb(140, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            new Point(0.5, 0), new Point(0.5, 1));
        dc.DrawRectangle(highlightBrush, null,
            new Rect(geo.BodyBounds.X, geo.BodyBounds.Y + 2, geo.BodyBounds.Width, Math.Max(8.0, geo.BodyBounds.Height * 0.42)));
        dc.Pop();

        // 8. Front dome
        DrawDomeCap(dc, geo.FrontCap, false);

        // 9. Outline
        var outlinePen = new Pen(new SolidColorBrush(Color.FromArgb(155, 30, 34, 40)), 1.4);
        dc.DrawGeometry(null, outlinePen, body);
        var capPen = new Pen(new SolidColorBrush(Color.FromArgb(170, 42, 46, 52)), 1.2);
        dc.DrawEllipse(null, capPen,
            new Point(geo.RearCap.X + geo.RearCap.Width * 0.5, geo.RearCap.Y + geo.RearCap.Height * 0.5),
            geo.RearCap.Width * 0.5, geo.RearCap.Height * 0.5);
        dc.DrawEllipse(null, capPen,
            new Point(geo.FrontCap.X + geo.FrontCap.Width * 0.5, geo.FrontCap.Y + geo.FrontCap.Height * 0.5),
            geo.FrontCap.Width * 0.5, geo.FrontCap.Height * 0.5);

        // 10. Probe
        if (ShowProbe)
        {
            var probeX = geo.FrontCenter.X - geo.CapWidth * 0.18;
            var probeTop = geo.RearCap.Y - geo.CapHeight * 0.32;
            var probeBottom = geo.FrontCap.Y + geo.FrontCap.Height - geo.CapHeight * 0.18;
            var floatY = maxY - (maxY - minY) * fill;
            floatY = Clamp(floatY, geo.FrontCap.Y + 5, geo.FrontCap.Y + geo.FrontCap.Height - 5);

            dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(185, 72, 76, 84)), 2.0),
                new Point(probeX, probeTop), new Point(probeX, probeBottom));
            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(205, 64, 68, 74)), null,
                new Rect(probeX - 7, probeTop - 5, 14, 8));
            var floatRect = new Rect(probeX - 9, floatY - 3.5, 18, 7);
            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(220, 78, 83, 90)),
                new Pen(new SolidColorBrush(Color.FromArgb(190, 40, 43, 49)), 1.0),
                floatRect);
        }

        // 11. Reference line
        if (ShowReferenceLine)
        {
            var refX = (geo.RearCenter.X + geo.FrontCenter.X) * 0.5;
            var refPen = new Pen(new SolidColorBrush(Color.FromArgb(90, 12, 14, 18)), 1.0)
            {
                DashStyle = DashStyles.Dash
            };
            dc.DrawLine(refPen,
                new Point(refX, geo.BodyBounds.Y + 6),
                new Point(refX + geo.PerspectiveX * 0.04, geo.BodyBounds.Y + geo.BodyBounds.Height - 6));
        }

        // 12. Level dots
        var dotX = geo.FrontCap.X + geo.FrontCap.Width - geo.CapWidth * 0.30;
        if (ShowLevelDot)
        {
            var dotY = maxY - (maxY - minY) * fill + geo.PerspectiveY * 0.16;
            dotY = Clamp(dotY, geo.FrontCap.Y + 5, geo.FrontCap.Y + geo.FrontCap.Height - 5);
            dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(220, 64, 68, 72)), null,
                new Point(dotX, dotY), 4, 4);
        }

        if (ShowWaterDot && water > 0.0)
        {
            var waterDotY = Clamp(waterTop + geo.PerspectiveY * 0.10, geo.FrontCap.Y + 5, geo.FrontCap.Y + geo.FrontCap.Height - 5);
            var wb = DarkenColor(ColWater, 32);
            var wo = DarkenColor(ColWater, 48);
            dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(225, wb.R, wb.G, wb.B)),
                new Pen(new SolidColorBrush(Color.FromArgb(205, wo.R, wo.G, wo.B)), 1.0),
                new Point(dotX + 10, waterDotY), 4, 4);
        }

        // 13. Percent label
        if (ShowPercentLabel)
        {
            var fontSize = Clamp(geo.CapHeight * 0.32, 9, 17);
            var text = string.Format("{0:F0}%", FillPercent);

            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var ft = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                fontSize,
                new SolidColorBrush(Color.FromRgb(45, 48, 54)),
                pixelsPerDip);

            var tx = geo.BodyBounds.X + geo.BodyBounds.Width * 0.48 - ft.Width * 0.5;
            var ty = geo.BodyBounds.Y + geo.BodyBounds.Height * 0.47 - ft.Height * 0.5;

            dc.DrawRoundedRectangle(
                new SolidColorBrush(Color.FromArgb(165, 255, 255, 255)),
                null,
                new Rect(tx - 5, ty - 2, ft.Width + 10, ft.Height + 4),
                5, 5);
            dc.DrawText(ft, new Point(tx, ty));
        }
    }

    private static object CoercePercent(DependencyObject d, object baseValue)
        => Clamp((double)baseValue, 0.0, 100.0);

    private static void DrawShadow(DrawingContext dc, TankGeometry geo)
    {
        var body = BuildBodyGeometry(geo);
        var gg = new GeometryGroup();
        gg.Children.Add(body);
        gg.Children.Add(new EllipseGeometry(new Rect(geo.RearCap.X, geo.RearCap.Y, geo.RearCap.Width, geo.RearCap.Height)));
        gg.Children.Add(new EllipseGeometry(new Rect(geo.FrontCap.X, geo.FrontCap.Y, geo.FrontCap.Width, geo.FrontCap.Height)));

        for (var i = 5; i >= 1; i--)
        {
            dc.PushTransform(new TranslateTransform(i * 1.8, i * 1.25));
            dc.DrawGeometry(
                new SolidColorBrush(Color.FromArgb((byte)(12 + i * 9), 0, 0, 0)),
                null,
                gg);
            dc.Pop();
        }
    }

    private static void DrawDomeCap(DrawingContext dc, Rect cap, bool rear)
    {
        var e = new EllipseGeometry(new Point(cap.X + cap.Width * 0.5, cap.Y + cap.Height * 0.5), cap.Width * 0.5, cap.Height * 0.5);
        var rg = new RadialGradientBrush
        {
            Center = new Point(rear ? 0.35 : 0.40, 0.35),
            GradientOrigin = new Point(rear ? 0.35 : 0.40, 0.35),
            RadiusX = 0.75,
            RadiusY = 0.75
        };
        rg.GradientStops.Add(new GradientStop(rear ? Color.FromArgb(145, 135, 139, 146) : Color.FromArgb(195, 178, 183, 191), 0));
        rg.GradientStops.Add(new GradientStop(rear ? Color.FromArgb(110, 52, 56, 64) : Color.FromArgb(165, 72, 76, 84), 1));

        dc.DrawGeometry(rg, new Pen(new SolidColorBrush(Color.FromArgb(rear ? (byte)120 : (byte)165, 33, 36, 43)), rear ? 1.0 : 1.2), e);
    }

    private static Geometry BuildBodyGeometry(TankGeometry geo)
    {
        var sg = new StreamGeometry();
        using (var ctx = sg.Open())
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
        var g = new GeometryGroup();
        g.Children.Add(body);
        g.Children.Add(new EllipseGeometry(new Rect(geo.RearCap.X, geo.RearCap.Y, geo.RearCap.Width, geo.RearCap.Height)));
        g.Children.Add(new EllipseGeometry(new Rect(geo.FrontCap.X, geo.FrontCap.Y, geo.FrontCap.Width, geo.FrontCap.Height)));
        return g;
    }

    private static TankGeometry BuildGeometry(double x, double y, double w, double h)
    {
        var capWidth = Math.Max(18.0, w * 0.24);
        var capHeight = Math.Max(14.0, h * 0.62);
        var rearX = x + w * 0.07;
        var frontX = x + w - capWidth - w * 0.08;
        var rearY = y + h * 0.13;
        var frontY = rearY + h * 0.10;

        var rear = new Rect(rearX, rearY, capWidth, capHeight);
        var front = new Rect(frontX, frontY, capWidth, capHeight);

        var tankBounds = Rect.Union(rear, front);
        var bodyX = rear.X + rear.Width * 0.5;
        var bodyW = (front.X + front.Width * 0.5) - bodyX;
        var bodyH = Math.Max(rear.Bottom, front.Bottom) - rear.Y;

        return new TankGeometry
        {
            RearCap = rear,
            FrontCap = front,
            RearCenter = new Point(rear.X + rear.Width * 0.5, rear.Y + rear.Height * 0.5),
            FrontCenter = new Point(front.X + front.Width * 0.5, front.Y + front.Height * 0.5),
            PerspectiveX = front.X - rear.X,
            PerspectiveY = front.Y - rear.Y,
            BodyBounds = new Rect(bodyX, rear.Y, bodyW, bodyH),
            TankBounds = tankBounds,
            CapWidth = capWidth,
            CapHeight = capHeight
        };
    }

    private static Color LightenColor(Color c, int amount)
        => Color.FromArgb(c.A,
            (byte)Math.Min(255, c.R + amount),
            (byte)Math.Min(255, c.G + amount),
            (byte)Math.Min(255, c.B + amount));

    private static Color DarkenColor(Color c, int amount)
        => Color.FromArgb(c.A,
            (byte)Math.Max(0, c.R - amount),
            (byte)Math.Max(0, c.G - amount),
            (byte)Math.Max(0, c.B - amount));

    private static double Clamp(double value, double min, double max)
        => value < min ? min : (value > max ? max : value);

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
