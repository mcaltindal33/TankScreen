//-----------------------------------------------------------------------
// <copyright file="CylindricalTankVisual.cs" company="PETRODATA BILISIM DANISMANLIK EGITIM ELKTRONIK OTMASYON SAN VE TIC. LTD. STI.">
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
    /*
     * ╔══════════════════════════════════════════════════════════════════╗
     *  CylindricalTankVisual — Responsive Yatay Silindirik Tank Görseli
     * ══════════════════════════════════════════════════════════════════════
     *  Sadece tankın görsel çizimini içerir. Kontrol boyutu ne olursa
     *  olsun tank orantılı şekilde tüm alanı doldurur.
     *
     *  Kullanım:
     *    var tank = new CylindricalTankVisual();
     *    tank.Dock = DockStyle.Fill;
     *    tank.FillPercent = 60;
     *    tank.FuelColor = Color.FromArgb(66, 133, 244);
     *    tank.WaterPercent = 0;
     *
     *  Katmanlar (painter's algorithm):
     *    1. Drop shadow
     *    2. Boş tank arka planı (gradient gri)
     *    3. Yakıt dolumu (gradient renkli)
     *    4. Su katmanı (mavi şerit)
     *    5. Üst parlaklık yansıması
     *    6. Dış çerçeve
     *    7. Eliptik uç kapaklar (3D derinlik)
     *    8. Orta referans çizgisi + seviye noktası
     *    9. Yüzde etiketi
     * ╚══════════════════════════════════════════════════════════════════╝
     */

    [ToolboxItem(true)]
    public partial class CylindricalTankVisual : UserControl
    {
        // ═══════════════════════════════════════════════════════════
        //  PRIVATE ALANLAR
        // ═══════════════════════════════════════════════════════════

        private double _fillPercent = 60.0;
        private Color _fuelColor = Color.FromArgb(66, 133, 244);
        private double _waterPercent = 0.0;
        private bool _showPercentLabel = true;
        private bool _showLevelDot = true;
        private bool _showWaterDot = true;
        private bool _showReferenceLine = true;
        private int _padding = 6;

        private static readonly Color ColWater = Color.FromArgb(100, 180, 230);

        // ═══════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public CylindricalTankVisual()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.Size = new Size(260, 120);
            this.BackColor = DesignMode ? Color.White : Color.Transparent;
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
            get { return _fillPercent; }
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
            get { return _fuelColor; }
            set
            {
                _fuelColor = value;
                Invalidate();
            }
        }

        [Category("Tank")]
        [DefaultValue(0.0)]
        [Description("Su doluluk yüzdesi (0–100). Tankın tabanından bu yüzde kadar mavi su şeridi çizilir.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double WaterPercent
        {
            get { return _waterPercent; }
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
            get { return _showPercentLabel; }
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
            get { return _showLevelDot; }
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
            get { return _showWaterDot; }
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
            get { return _showReferenceLine; }
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
            get { return _padding; }
            set
            {
                _padding = Math.Max(0, value);
                Invalidate();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  OnPaint — ANA ÇİZİM
        // ═══════════════════════════════════════════════════════════

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int W = ClientSize.Width;
            int H = ClientSize.Height;

            int tLeft = _padding;
            int tTop = _padding;
            int tW = W - _padding * 2;
            int tH = H - _padding * 2;

            if (tW < 10 || tH < 10)
                return;

            DrawCylindricalTank(g, tLeft, tTop, tW, tH);
        }

        // ═══════════════════════════════════════════════════════════
        //  DrawCylindricalTank — SİLİNDİRİK TANK ÇİZİMİ
        // ═══════════════════════════════════════════════════════════

        private void DrawCylindricalTank(Graphics g, int x, int y, int tw, int th)
        {
            int capW = (int)(th * 0.38);
            if (capW > tw / 3)
                capW = tw / 3;

            float fill = (float)(_fillPercent / 100.0);
            Color fLight = LightenColor(_fuelColor, 55);
            Color fDark = DarkenColor(_fuelColor, 30);

            // ── 1. Gölge
            DrawDropShadow(g, x, y, tw, th, capW);

            // ── 2. Kapsül path (clip mask)
            GraphicsPath capsule = BuildCapsulePath(x, y, tw, th, capW);
            g.SetClip(capsule);

            // Boş tank arka planı
            using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                new Point(x, y),
                new Point(x, y + th),
                Color.FromArgb(232, 232, 232),
                Color.FromArgb(205, 205, 205)))
            {
                g.FillPath(bgBrush, capsule);
            }

            // ── 3. Yakıt dolumu
            if (fill > 0f)
            {
                int fillH = (int)(th * fill);
                int fillY = y + th - fillH;
                Rectangle fuelRect = new Rectangle(x, fillY, tw, fillH);

                Region fuelRegion = new Region(capsule);
                fuelRegion.Intersect(fuelRect);
                g.SetClip(fuelRegion, CombineMode.Replace);

                using (LinearGradientBrush fuelBrush = new LinearGradientBrush(
                    new Point(x, fillY),
                    new Point(x, y + th),
                    fLight,
                    fDark))
                {
                    g.FillRectangle(fuelBrush, fuelRect);
                }

                // Yakıt yüzey çizgisi
                if (fillY > y + 4)
                {
                    g.SetClip(capsule);
                    using (Pen surfPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1.5f))
                    {
                        g.DrawLine(surfPen, x + capW, fillY, x + tw - capW, fillY);
                    }
                }

                fuelRegion.Dispose();
            }

            // ── 4. Su katmanı
            if (_waterPercent > 0)
            {
                g.SetClip(capsule);
                int waterPx = (int)(th * _waterPercent / 100.0);
                if (waterPx < 3) waterPx = 3;

                Rectangle waterRect = new Rectangle(x, y + th - waterPx, tw, waterPx);

                using (LinearGradientBrush wb = new LinearGradientBrush(
                    new Point(x, y + th - waterPx),
                    new Point(x, y + th),
                    Color.FromArgb(160, LightenColor(ColWater, 30)),
                    Color.FromArgb(220, DarkenColor(ColWater, 20))))
                {
                    g.FillRectangle(wb, waterRect);
                }

                // Su yüzeyi noktası
                if (_showWaterDot)
                {
                    g.ResetClip();
                    int wDotX = x + tw / 2;
                    int wDotY = y + th - waterPx;
                    if (wDotY < y + 6) wDotY = y + 6;
                    if (wDotY > y + th - 6) wDotY = y + th - 6;

                    using (SolidBrush wDotFill = new SolidBrush(Color.FromArgb(220, DarkenColor(ColWater, 40))))
                    using (Pen wDotBorder = new Pen(Color.FromArgb(255, DarkenColor(ColWater, 70)), 1f))
                    {
                        g.FillEllipse(wDotFill, wDotX - 4, wDotY - 4, 8, 8);
                        g.DrawEllipse(wDotBorder, wDotX - 4, wDotY - 4, 8, 8);
                    }

                    g.SetClip(capsule);
                }
            }

            // ── 5. Üst parlaklık yansıması
            g.SetClip(capsule);
            using (LinearGradientBrush glareBrush = new LinearGradientBrush(
                new PointF(x, y),
                new PointF(x, y + th * 0.38f),
                Color.FromArgb(85, 255, 255, 255),
                Color.Transparent))
            {
                g.FillRectangle(glareBrush, x + capW, y + 2, tw - capW * 2, (int)(th * 0.36f));
            }

            g.ResetClip();
            capsule.Dispose();

            // ── 6. Dış çerçeve
            GraphicsPath outline = BuildCapsulePath(x, y, tw, th, capW);
            using (Pen outlinePen = new Pen(Color.FromArgb(175, 175, 175), 1.8f))
            {
                g.DrawPath(outlinePen, outline);
            }
            outline.Dispose();

            // ── 7. Eliptik uç kapaklar
            DrawEndCap(g, x, y, capW, th, true);
            DrawEndCap(g, x + tw - capW * 2, y, capW, th, false);

            // ── 8. Orta referans çizgisi
            if (_showReferenceLine)
            {
                int midX = x + tw / 2;
                using (Pen midPen = new Pen(Color.FromArgb(65, 0, 0, 0), 1f))
                {
                    midPen.DashStyle = DashStyle.Dash;
                    g.DrawLine(midPen, midX, y + 6, midX, y + th - 6);
                }
            }

            // ── 9. Seviye noktası
            if (_showLevelDot)
            {
                int midX = x + tw / 2;
                int dotY = y + th - (int)(th * fill);
                if (dotY < y + 8) dotY = y + 8;
                if (dotY > y + th - 8) dotY = y + th - 8;

                using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(170, 50, 50, 50)))
                {
                    g.FillEllipse(dotBrush, midX - 4, dotY - 4, 8, 8);
                }
            }

            // ── 10. Yüzde etiketi
            if (_showPercentLabel)
            {
                float fontSize = th * 0.14f;
                if (fontSize < 9f) fontSize = 9f;
                if (fontSize > 18f) fontSize = 18f;

                using (Font pFont = new Font("Segoe UI", fontSize, FontStyle.Bold))
                {
                    string pStr = string.Format("{0:F0}%", _fillPercent);
                    SizeF pSize = g.MeasureString(pStr, pFont);

                    float tx = x + tw * 0.35f - pSize.Width / 2f;
                    float ty = y + (th - pSize.Height) / 2f;

                    // Arka plan yuvarlak dikdörtgen
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
                    {
                        RectangleF bgRect = new RectangleF(tx - 4, ty - 1, pSize.Width + 8, pSize.Height + 2);
                        GraphicsPath bgPath = new GraphicsPath();
                        int r = 4;
                        bgPath.AddArc(bgRect.X, bgRect.Y, r * 2, r * 2, 180, 90);
                        bgPath.AddArc(bgRect.Right - r * 2, bgRect.Y, r * 2, r * 2, 270, 90);
                        bgPath.AddArc(bgRect.Right - r * 2, bgRect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                        bgPath.AddArc(bgRect.X, bgRect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                        bgPath.CloseFigure();
                        g.FillPath(bgBrush, bgPath);
                        bgPath.Dispose();
                    }

                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(50, 50, 50)))
                    {
                        g.DrawString(pStr, pFont, textBrush, tx, ty);
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  YARDIMCI ÇİZİM METODLARI
        // ═══════════════════════════════════════════════════════════

        private void DrawEndCap(Graphics g, int x, int y, int capW, int th, bool isLeft)
        {
            Rectangle capRect = new Rectangle(x, y, capW * 2, th);
            Region region = new Region(capRect);
            g.SetClip(region, CombineMode.Replace);

            using (LinearGradientBrush depthBrush = new LinearGradientBrush(
                new Point(x, y),
                new Point(x + capW * 2, y),
                isLeft ? Color.FromArgb(55, 0, 0, 0) : Color.Transparent,
                isLeft ? Color.Transparent : Color.FromArgb(55, 0, 0, 0)))
            {
                g.FillEllipse(depthBrush, capRect);
            }

            g.ResetClip();
            region.Dispose();
        }

        private GraphicsPath BuildCapsulePath(int x, int y, int tw, int th, int capW)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(x, y, capW * 2, th, 90, 180);
            path.AddArc(x + tw - capW * 2, y, capW * 2, th, 270, 180);
            path.CloseFigure();
            return path;
        }

        private void DrawDropShadow(Graphics g, int x, int y, int tw, int th, int capW)
        {
            for (int i = 4; i >= 1; i--)
            {
                GraphicsPath sp = BuildCapsulePath(x + i, y + i, tw, th, capW);
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(14 * i, 0, 0, 0)))
                {
                    g.FillPath(sb, sp);
                }
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
