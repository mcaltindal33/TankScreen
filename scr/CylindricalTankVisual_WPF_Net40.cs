//-----------------------------------------------------------------------
// <copyright file="CylindricalTankVisual.cs" company="PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.">
//     Author: MUSTAFA CAGRI ALTINDAL
//     Copyright (c) PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
//
// HEDEF: WPF + .NET Framework 4.0
//
// KULLANIM (XAML):
//   xmlns:tank="clr-namespace:DXApplication1"
//   <tank:CylindricalTankVisual Width="260" Height="120"
//       FillPercent="60" FuelColor="#FF4285F4" WaterPercent="5"/>
//
// KULLANIM (CODE-BEHIND):
//   var tank = new CylindricalTankVisual();
//   tank.Width  = 260;
//   tank.Height = 120;
//   tank.FillPercent  = 60;
//   tank.FuelColor    = Color.FromRgb(66, 133, 244);  // System.Windows.Media.Color
//   tank.WaterPercent = 5;
//   myGrid.Children.Add(tank);
//
// BAĞIMLILIK: Sadece WPF'in standart System.Windows.Media namespace'i kullanılır.
//             DevExpress veya System.Drawing referansı GEREKMİYOR.
//
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace DXApplication1
{
    /*
     * ╔══════════════════════════════════════════════════════════════════╗
     *  CylindricalTankVisual — WPF / .NET Framework 4.0
     * ══════════════════════════════════════════════════════════════════════
     *  FrameworkElement türetmesi + OnRender override ile çalışır.
     *  DependencyProperty sistemi sayesinde XAML binding ve animasyon
     *  tam olarak desteklenir.
     *
     *  Katmanlar (painter's algorithm):
     *    1. Drop shadow (yarı saydam dolgular)
     *    2. Boş tank arka planı (gradient gri)
     *    3. Yakıt dolumu  (gradient renkli, kapsüle clip)
     *    4. Su katmanı    (mavi şerit)
     *    5. Üst parlaklık yansıması
     *    6. Dış çerçeve
     *    7. Eliptik uç kapak gölgeleri (3D derinlik)
     *    8. Orta referans çizgisi
     *    9. Seviye noktası + su noktası
     *   10. Yüzde etiketi
     * ╚══════════════════════════════════════════════════════════════════╝
     */
    public class CylindricalTankVisual : FrameworkElement
    {
        // ═══════════════════════════════════════════════════════════
        //  DEPENDENCY PROPERTY'LER
        //  WPF'de binding, animasyon ve XAML desteği için zorunludur.
        // ═══════════════════════════════════════════════════════════

        public static readonly DependencyProperty FillPercentProperty =
            DependencyProperty.Register(
                "FillPercent",
                typeof(double),
                typeof(CylindricalTankVisual),
                new FrameworkPropertyMetadata(
                    60.0,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,
                    CoerceFillPercent));

        public static readonly DependencyProperty FuelColorProperty =
            DependencyProperty.Register(
                "FuelColor",
                typeof(Color),
                typeof(CylindricalTankVisual),
                new FrameworkPropertyMetadata(
                    Color.FromRgb(66, 133, 244),
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty WaterPercentProperty =
            DependencyProperty.Register(
                "WaterPercent",
                typeof(double),
                typeof(CylindricalTankVisual),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,
                    CoerceFillPercent));

        public static readonly DependencyProperty ShowPercentLabelProperty =
            DependencyProperty.Register(
                "ShowPercentLabel",
                typeof(bool),
                typeof(CylindricalTankVisual),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowLevelDotProperty =
            DependencyProperty.Register(
                "ShowLevelDot",
                typeof(bool),
                typeof(CylindricalTankVisual),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowWaterDotProperty =
            DependencyProperty.Register(
                "ShowWaterDot",
                typeof(bool),
                typeof(CylindricalTankVisual),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowReferenceLineProperty =
            DependencyProperty.Register(
                "ShowReferenceLine",
                typeof(bool),
                typeof(CylindricalTankVisual),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TankPaddingProperty =
            DependencyProperty.Register(
                "TankPadding",
                typeof(double),
                typeof(CylindricalTankVisual),
                new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsRender));

        // ── Coerce: değeri 0–100 arasına sıkıştırır
        private static object CoerceFillPercent(DependencyObject d, object baseValue)
        {
            double v = (double)baseValue;
            if (v < 0.0) return 0.0;
            if (v > 100.0) return 100.0;
            return v;
        }

        // ── CLR sarmalayıcılar (XAML ve code-behind için)
        [Category("Tank")]
        [Description("Tankın doluluk yüzdesi (0–100).")]
        public double FillPercent
        {
            get { return (double)GetValue(FillPercentProperty); }
            set { SetValue(FillPercentProperty, value); }
        }

        [Category("Tank")]
        [Description("Yakıt rengi (System.Windows.Media.Color).")]
        public Color FuelColor
        {
            get { return (Color)GetValue(FuelColorProperty); }
            set { SetValue(FuelColorProperty, value); }
        }

        [Category("Tank")]
        [Description("Su doluluk yüzdesi (0–100).")]
        public double WaterPercent
        {
            get { return (double)GetValue(WaterPercentProperty); }
            set { SetValue(WaterPercentProperty, value); }
        }

        [Category("Tank")]
        [Description("Yüzde etiketini göster.")]
        public bool ShowPercentLabel
        {
            get { return (bool)GetValue(ShowPercentLabelProperty); }
            set { SetValue(ShowPercentLabelProperty, value); }
        }

        [Category("Tank")]
        [Description("Yakıt seviye noktasını göster.")]
        public bool ShowLevelDot
        {
            get { return (bool)GetValue(ShowLevelDotProperty); }
            set { SetValue(ShowLevelDotProperty, value); }
        }

        [Category("Tank")]
        [Description("Su yüzeyi noktasını göster.")]
        public bool ShowWaterDot
        {
            get { return (bool)GetValue(ShowWaterDotProperty); }
            set { SetValue(ShowWaterDotProperty, value); }
        }

        [Category("Tank")]
        [Description("Orta referans çizgisini göster.")]
        public bool ShowReferenceLine
        {
            get { return (bool)GetValue(ShowReferenceLineProperty); }
            set { SetValue(ShowReferenceLineProperty, value); }
        }

        [Category("Tank")]
        [Description("Tank etrafındaki iç boşluk (piksel).")]
        public double TankPadding
        {
            get { return (double)GetValue(TankPaddingProperty); }
            set { SetValue(TankPaddingProperty, value); }
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
        //  OnRender — ANA ÇİZİM (WPF'de OnPaint'in karşılığı)
        //
        //  WPF retained-mode: sistem ihtiyaç duyduğunda bu metodu
        //  otomatik çağırır. AffectsRender bayrağı sayesinde
        //  property değişince de tetiklenir.
        // ═══════════════════════════════════════════════════════════

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double W = ActualWidth;
            double H = ActualHeight;
            double pad = TankPadding;

            double tX = pad;
            double tY = pad;
            double tW = W - pad * 2;
            double tH = H - pad * 2;

            if (tW < 10 || tH < 10)
                return;

            DrawCylindricalTank(dc, tX, tY, tW, tH);
        }

        // ═══════════════════════════════════════════════════════════
        //  DrawCylindricalTank
        // ═══════════════════════════════════════════════════════════

        private void DrawCylindricalTank(DrawingContext dc, double x, double y, double tw, double th)
        {
            // capW: Eliptik uç kapak yarı genişliği
            double capW = th * 0.38;
            if (capW > tw / 3.0)
                capW = tw / 3.0;

            double fill      = FillPercent / 100.0;
            Color  fuelColor = FuelColor;
            Color  fLight    = LightenColor(fuelColor, 55);
            Color  fDark     = DarkenColor(fuelColor, 30);

            // Kapsül geometrisi (clip için)
            Geometry capsule = BuildCapsuleGeometry(x, y, tw, th, capW);

            // ── 1. Drop shadow
            DrawDropShadow(dc, x, y, tw, th, capW);

            // ── 2. Boş tank arka planı
            dc.PushClip(capsule);
            LinearGradientBrush bgBrush = new LinearGradientBrush(
                Color.FromRgb(232, 232, 232),
                Color.FromRgb(205, 205, 205),
                new Point(0.5, 0), new Point(0.5, 1));
            dc.DrawGeometry(bgBrush, null, capsule);

            // ── 3. Yakıt dolumu
            if (fill > 0.0)
            {
                double fillH = th * fill;
                double fillY = y + th - fillH;

                RectangleGeometry fuelRect = new RectangleGeometry(
                    new Rect(x, fillY, tw, fillH));

                CombinedGeometry fuelClip = new CombinedGeometry(
                    GeometryCombineMode.Intersect, capsule, fuelRect);

                dc.PushClip(fuelClip);

                // Gradyan: WPF'de RelativeTransform kullanarak piksel koordinatlarını eşleriz
                LinearGradientBrush fuelBrush = new LinearGradientBrush(
                    fLight, fDark, new Point(0.5, 0), new Point(0.5, 1));

                dc.DrawGeometry(fuelBrush, null,
                    new RectangleGeometry(new Rect(x, fillY, tw, fillH)));
                dc.Pop(); // fuelClip

                // Yakıt yüzey çizgisi
                if (fillY > y + 4)
                {
                    Pen surfPen = new Pen(
                        new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)), 1.5);
                    dc.DrawLine(surfPen,
                        new Point(x + capW, fillY),
                        new Point(x + tw - capW, fillY));
                }
            }

            // ── 4. Su katmanı
            if (WaterPercent > 0)
            {
                double waterPx = th * WaterPercent / 100.0;
                if (waterPx < 3) waterPx = 3;

                double waterY = y + th - waterPx;

                RectangleGeometry waterRect = new RectangleGeometry(
                    new Rect(x, waterY, tw, waterPx));
                CombinedGeometry waterClip = new CombinedGeometry(
                    GeometryCombineMode.Intersect, capsule, waterRect);

                dc.PushClip(waterClip);
                LinearGradientBrush waterBrush = new LinearGradientBrush(
                    Color.FromArgb(160, ColWater.R, ColWater.G, ColWater.B),
                    Color.FromArgb(220, DarkenColor(ColWater, 20).R,
                                        DarkenColor(ColWater, 20).G,
                                        DarkenColor(ColWater, 20).B),
                    new Point(0.5, 0), new Point(0.5, 1));
                dc.DrawGeometry(waterBrush, null,
                    new RectangleGeometry(new Rect(x, waterY, tw, waterPx)));
                dc.Pop(); // waterClip

                // Su yüzeyi noktası
                if (ShowWaterDot)
                {
                    dc.Pop(); // capsule clip'ten geçici çık
                    double wDotX = x + tw / 2.0;
                    double wDotY = waterY;
                    if (wDotY < y + 6) wDotY = y + 6;
                    if (wDotY > y + th - 6) wDotY = y + th - 6;

                    Color wDotColor = DarkenColor(ColWater, 40);
                    Color wBorderColor = DarkenColor(ColWater, 70);
                    dc.DrawEllipse(
                        new SolidColorBrush(Color.FromArgb(220, wDotColor.R, wDotColor.G, wDotColor.B)),
                        new Pen(new SolidColorBrush(wBorderColor), 1.0),
                        new Point(wDotX, wDotY), 4, 4);
                    dc.PushClip(capsule); // geri al
                }
            }

            // ── 5. Üst parlaklık
            LinearGradientBrush glareBrush = new LinearGradientBrush(
                Color.FromArgb(85, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                new Point(0.5, 0), new Point(0.5, 1));
            dc.DrawRectangle(glareBrush, null,
                new Rect(x + capW, y + 2, tw - capW * 2, th * 0.36));

            dc.Pop(); // capsule clip

            // ── 6. Dış çerçeve
            Pen outlinePen = new Pen(
                new SolidColorBrush(Color.FromRgb(175, 175, 175)), 1.8);
            outlinePen.LineJoin = PenLineJoin.Round;
            dc.DrawGeometry(null, outlinePen, capsule);

            // ── 7. Uç kapak gölgeleri
            DrawEndCap(dc, x, y, capW, th, true);
            DrawEndCap(dc, x + tw - capW * 2, y, capW, th, false);

            // ── 8. Orta referans çizgisi
            if (ShowReferenceLine)
            {
                double midX = x + tw / 2.0;
                Pen midPen = new Pen(
                    new SolidColorBrush(Color.FromArgb(65, 0, 0, 0)), 1.0);
                midPen.DashStyle = DashStyles.Dash;
                dc.DrawLine(midPen,
                    new Point(midX, y + 6),
                    new Point(midX, y + th - 6));
            }

            // ── 9. Seviye noktası
            if (ShowLevelDot)
            {
                double midX = x + tw / 2.0;
                double dotY = y + th - th * fill;
                if (dotY < y + 8) dotY = y + 8;
                if (dotY > y + th - 8) dotY = y + th - 8;

                dc.DrawEllipse(
                    new SolidColorBrush(Color.FromArgb(170, 50, 50, 50)),
                    null,
                    new Point(midX, dotY), 4, 4);
            }

            // ── 10. Yüzde etiketi
            if (ShowPercentLabel)
            {
                double fontSize = th * 0.14;
                if (fontSize < 9) fontSize = 9;
                if (fontSize > 18) fontSize = 18;

                string text = string.Format("{0:F0}%", FillPercent);

                FormattedText ft = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"),
                        FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                    fontSize,
                    Brushes.Black);

                double tx = x + tw * 0.35 - ft.Width / 2.0;
                double ty = y + (th - ft.Height) / 2.0;

                // Arka plan
                Rect bgRect = new Rect(tx - 4, ty - 1, ft.Width + 8, ft.Height + 2);
                dc.DrawRoundedRectangle(
                    new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)),
                    null,
                    bgRect, 4, 4);

                ft.SetForegroundBrush(new SolidColorBrush(Color.FromRgb(50, 50, 50)));
                dc.DrawText(ft, new Point(tx, ty));
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  YARDIMCI METODLAR
        // ═══════════════════════════════════════════════════════════

        private static readonly Color ColWater = Color.FromRgb(100, 180, 230);

        private Geometry BuildCapsuleGeometry(double x, double y, double tw, double th, double capW)
        {
            // StreamGeometry WPF'de GraphicsPath'in karşılığıdır
            StreamGeometry sg = new StreamGeometry();
            using (StreamGeometryContext ctx = sg.Open())
            {
                double rx = capW;   // yatay yarıçap
                double ry = th / 2; // dikey yarıçap

                // Sol yay (180° → 360°, yani sol yarım daire)
                Point leftCenter  = new Point(x + rx, y + ry);
                Point rightCenter = new Point(x + tw - rx, y + ry);

                ctx.BeginFigure(new Point(x + rx, y), true, true);
                // Üst düz çizgi
                ctx.LineTo(new Point(x + tw - rx, y), true, false);
                // Sağ yarım daire (üstten alt)
                ctx.ArcTo(new Point(x + tw - rx, y + th),
                    new Size(rx, ry), 0, false, SweepDirection.Clockwise, true, false);
                // Alt düz çizgi
                ctx.LineTo(new Point(x + rx, y + th), true, false);
                // Sol yarım daire (alttan üst)
                ctx.ArcTo(new Point(x + rx, y),
                    new Size(rx, ry), 0, false, SweepDirection.Clockwise, true, false);
            }
            sg.Freeze(); // performans: immutable yap
            return sg;
        }

        private void DrawDropShadow(DrawingContext dc, double x, double y,
                                     double tw, double th, double capW)
        {
            for (int i = 4; i >= 1; i--)
            {
                Geometry sp = BuildCapsuleGeometry(x + i, y + i, tw, th, capW);
                dc.DrawGeometry(
                    new SolidColorBrush(Color.FromArgb((byte)(14 * i), 0, 0, 0)),
                    null, sp);
            }
        }

        private void DrawEndCap(DrawingContext dc, double x, double y,
                                  double capW, double th, bool isLeft)
        {
            EllipseGeometry ellipse = new EllipseGeometry(
                new Point(x + capW, y + th / 2.0), capW, th / 2.0);

            RectangleGeometry clipRect = new RectangleGeometry(
                new Rect(x, y, capW * 2, th));

            CombinedGeometry clipped = new CombinedGeometry(
                GeometryCombineMode.Intersect, ellipse, clipRect);

            LinearGradientBrush depthBrush = new LinearGradientBrush(
                isLeft ? Color.FromArgb(55, 0, 0, 0) : Color.FromArgb(0, 0, 0, 0),
                isLeft ? Color.FromArgb(0, 0, 0, 0) : Color.FromArgb(55, 0, 0, 0),
                new Point(0, 0.5), new Point(1, 0.5));

            dc.DrawGeometry(depthBrush, null, clipped);
        }

        // ── Renk yardımcıları
        private static Color LightenColor(Color c, int a)
        {
            return Color.FromArgb(c.A,
                (byte)Math.Min(255, c.R + a),
                (byte)Math.Min(255, c.G + a),
                (byte)Math.Min(255, c.B + a));
        }

        private static Color DarkenColor(Color c, int a)
        {
            return Color.FromArgb(c.A,
                (byte)Math.Max(0, c.R - a),
                (byte)Math.Max(0, c.G - a),
                (byte)Math.Max(0, c.B - a));
        }
    }
}
