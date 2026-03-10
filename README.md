# 🛢️ CylindricalTankVisual

<p align="center">
  <img src="https://img.shields.io/badge/.NET%20Framework-4.0-blue?style=flat-square&logo=dotnet"/>
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet"/>
  <img src="https://img.shields.io/badge/WinForms-supported-green?style=flat-square"/>
  <img src="https://img.shields.io/badge/WPF-supported-green?style=flat-square"/>
  <img src="https://img.shields.io/badge/License-MIT-yellow?style=flat-square"/>
</p>

<p align="center">
  <b>Responsive, 3D-rendered cylindrical tank UI component for WinForms and WPF.</b><br/>
  Fuel level, water layer, gradient shading, XAML binding — all in pure C#, zero image assets.
</p>

---

## 📋 İçindekiler / Table of Contents

- [Özellikler / Features](#-özellikler--features)
- [Kurulum / Installation](#-kurulum--installation)
- [Hızlı Başlangıç / Quick Start](#-hızlı-başlangıç--quick-start)
- [Özelleştirme / Customization](#-özelleştirme--customization)
- [Mimari / Architecture](#-mimari--architecture)
- [Sürüm Notları / Version Notes](#-sürüm-notları--version-notes)
- [Katkıda Bulunma / Contributing](#-katkıda-bulunma--contributing)
- [Lisans / License](#-lisans--license)

---

## ✨ Özellikler / Features

| Özellik | Açıklama |
|---|---|
| 🎨 **3B Görünüm** | Gölge, gradient, parlaklık yansıması ile gerçekçi silindir |
| ⛽ **Yakıt Katmanı** | Özelleştirilebilir renk ve doluluk yüzdesi |
| 💧 **Su Katmanı** | Tabandaki su birikintisi ayrı renk ve yüzde ile gösterilir |
| 📐 **Responsive** | Kontrol boyutu ne olursa olsun tank tüm alanı doldurur |
| 🔗 **XAML Binding** | WPF `DependencyProperty` desteği — animasyon ve binding hazır |
| 🖱️ **Sürükle-Bırak** | Visual Studio Toolbox'tan direkt kullanım |
| 🚫 **Bağımsız** | WPF versiyonu sıfır üçüncü taraf bağımlılığı |

---

## 📦 Kurulum / Installation

### Yöntem 1 — Kaynak Kodu (Önerilen)

Projenize uygun dosyayı kopyalayın:

```
📁 src/
 ├── CylindricalTankVisual.cs              ← WinForms / .NET Framework 4.0
 ├── CylindricalTankVisual_WPF_Net40.cs   ← WPF    / .NET Framework 4.0
 └── CylindricalTankVisual_WPF_Net8.cs    ← WPF    / .NET 8
```

### Yöntem 2 — Manuel Referans

1. İlgili `.cs` dosyasını projenize ekleyin
2. Namespace'i kendi projenizle eşleştirin (`DXApplication1` → kendi namespace'iniz)
3. Derleyin — hazır!

> **WinForms notu:** `CylindricalTankVisual.Designer.cs` dosyasının da projede bulunması gerekir.

---

## 🚀 Hızlı Başlangıç / Quick Start

### WinForms

```csharp
var tank = new CylindricalTankVisual();
tank.Dock        = DockStyle.Fill;
tank.FillPercent = 75.0;
tank.FuelColor   = Color.FromArgb(66, 133, 244);
tank.WaterPercent = 8.0;

this.Controls.Add(tank);
```

### WPF — XAML

```xml
<Window xmlns:tank="clr-namespace:DXApplication1">

    <tank:CylindricalTankVisual
        Width="320"
        Height="130"
        FillPercent="75"
        FuelColor="#FF4285F4"
        WaterPercent="8"
        ShowPercentLabel="True"
        ShowReferenceLine="True"/>

</Window>
```

### WPF — Binding Örneği / Binding Example

```xml
<tank:CylindricalTankVisual
    FillPercent="{Binding TankLevel}"
    WaterPercent="{Binding WaterLevel}"
    FuelColor="{Binding TankColor}"/>
```

---

## ⚙️ Özelleştirme / Customization

| Property | Tür / Type | Varsayılan / Default | Açıklama |
|---|---|---|---|
| `FillPercent` | `double` | `60.0` | Yakıt doluluk yüzdesi (0–100) |
| `FuelColor` | `Color` | `#4285F4` | Yakıt rengi |
| `WaterPercent` | `double` | `0.0` | Su katmanı yüzdesi (0–100) |
| `ShowPercentLabel` | `bool` | `true` | Yüzde etiketini göster |
| `ShowLevelDot` | `bool` | `true` | Yakıt seviye noktasını göster |
| `ShowWaterDot` | `bool` | `true` | Su yüzeyi noktasını göster |
| `ShowReferenceLine` | `bool` | `true` | Orta referans çizgisini göster |
| `TankPadding` / `Padding` | `double` / `int` | `6` | Çevre boşluğu (piksel) |

---

## 🏗️ Mimari / Architecture

Bileşen, **painter's algorithm** ile 10 katmanlı bir çizim mantığı kullanır:

```
1. Drop shadow          — Yumuşak gölge efekti
2. Tank arka planı      — Gradient gri zemin
3. Yakıt dolumu         — Renkli gradient, kapsüle kırpılı
4. Su katmanı           — Mavi şerit, tabanda
5. Parlaklık yansıması  — Üstte beyaz highlight
6. Dış çerçeve          — İnce gri outline
7. Uç kapak gölgeleri   — Sol/sağ eliptik 3B derinlik
8. Referans çizgisi     — Kesik orta dikey çizgi
9. Seviye noktaları     — Yakıt + su yüzey işaretçileri
10. Yüzde etiketi       — Yuvarlak köşeli arka planlı metin
```

### Platform Farkları / Platform Differences

| | WinForms | WPF |
|---|---|---|
| **Base class** | `XtraUserControl` | `FrameworkElement` |
| **Render hook** | `OnPaint` | `OnRender` |
| **Property sistemi** | CLR property + `Invalidate()` | `DependencyProperty` |
| **Çizim API** | `System.Drawing` (GDI+) | `System.Windows.Media` |
| **Clip** | `Graphics.SetClip()` | `DrawingContext.PushClip()` |
| **Animasyon** | Manuel timer gerekir | XAML Storyboard ile doğrudan |

---

## 📝 Sürüm Notları / Version Notes

### .NET 8 — Kritik Fark

`FormattedText` constructor'ı `.NET 8`'de `pixelsPerDip` parametresi **zorunludur**:

```csharp
// .NET Framework 4.0
new FormattedText(text, culture, direction, typeface, size, brush);

// .NET 8 — pixelsPerDip ZORUNLU
double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
new FormattedText(text, culture, direction, typeface, size, brush, dpi);
```

---

## 🤝 Katkıda Bulunma / Contributing

Pull request ve issue'lar memnuniyetle karşılanır.

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/yeni-ozellik`)
3. Değişikliklerinizi commit edin (`git commit -m 'feat: yeni özellik eklendi'`)
4. Branch'inizi push edin (`git push origin feature/yeni-ozellik`)
5. Pull Request açın

---

## 🏢 Hakkında / About

**PETRODATA BİLİŞİM DANIŞMANLIK EĞİTİM ELEKTRONİK OTOMASYON SAN. VE TİC. LTD. ŞTİ.**

Petrol, gaz ve endüstriyel otomasyon sektörlerine yönelik yazılım çözümleri geliştirmekteyiz.

---

## 📄 Lisans / License

Bu proje MIT lisansı ile lisanslanmıştır. Ayrıntılar için [LICENSE](LICENSE) dosyasına bakınız.

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

<p align="center">
  Made with ❤️ by <b>PETRODATA</b> · Mustafa Çağrı Altındal
</p>