//-----------------------------------------------------------------------
// <copyright file="CylindricalTankVisual.cs" company="PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.">
//     Author: MUSTAFA CAGRI ALTINDAL
//     Copyright (c) PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Butex.TankScreen
{
    /*
 * ╔══════════════════════════════════════════════════════════════════╗
 *  CylindricalTankVisual — Responsive Yatay Silindirik Tank Görseli
 * ══════════════════════════════════════════════════════════════════════
 *  Sadece tankın görsel çizimini içerir. Kontrol boyutu ne olursa
 *  olsun tank orantılı şekilde tüm alanı doldurur.
 *
 *  Kullanım:
 *    var tank = new CylindricalTankVisual
 *    {
 *        Dock = DockStyle.Fill,    // veya Anchor ile responsive
 *        FillPercent = 60,
 *        FuelColor = Color.FromArgb(66, 133, 244),
 *        WaterHeight = 0,
 *    };
 *
 *  Katmanlar (painter's algorithm):
 *    1. Drop shadow
 *    2. Boş tank arka planı (gradient gri)
 *    3. Yakıt dolumu (gradient renkli)
 *    4. Su katmanı (kırmızı şerit)
 *    5. Üst parlaklık yansıması
 *    6. Dış çerçeve
 *    7. Eliptik uç kapaklar (3D derinlik)
 *    8. Orta referans çizgisi + seviye noktası
 *    9. Yüzde etiketi
 * ╚══════════════════════════════════════════════════════════════════╝
 */

    [ToolboxItem(true)]
    [Designer(typeof(ControlDesigner))]
    public partial class CylindricalTankVisual : UserControl
    {
        // ═══════════════════════════════════════════════════════════
        //  PRIVATE ALANLAR
        // ═══════════════════════════════════════════════════════════

        private double _fillPercent = 50.0;
        private Color _fuelColor = Color.Olive;
        private double _waterPercent = 5.0;
        private bool _showPercentLabel = true;
        private bool _showLevelDot = true;
        private bool _showWaterDot = true;
        private bool _showReferenceLine = true;
        private int _padding = 6;
        private static readonly Color ColWater = Color.FromArgb(100, 180, 230);

        //private static readonly Color ColWater = Color.FromArgb(170, 50, 50, 50);

        // ═══════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public CylindricalTankVisual()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ResizeRedraw = true;
            Size = new Size(260, 120);
            BackColor = DesignMode ? Color.White : Color.Transparent;
        }

        // ═══════════════════════════════════════════════════════════
        //  PUBLIC PROPERTY'LER
        // ═══════════════════════════════════════════════════════════

        [Category("Tank")]
        [DefaultValue(60.0)]
        [Description("Tankın doluluk yüzdesi (0–100).")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double FillPercent
        {
            get => _fillPercent;
            set
            {
                _fillPercent = Math.Max(0, Math.Min(100, value));
                Invalidate();
            }
        }

        [Category("Tank")]
        [Description("Yakıt rengi.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color FuelColor
        {
            get => _fuelColor;
            set
            {
                _fuelColor = value;
                Invalidate();
            }
        }

        [Category("Tank")]
        [DefaultValue(0.0)]
        [Description(
            "Su doluluk yüzdesi (0–100). FillPercent gibi çalışır; tankın tabanından bu yüzde kadar mavi su şeridi çizilir.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double WaterPercent
        {
            get => _waterPercent;
            set
            {
                _waterPercent = Math.Max(0, Math.Min(100, value));
                Invalidate();
            }
        }

        [Category("Tank")]
        [DefaultValue(true)]
        [Description("Yüzde etiketi gösterilsin mi?")]
        public bool ShowPercentLabel
        {
            get => _showPercentLabel;
            set
            {
                _showPercentLabel = value;
                Invalidate();
            }
        }

        [Category("Tank")]
        [DefaultValue(true)]
        [Description("Seviye noktası (dot) gösterilsin mi?")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ShowLevelDot
        {
            get => _showLevelDot;
            set
            {
                _showLevelDot = value;
                Invalidate();
            }
        }

        [Category("Tank")]
        [DefaultValue(true)]
        [Description("Su yüzeyi noktası gösterilsin mi? WaterPercent > 0 olduğunda su yüzeyine mavi bir nokta çizer.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ShowWaterDot
        {
            get => _showWaterDot;
            set
            {
                _showWaterDot = value;
                Invalidate();
            }
        }

        [Category("Tank")]
        [DefaultValue(true)]
        [Description("Orta referans çizgisi gösterilsin mi?")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ShowReferenceLine
        {
            get => _showReferenceLine;
            set
            {
                _showReferenceLine = value;
                Invalidate();
            }
        }

        [Category("Tank")]
        [DefaultValue(6)]
        [Description("Tank etrafındaki iç boşluk (piksel). Gölge için alan bırakır.")]
        public new int Padding
        {
            get => _padding;
            set
            {
                _padding = Math.Max(0, value);
                Invalidate();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  OnPaint — ANA ÇİZİM
        //
        //  Tank boyutları ClientSize'dan hesaplanır; kontrol hangi
        //  boyutta olursa olsun tank tüm alanı doldurur.
        // ═══════════════════════════════════════════════════════════

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int W = ClientSize.Width;
            int H = ClientSize.Height;

            // Tank sınırları: padding kadar içeride
            int tLeft = _padding;
            int tTop = _padding;
            int tW = W - _padding * 2;
            int tH = H - _padding * 2;

            if(tW < 10 || tH < 10)
                return; // Çok küçükse çizme

            DrawCylindricalTank(g, tLeft, tTop, tW, tH);
        }

        // ═══════════════════════════════════════════════════════════
        //  DrawCylindricalTank — SİLİNDİRİK TANK ÇİZİMİ
        //
        //  Tüm boyutlar tw/th parametrelerine orantılı hesaplanır,
        //  böylece kontrol responsive çalışır.
        // ═══════════════════════════════════════════════════════════

        private void DrawCylindricalTank(Graphics g, int x, int y, int tw, int th)
        {
            // capW: Eliptik uç kapak yarı genişliği — th'ye orantılı
            int capW = (int)(th * 0.38);
            capW = Math.Min(capW, tw / 3); // Çok dar kontrollerde taşmayı önle

            float fill = (float)(_fillPercent / 100.0);
            Color fLight = LightenColor(_fuelColor, 55);
            Color fDark = DarkenColor(_fuelColor, 30);

            // ── 1. Gölge
            DrawDropShadow(g, x, y, tw, th, capW);

            // ── 2. Kapsül path (clip mask)
            GraphicsPath capsule = BuildCapsulePath(x, y, tw, th, capW);
            g.SetClip(capsule);

            // Boş tank arka planı
            using(var bgBrush = new LinearGradientBrush(
                new Point(x, y),
                new Point(x, y + th),
                Color.FromArgb(232, 232, 232),
                Color.FromArgb(205, 205, 205)))
                g.FillPath(bgBrush, capsule);

            // ── 3. Yakıt dolumu
            if(fill > 0f)
            {
                int fillH = (int)(th * fill);
                int fillY = y + th - fillH;
                var fuelRect = new Rectangle(x, fillY, tw, fillH);

                var fuelRegion = new Region(capsule);
                fuelRegion.Intersect(fuelRect);
                g.SetClip(fuelRegion, CombineMode.Replace);

                using var fuelBrush = new LinearGradientBrush(
                    new Point(x, fillY),
                    new Point(x, y + th),
                    fLight,
                    fDark);
                g.FillRectangle(fuelBrush, fuelRect);

                // Yakıt yüzey çizgisi
                if(fillY > y + 4)
                {
                    g.SetClip(capsule);
                    using var surfPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1.5f);
                    g.DrawLine(surfPen, x + capW, fillY, x + tw - capW, fillY);
                }

                fuelRegion.Dispose();
            }

            // ── 4. Su katmanı (açık mavi — WaterPercent doğrudan tank yüksekliğiyle oranlanır)
            if(_waterPercent > 0)
            {
                g.SetClip(capsule);
                int waterPx = Math.Max(3, (int)(th * _waterPercent / 100.0));
                var waterRect = new Rectangle(x, y + th - waterPx, tw, waterPx);
                using var wb = new LinearGradientBrush(
                    new Point(x, y + th - waterPx),
                    new Point(x, y + th),
                    Color.FromArgb(160, LightenColor(ColWater, 30)),
                    Color.FromArgb(220, DarkenColor(ColWater, 20)));
                g.FillRectangle(wb, waterRect);

                // Su yüzeyi noktası: yakıt noktasıyla aynı dikey çizgide (midX), su yükseldikçe yukarı çıkar
                if(_showWaterDot)
                {
                    g.ResetClip();
                    int wDotX = x + tw / 2; // yakıt noktasıyla aynı merkez
                    int wDotY = y + th - waterPx;
                    wDotY = Math.Max(y + 6, Math.Min(y + th - 6, wDotY));
                    //using var wDotFill = new SolidBrush(Color.FromArgb(220, DarkenColor(ColWater, 40)));
                    using var wDotFill = new SolidBrush(Color.FromArgb(170, 50, 50, 50));
                    using var wDotBorder = new Pen(Color.FromArgb(255, DarkenColor(ColWater, 70)), 1f);
                    g.FillEllipse(wDotFill, wDotX - 4, wDotY - 4, 8, 8);
                    g.DrawEllipse(wDotBorder, wDotX - 4, wDotY - 4, 8, 8);
                    g.SetClip(capsule);
                }
            }

            // ── 5. Üst parlaklık yansıması
            g.SetClip(capsule);
            using(var glareBrush = new LinearGradientBrush(
                new PointF(x, y),
                new PointF(x, y + th * 0.38f),
                Color.FromArgb(85, 255, 255, 255),
                Color.Transparent))
                g.FillRectangle(glareBrush, x + capW, y + 2, tw - capW * 2, (int)(th * 0.36f));

            g.ResetClip();
            capsule.Dispose();

            // ── 6. Dış çerçeve
            GraphicsPath outline = BuildCapsulePath(x, y, tw, th, capW);
            using(var outlinePen = new Pen(Color.FromArgb(175, 175, 175), 1.8f))
                g.DrawPath(outlinePen, outline);
            outline.Dispose();

            // ── 7. Eliptik uç kapaklar
            DrawEndCap(g, x, y, capW, th, isLeft: true);
            DrawEndCap(g, x + tw - capW * 2, y, capW, th, isLeft: false);

            // ── 8. Orta referans çizgisi
            if(_showReferenceLine)
            {
                int midX = x + tw / 2;
                using var midPen = new Pen(Color.FromArgb(65, 0, 0, 0), 1f);
                midPen.DashStyle = DashStyle.Dash;
                g.DrawLine(midPen, midX, y + 6, midX, y + th - 6);
            }

            // ── 9. Seviye noktası
            if(_showLevelDot)
            {
                int midX = x + tw / 2;
                int dotY = y + th - (int)(th * fill);
                dotY = Math.Max(y + 8, Math.Min(y + th - 8, dotY));
                using var dotBrush = new SolidBrush(Color.FromArgb(170, 50, 50, 50));
                g.FillEllipse(dotBrush, midX - 4, dotY - 4, 8, 8);
            }

            // ── 10. Yüzde etiketi — orta solda, seviye noktasının solunda
            if(_showPercentLabel)
            {
                float fontSize = Math.Max(9f, Math.Min(18f, th * 0.14f));
                using var pFont = new Font("Segoe UI Semibold", fontSize, FontStyle.Bold);
                string pStr = $"{_fillPercent:F0}%";
                var pSize = g.MeasureString(pStr, pFont);

                // Merkez noktanın soluna kaydır: tankın %35'i (merkez = %50)
                float tx = x + tw * 0.35f - pSize.Width / 2f;
                float ty = y + (th - pSize.Height) / 2f;

                // Yarı saydam beyaz arka plan (okunabilirlik için)
                using var bgBrush = new SolidBrush(Color.FromArgb(160, 255, 255, 255));
                var bgRect = new RectangleF(tx - 4, ty - 1, pSize.Width + 8, pSize.Height + 2);
                using var bgPath = new GraphicsPath();
                int r = 4;
                bgPath.AddArc(bgRect.X, bgRect.Y, r * 2, r * 2, 180, 90);
                bgPath.AddArc(bgRect.Right - r * 2, bgRect.Y, r * 2, r * 2, 270, 90);
                bgPath.AddArc(bgRect.Right - r * 2, bgRect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                bgPath.AddArc(bgRect.X, bgRect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                bgPath.CloseFigure();
                g.FillPath(bgBrush, bgPath);

                using var textBrush = new SolidBrush(Color.FromArgb(50, 50, 50));
                g.DrawString(pStr, pFont, textBrush, tx, ty);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  YARDIMCI ÇİZİM METODLARI
        // ═══════════════════════════════════════════════════════════

        private void DrawEndCap(Graphics g, int x, int y, int capW, int th, bool isLeft)
        {
            var capRect = new Rectangle(x, y, capW * 2, th);
            var region = new Region(capRect);
            g.SetClip(region, CombineMode.Replace);

            using(var depthBrush = new LinearGradientBrush(
                new Point(x, y),
                new Point(x + capW * 2, y),
                isLeft ? Color.FromArgb(55, 0, 0, 0) : Color.Transparent,
                isLeft ? Color.Transparent : Color.FromArgb(55, 0, 0, 0)))
                g.FillEllipse(depthBrush, capRect);

            g.ResetClip();
            region.Dispose();
        }

        private GraphicsPath BuildCapsulePath(int x, int y, int tw, int th, int capW)
        {
            var path = new GraphicsPath();
            path.AddArc(x, y, capW * 2, th, 90, 180);
            path.AddArc(x + tw - capW * 2, y, capW * 2, th, 270, 180);
            path.CloseFigure();
            return path;
        }

        private void DrawDropShadow(Graphics g, int x, int y, int tw, int th, int capW)
        {
            for(int i = 4; i >= 1; i--)
            {
                var sp = BuildCapsulePath(x + i, y + i, tw, th, capW);
                using var sb = new SolidBrush(Color.FromArgb(14 * i, 0, 0, 0));
                g.FillPath(sb, sp);
                sp.Dispose();
            }
        }

        private static Color LightenColor(Color c, int a)
        {
            return Color.FromArgb(
                c.A,
                Math.Min(255, c.R + a),
                Math.Min(255, c.G + a),
                Math.Min(255, c.B + a));
        }

        private static Color DarkenColor(Color c, int a)
        {
            return Color.FromArgb(
                c.A,
                Math.Max(0, c.R - a),
                Math.Max(0, c.G - a),
                Math.Max(0, c.B - a));
        }
    }
}
