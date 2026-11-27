<p align="left">
  <a href="https://iotsharp.net/">
    <img src="docs/static/img/logo_white.svg" width="360px" alt="IoTSharp logo" />
  </a>
</p>

# IoTSharp

[English](README.md) | [Chinese](README.zh.md) | [Türkçe](README.tr.md)

[![Lisans](https://img.shields.io/github/license/IoTSharp/IoTSharp.svg)](LICENSE)
[![.NET build](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-build.yml)
[![Docs deploy](https://github.com/IoTSharp/IoTSharp/actions/workflows/docs-deploy.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/docs-deploy.yml)
[![Release binaries](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-publish.yml)
[![NuGet packages](https://github.com/IoTSharp/IoTSharp/actions/workflows/pack-nupkg.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/pack-nupkg.yml)
[![Docker images](https://github.com/IoTSharp/IoTSharp/actions/workflows/docker-release.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/docker-release.yml)
![Docker Pulls](https://img.shields.io/docker/pulls/maikebing/iotsharp)
![GitHub all releases](https://img.shields.io/github/downloads/IoTSharp/IoTSharp/total)

IoTSharp; cihaz erişimi, telemetri toplama, kural zinciri işleme, görselleştirme, çok kiracılı (multi-tenant) operasyonlar ve ürün dağıtımı için açık kaynaklı bir endüstriyel IoT platformudur.

## Çevrimiçi demo ve test hesapları

- Demo: <https://iotsharp.online/#/login>
- Kullanıcı adı: `demo@iotsharp.online`
- Parola: `P@ssw0rd`

Yalnızca paylaşımlı demo hesabının kullanıcı adı ve parolası her 30 dakikada bir sıfırlanır. **Normal kayıtlı hesaplar hizmeti test etmek için kullanılabilir. Bu hesapların kullanıcı bilgileri, parolaları ve iş verileri sıfırlanmaz ve uzun süreli saklanır.** [Test hesabı oluşturun](https://iotsharp.online/#/signup).

## 🚀 Genel Bakış

IoTSharp, üretim ortamında bir IoT platformu çalıştırmak için gereken temel yapı taşlarını bir araya getirir:

- HTTP, MQTT, CoAP ve genişletilebilir protokol entegrasyonu ile cihaz ve ağ geçidi bağlantısı.
- Telemetri, öznitelikler, alarmlar, ürünler, varlıklar ve kiracı farkındalığına sahip yönetim modelleri.
- Dönüştürme, bildirim, otomasyon ve iş aksiyonları için kural zinciri (rule-chain) tabanlı işleme.
- Farklı dağıtım ve ölçekleme ihtiyaçları için ilişkisel ve zaman serisi depolama seçenekleri.
- Docker, Windows servisi, Linux servisi, kurulum akışları ve sürüm paketleri dahil çoklu dağıtım kipleri.
- Yol haritası artık yapay zekâ çalışma tezgâhını, MCP araçlarını ve ajan destekli operasyonları toplama, kurallar ve sürüm iş akışları için yatay bir yetenek olarak ele almaktadır.
- Planlanan: İsteğe bağlı Sezika destekli aday kararlarıyla ekipman, alarm ve bakım kayıtları için [Ontoloji Mühendisliği uygulama izi](ROADMAP.md#ontology-engineering). Uygulama ve alan doğrulaması devam etmektedir.

Mevcut ana uygulama `.NET 10` sürümünü hedefler ve web konsolu IoTSharp markalı bir Vue 3 uygulaması olarak sürdürülür.

## 🧱 Ürün Matrisi (Bulut-Uç-Cihaz)

IoTSharp tek bir platform uygulaması değil; üç katmanı ve bir yapay zekâ temelini kapsayan açık kaynaklı bir ürün matrisidir:

| Katman | Proje | Açıklama |
| --- | --- | --- |
| Platform | [IoTSharp](https://github.com/IoTSharp/IoTSharp) (bu depo) | Cihaz erişimi, telemetri, kural zincirleri, çoklu kiracılık, EdgeNode yönetimi ve sürüm operasyonları için kontrol düzlemi |
| Uç (Edge) | [IoTEdge](https://github.com/IoTSharp/IoTEdge) | Uç ağ geçidi çalışma zamanı: yerel yönetim arayüzüne sahip tek sunuculuk çalıştırılabilir dosya, yerleşik Modbus, OPC UA ve yaygın PLC toplama sürücüleri artı betik tabanlı dönüştürme; kayıt, kalp atışı (heartbeat), yetenek bildirimi, toplama yapılandırması dağıtımı ve görev alındıları için platformla entegre olur |
| Cihaz (Device) | [IoTEmbedded](https://github.com/IoTSharp/IoTEmbedded) | MCU/RTOS hedefleri için gömülü cihaz çalışma zamanı: yerleşik BASIC betik motoru, Modbus RTU ve MQTT erişimine sahip bellenim düzeyinde çalışma zamanı; hata durumunda geri alma özellikli çift yuvalı betik depolamayı destekler |
| Yapay zekâ temeli | [Tomur](https://github.com/IoTSharp/Tomur) | Yerel model çalışma zamanı: platformun yapay zekâ yeteneklerini çevrimdışı/intranet ortamlarında barındırır; OpenAI uyumlu API'ler arkasında GGUF LLM, konuşma, görüntü ve OCR çok kipli çıkarım sunar, tek dosya olarak dağıtılabilir |

Tüm katmanlar açık kaynaklıdır ve bağımsız olarak kullanılabilir; birlikte uçtan uca bir endüstriyel veri toplama ve operasyon çözümü oluştururlar. Çevrimdışı veya intranet senaryolarında SonnetDB ile Tomur'u birleştirmek, sıfır harici bağımlılıkla tam işlevli bir dağıtım sağlar.

## 🧭 Hızlı Başlangıç

### 🖥️ Beş dakikalık SonnetDB profili

İlk yerel çalıştırma için varsayılan yol SonnetDB profilidir. İlişkisel veriler, telemetri, önbellek, nesne depolama ve platform olay kuyruğu için tek veri bağımlılığı olarak SonnetDB ile IoTSharp'ı başlatır:

```bash
docker compose -f docker-compose.sonnetdb.yml up -d
```

Ardından `http://localhost:2927` adresini açın ve Kurulum (Installer) başlatmasını tamamlayın.

Bu profil `ASPNETCORE_ENVIRONMENT=SonnetDB` kullanır ve `appsettings.SonnetDB.json` dosyasını bağlar. Geleneksel PostgreSQL / TimescaleDB Docker Compose uyumluluk ve üretim topolojisi doğrulaması için kullanılabilir olmaya devam eder, ancak önerilen hızlı başlangıç girişi SonnetDB'dir.

IoTSharp, SonnetDB'yi yayımlanmış `5.0.0` NuGet paketleri ve `iotsharp/sonnetdb:5.0.0` kapsayıcı imajı üzerinden kullanır. Paket sürümleri `Directory.Packages.props` içinde yönetilir; `SONNETDB_IMAGE` dağıtım imajını geçersiz kılabilir. SonnetDB kaynak kodu kontrolü gerekmez. Kalan `IoTSharp.Data.JsonDB` alt modülü `git submodule update --init IoTSharp.Data.JsonDB` ile başlatılır.

### 🖥️ IoTSharp'ı yerel olarak çalıştırma

Önerilen belge giriş noktaları:

- Hızlı başlangıç: <https://iotsharp.net/guide/getting-started/>
- Dağıtım kılavuzu: <https://iotsharp.net/guide/deployment/>
- Ürün belgeleri: <https://iotsharp.net/docs/intro>
- Kurulum seçenekleri: <https://iotsharp.net/docs/getting-started/installation-options>

Ön yüz geliştirmesi için mevcut yerel geliştirme sunucusu varsayılanı:

- Ön yüz: `http://localhost:27915`

## 📚 Belgeler

- İngilizce README: [README.md](README.md)
- Çince README: [README.zh.md](README.zh.md)
- Yol haritası: [ROADMAP.md](ROADMAP.md)
- Değişiklik günlüğü: [CHANGELOG.md](CHANGELOG.md)

## 🤝 Katkıda Bulunma

Issue ve pull request'ler aracılığıyla katkılarınızı bekliyoruz:

- Pull request'ler: <https://github.com/IoTSharp/IoTSharp/pulls>
- Issue'lar: <https://github.com/IoTSharp/IoTSharp/issues>

Katkıda bulunmadan önce lütfen kod tabanı yapısını, ilgili belgeleri ve belge sitesindeki güncel sürüm/dağıtım yönünü inceleyin.

## 💬 Topluluk Desteği

IoTSharp'ı kullanma veya dağıtma konusunda yardıma ihtiyacınız olursa belge ve topluluk materyallerinde topluluk kanalları mevcuttur:

- GitHub: <https://github.com/IoTSharp/IoTSharp>
- Gitee: <https://gitee.com/IoTSharp/IoTSharp>
- Resmî site: <https://iotsharp.net>

![WeChat grubu](docs/static/img/qyqun.jpg)

## ❤️ Bağış

IoTSharp, Apache 2.0 lisansı altında yayımlanmaktadır. Projeyi desteklemek isterseniz şu kanallardan katkıda bulunabilirsiniz:

- OpenCollective: <https://opencollective.com/IoTSharp>
- Afdian: <https://afdian.net/a/maikebing>
- Destekçiler listesi: [BACKERS.md](BACKERS.md)

## ✨ Dilek

- İyilik yapasın, kötülükten uzak durasın.
- Kendin için bağışlanma bulasın ve başkalarını bağışlayasın.
- Aldığından fazlasını vererek özgürce paylaşasın.
