//-----------------------------------------------------------------------
// <copyright file="CylindricalTankVisual.cs" company="PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.">
//     Author: MUSTAFA CAGRI ALTINDAL
//     Copyright (c) PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
//
// HEDEF: WPF + .NET 8
//
// .NET 4.0'dan farklar:
//   - FormattedText constructor'ı .NET 8'de pixelsPerDip parametresi ZORUNLU
//   - Property syntax: expression-bodied (=>)  ve auto-property kullanılabilir
//   - 'using var' ve null-coalescing atama (?=) desteklenir
//   - StreamGeometry aynı API, ek değişiklik yok
//   - Namespace: file-scoped (namespace DXApplication1;) kullanılabilir
//   - Nullable reference types (#nullable enable) opsiyonel
//
// KULLANIM (XAML):
//   xmlns:tank="clr-namespace:DXApplication1"
//   <tank:CylindricalTankVisual Width="260" Height="120"
//       FillPercent="60" FuelColor="#FF4285F4" WaterPercent="5"/>
//
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DXApplication1;   // .NET 8: file-scoped namespace

/*
 * ╔══════════════════════════════════════════════════════════════════╗
 *  CylindricalTankVisual — WPF / .NET 8
 * ══════════════════════════════════════════════════════════════════════
 *  .NET 4.0 versiyonundan tek önemli fark:
 *    FormattedText → pixelsPerDip parametresi eklendi
 *    (VisualTreeHelper.GetDpi ile ekran DPI'ı alınır)
 *  Geri kalan API tamamen aynıdır.
 * ╚══════════════════════════════════════════════════════════════════╝
 */
public class CylindricalTankVisual : FrameworkElement
{
    // ═══════════════════════════════════════════════════════════
    //  DEPENDENCY PROPERTY'LER
    // ═══════════════════════════════════════════════════════════

    public static readonly DependencyProperty FillPercentProperty =
        DependencyProperty.Register(
            nameof(FillPercent), typeof(double), typeof(CylindricalTankVisual),
            new FrameworkPropertyMetadata(60.0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                null, CoercePercent));

    public static readonly DependencyProperty FuelColorProperty =
        DependencyProperty.Register(
            nameof(FuelColor), typeof(Color), typeof(CylindricalTankVisual),
            new FrameworkPropertyMetadata(Color.FromRgb(66, 133, 244),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty WaterPercentProperty =
        DependencyProperty.Register(
            nameof(WaterPercent), typeof(double), typeof(CylindricalTankVisual),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                null, CoercePercent));

    public static readonly DependencyProperty ShowPercentLabelProperty =
        DependencyProperty.Register(
            nameof(ShowPercentLabel), typeof(bool), typeof(CylindricalTankVisual),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowLevelDotProperty =
        DependencyProperty.Register(
            nameof(ShowLevelDot), typeof(bool), typeof(CylindricalTankVisual),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowWaterDotProperty =
        DependencyProperty.Register(
            nameof(ShowWaterDot), typeof(bool), typeof(CylindricalTankVisual),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowReferenceLineProperty =
        DependencyProperty.Register(
            nameof(ShowReferenceLine), typeof(bool), typeof(CylindricalTankVisual),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TankPaddingProperty =
        DependencyProperty.Register(
            nameof(TankPadding), typeof(double), typeof(CylindricalTankVisual),
            new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsRender));

    private static object CoercePercent(DependencyObject d, object baseValue)
    {
        double v = (double)baseValue;
        return v < 0.0 ? 0.0 : v > 100.0 ? 100.0 : v;
    }

    // ── CLR sarmalayıcılar (expression-bodied — .NET 8 stili)
    [Category("Tank")] public double FillPercent
    {
        get => (double)GetValue(FillPercentProperty);
        set => SetValue(FillPercentProperty, value);
    }

    [Category("Tank")] public Color FuelColor
    {
        get => (Color)GetValue(FuelColorProperty);
        set => SetValue(FuelColorProperty, value);
    }

    [Category("Tank")] public double WaterPercent
    {
        get => (double)GetValue(WaterPercentProperty);
        set => SetValue(WaterPercentProperty, value);
    }

    [Category("Tank")] public bool ShowPercentLabel
    {
        get => (bool)GetValue(ShowPercentLabelProperty);
        set => SetValue(ShowPercentLabelProperty, value);
    }

    [Category("Tank")] public bool ShowLevelDot
    {
        get => (bool)GetValue(ShowLevelDotProperty);
        set => SetValue(ShowLevelDotProperty, value);
    }

    [Category("Tank")] public bool ShowWaterDot
    {
        get => (bool)GetValue(ShowWaterDotProperty);
        set => SetValue(ShowWaterDotProperty, value);
    }

    [Category("Tank")] public bool ShowReferenceLine
    {
        get => (bool)GetValue(ShowReferenceLineProperty);
        set => SetValue(ShowReferenceLineProperty, value);
    }

    [Category("Tank")] public double TankPadding
    {
        get => (double)GetValue(TankPaddingProperty);
        set => SetValue(TankPaddingProperty, value);
    }

    // ═══════════════════════════════════════════════════════════
    //  CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════

    public CylindricalTankVisual()
    {
        Width  = 260;
        Height = 120;
    }

    // ═══════════════════════════════════════════════════════════
    //  OnRender
    // ═══════════════════════════════════════════════════════════

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        double pad = TankPadding;
        double tX  = pad;
        double tY  = pad;
        double tW  = ActualWidth  - pad * 2;
        double tH  = ActualHeight - pad * 2;

        if (tW < 10 || tH < 10) return;

        DrawCylindricalTank(dc, tX, tY, tW, tH);
    }

    // ═══════════════════════════════════════════════════════════
    //  DrawCylindricalTank
    // ═══════════════════════════════════════════════════════════

    private void DrawCylindricalTank(DrawingContext dc, double x, double y, double tw, double th)
    {
        double capW = Math.Min(th * 0.38, tw / 3.0);
        double fill = FillPercent / 100.0;

        Color fLight = LightenColor(FuelColor, 55);
        Color fDark  = DarkenColor(FuelColor, 30);

        Geometry capsule = BuildCapsuleGeometry(x, y, tw, th, capW);

        // ── 1. Gölge
        DrawDropShadow(dc, x, y, tw, th, capW);

        // ── 2. Arka plan
        dc.PushClip(capsule);
        dc.DrawGeometry(
            new LinearGradientBrush(
                Color.FromRgb(232, 232, 232),
                Color.FromRgb(205, 205, 205),
                new Point(0.5, 0), new Point(0.5, 1)),
            null, capsule);

        // ── 3. Yakıt
        if (fill > 0.0)
        {
            double fillH = th * fill;
            double fillY = y + th - fillH;

            var fuelClip = new CombinedGeometry(
                GeometryCombineMode.Intersect,
                capsule,
                new RectangleGeometry(new Rect(x, fillY, tw, fillH)));

            dc.PushClip(fuelClip);
            dc.DrawGeometry(
                new LinearGradientBrush(fLight, fDark, new Point(0.5, 0), new Point(0.5, 1)),
                null,
                new RectangleGeometry(new Rect(x, fillY, tw, fillH)));
            dc.Pop();

            if (fillY > y + 4)
                dc.DrawLine(
                    new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)), 1.5),
                    new Point(x + capW, fillY), new Point(x + tw - capW, fillY));
        }

        // ── 4. Su
        if (WaterPercent > 0)
        {
            double waterPx = Math.Max(3, th * WaterPercent / 100.0);
            double waterY  = y + th - waterPx;

            var waterClip = new CombinedGeometry(
                GeometryCombineMode.Intersect,
                capsule,
                new RectangleGeometry(new Rect(x, waterY, tw, waterPx)));

            dc.PushClip(waterClip);
            dc.DrawGeometry(
                new LinearGradientBrush(
                    Color.FromArgb(160, ColWater.R, ColWater.G, ColWater.B),
                    Color.FromArgb(220, DarkenColor(ColWater, 20).R,
                                        DarkenColor(ColWater, 20).G,
                                        DarkenColor(ColWater, 20).B),
                    new Point(0.5, 0), new Point(0.5, 1)),
                null,
                new RectangleGeometry(new Rect(x, waterY, tw, waterPx)));
            dc.Pop();

            if (ShowWaterDot)
            {
                dc.Pop(); // capsule clip'ten çık
                double wDotX = x + tw / 2.0;
                double wDotY = Math.Max(y + 6, Math.Min(y + th - 6, waterY));
                Color wc = DarkenColor(ColWater, 40);
                dc.DrawEllipse(
                    new SolidColorBrush(Color.FromArgb(220, wc.R, wc.G, wc.B)),
                    new Pen(new SolidColorBrush(DarkenColor(ColWater, 70)), 1.0),
                    new Point(wDotX, wDotY), 4, 4);
                dc.PushClip(capsule);
            }
        }

        // ── 5. Parlaklık
        dc.DrawRectangle(
            new LinearGradientBrush(
                Color.FromArgb(85, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                new Point(0.5, 0), new Point(0.5, 1)),
            null,
            new Rect(x + capW, y + 2, tw - capW * 2, th * 0.36));

        dc.Pop(); // capsule

        // ── 6. Çerçeve
        dc.DrawGeometry(null,
            new Pen(new SolidColorBrush(Color.FromRgb(175, 175, 175)), 1.8)
            { LineJoin = PenLineJoin.Round },
            capsule);

        // ── 7. Kapak gölgeleri
        DrawEndCap(dc, x,              y, capW, th, isLeft: true);
        DrawEndCap(dc, x + tw - capW * 2, y, capW, th, isLeft: false);

        // ── 8. Referans çizgisi
        if (ShowReferenceLine)
        {
            double midX = x + tw / 2.0;
            dc.DrawLine(
                new Pen(new SolidColorBrush(Color.FromArgb(65, 0, 0, 0)), 1.0)
                { DashStyle = DashStyles.Dash },
                new Point(midX, y + 6),
                new Point(midX, y + th - 6));
        }

        // ── 9. Seviye noktası
        if (ShowLevelDot)
        {
            double midX = x + tw / 2.0;
            double dotY = Math.Max(y + 8, Math.Min(y + th - 8, y + th - th * fill));
            dc.DrawEllipse(
                new SolidColorBrush(Color.FromArgb(170, 50, 50, 50)),
                null,
                new Point(midX, dotY), 4, 4);
        }

        // ── 10. Yüzde etiketi
        if (ShowPercentLabel)
        {
            double fontSize = Math.Clamp(th * 0.14, 9.0, 18.0);
            string text     = $"{FillPercent:F0}%";

            // .NET 8 / WPF: pixelsPerDip ZORUNLU — VisualTreeHelper ile alınır
            double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            var ft = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"),
                    FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                fontSize,
                new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                pixelsPerDip);   // ← .NET 4.0'da bu parametre YOK, .NET 8'de ZORUNLU

            double tx = x + tw * 0.35 - ft.Width / 2.0;
            double ty = y + (th - ft.Height) / 2.0;

            dc.DrawRoundedRectangle(
                new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)),
                null,
                new Rect(tx - 4, ty - 1, ft.Width + 8, ft.Height + 2),
                4, 4);

            dc.DrawText(ft, new Point(tx, ty));
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  YARDIMCI METODLAR
    // ═══════════════════════════════════════════════════════════

    private static readonly Color ColWater = Color.FromRgb(100, 180, 230);

    private static Geometry BuildCapsuleGeometry(double x, double y, double tw, double th, double capW)
    {
        double rx = capW;
        double ry = th / 2.0;

        var sg = new StreamGeometry();
        using var ctx = sg.Open();

        ctx.BeginFigure(new Point(x + rx, y), true, true);
        ctx.LineTo(new Point(x + tw - rx, y), true, false);
        ctx.ArcTo(new Point(x + tw - rx, y + th),
            new Size(rx, ry), 0, false, SweepDirection.Clockwise, true, false);
        ctx.LineTo(new Point(x + rx, y + th), true, false);
        ctx.ArcTo(new Point(x + rx, y),
            new Size(rx, ry), 0, false, SweepDirection.Clockwise, true, false);

        sg.Freeze();
        return sg;
    }

    private static void DrawDropShadow(DrawingContext dc, double x, double y,
                                        double tw, double th, double capW)
    {
        for (int i = 4; i >= 1; i--)
        {
            var sp = BuildCapsuleGeometry(x + i, y + i, tw, th, capW);
            dc.DrawGeometry(
                new SolidColorBrush(Color.FromArgb((byte)(14 * i), 0, 0, 0)),
                null, sp);
        }
    }

    private static void DrawEndCap(DrawingContext dc, double x, double y,
                                    double capW, double th, bool isLeft)
    {
        var ellipse  = new EllipseGeometry(new Point(x + capW, y + th / 2.0), capW, th / 2.0);
        var clipRect = new RectangleGeometry(new Rect(x, y, capW * 2, th));
        var clipped  = new CombinedGeometry(GeometryCombineMode.Intersect, ellipse, clipRect);

        dc.DrawGeometry(
            new LinearGradientBrush(
                isLeft ? Color.FromArgb(55, 0, 0, 0) : Color.FromArgb(0, 0, 0, 0),
                isLeft ? Color.FromArgb(0, 0, 0, 0)  : Color.FromArgb(55, 0, 0, 0),
                new Point(0, 0.5), new Point(1, 0.5)),
            null, clipped);
    }

    private static Color LightenColor(Color c, int a) =>
        Color.FromArgb(c.A,
            (byte)Math.Min(255, c.R + a),
            (byte)Math.Min(255, c.G + a),
            (byte)Math.Min(255, c.B + a));

    private static Color DarkenColor(Color c, int a) =>
        Color.FromArgb(c.A,
            (byte)Math.Max(0, c.R - a),
            (byte)Math.Max(0, c.G - a),
            (byte)Math.Max(0, c.B - a));
}
