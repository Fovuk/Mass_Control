# MASS CONTROL

Unity 2D platform / puzzle oyunu. Karakterin kütlesini ve boyutunu değiştirerek engelleri aş, kutuları it, yıldızları topla ve portala ulaş.

![Bölüm seçimi](Docs/screenshots/level-select.jpeg)

## Oynanış

- **Küçük form:** Hızlı hareket, çift zıplama, dar geçitlere girme.
- **Büyük form:** Daha yavaş ama ağır; itilebilir nesneleri (boulder) hareket ettirme.
- Her bölümde **3 yıldız** topla, **bitiş portalına** ulaş.
- Tuzaklara (spike vb.) değersen bölüm yeniden başlar.

![Oynanış — Bölüm 1](Docs/screenshots/gameplay.jpeg)

## Kontroller

| Kontroller | Aksiyon |
|------------|---------|
| Hareket (joystick / WASD / oklar) | Yatay hareket |
| **Zıplama** | Zıpla (küçükken çift zıplama) |
| **Boyut Değiştir** | Küçük ↔ büyük form |

## Özellikler

- Bölüm seçim ekranı (Bölüm 1–6) ve yıldız ilerlemesi
- Morph (boyut / kütle) mekaniği
- İtilebilir nesneler
- Tilemap tabanlı seviyeler
- Hareketli platform desteği
- Loop’lu menü / oyun içi müzik ve SFX
- Mobil uyumlu dokunmatik UI (joystick + butonlar)

## Gereksinimler

- [Unity](https://unity.com/) **6000.x** (URP 2D)
- Bu repo’yu açıp `Assets/Scenes/MainMenu.unity` ile başlat

## Projeyi açma

1. Unity Hub → **Add** → bu klasörü seç
2. Projeyi aç
3. `Assets/Scenes/MainMenu.unity` sahnesini yükle
4. Play

## Sahne yapısı

| Sahne | Açıklama |
|-------|----------|
| `MainMenu` | Ana menü / bölüm seçimi |
| `Level_01` … `Level_06` | Oynanabilir bölümler |

## Teknik notlar

- Render: **Universal Render Pipeline (2D)**
- Input: **Unity Input System**
- Kamera: **Cinemachine**
- Harita: **2D Tilemap**
- Kayıt: yerel progress (`SaveManager`)

## Lisans / assetler

Oyun kodu bu repo’da. Kullanılan görseller üçüncü parti paketlerden gelebilir (ör. Free Platform Game Assets); ticari kullanımda paket lisanslarını kontrol et.
