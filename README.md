# Ocelot API Gateway Örneği

Bu proje, Ocelot API Gateway kullanarak mikroservis mimarisini gösteren bir örnek uygulamadır. Proje, bir API Gateway ve üç downstream servisten (Service A, Service B, Service C) oluşmaktadır.

## 📋 İçindekiler

- [Proje Yapısı](#proje-yapısı)
- [Teknolojiler](#teknolojiler)
- [Kurulum](#kurulum)
- [Servisler](#servisler)
- [JWT Authentication](#jwt-authentication)
- [API Gateway Yapılandırması](#api-gateway-yapılandırması)
- [Kullanım](#kullanım)
- [Endpoint'ler](#endpointler)

## 🏗️ Proje Yapısı

```
OcelotAPIGatewayExample/
├── API_Gateway/          # Ocelot API Gateway
├── Resources/
│   ├── ServiceA/         # JWT Authentication ile korumalı servis
│   ├── ServiceB/         # Public servis
│   └── ServiceC/         # Public servis
└── OcelotAPIGatewayExample.sln
```

## 🛠️ Teknolojiler

- **.NET 9.0**
- **Ocelot** (24.1.0) - API Gateway
- **JWT Bearer Authentication** - Token tabanlı kimlik doğrulama
- **ASP.NET Core Minimal APIs**

## 📦 Kurulum

### Gereksinimler

- .NET 9.0 SDK
- Visual Studio 2022 veya JetBrains Rider (veya herhangi bir .NET IDE)

### Adımlar

1. Projeyi klonlayın veya indirin
2. Solution dosyasını açın:
   ```bash
   dotnet restore
   ```
3. Tüm projeleri build edin:
   ```bash
   dotnet build
   ```

## 🚀 Servisler

### API Gateway
- **Port**: `7056` (HTTPS), `5160` (HTTP)
- **Base URL**: `https://localhost:7056`
- **Görev**: Tüm istekleri downstream servislere yönlendirir ve JWT token doğrulaması yapar

### Service A
- **Port**: `7060` (HTTPS), `5274` (HTTP)
- **Durum**: JWT Authentication ile korumalı
- **Endpoint'ler**:
  - `GET /` - Ana endpoint
  - `GET /test` - Test endpoint'i

### Service B
- **Port**: `7203` (HTTPS), `5064` (HTTP)
- **Durum**: Public (Authentication gerektirmez)
- **Endpoint'ler**:
  - `GET /` - Ana endpoint

### Service C
- **Port**: `7236` (HTTPS), `5076` (HTTP)
- **Durum**: Public (Authentication gerektirmez)
- **Endpoint'ler**:
  - `GET /` - Ana endpoint

## 🔐 JWT Authentication

### Yapılandırma

JWT ayarları `appsettings.json` dosyalarında yapılandırılmıştır:

```json
{
  "JwtSettings": {
    "SecretKey": "BuCokGizliBirAnahtarOlmalidirVeEnAz32KarakterUzunlugundaOlmalidir",
    "Issuer": "ServiceA",
    "Audience": "ServiceA",
    "ExpirationInMinutes": 60
  }
}
```

### Önemli Notlar

⚠️ **Production Ortamı İçin:**
- `SecretKey`'i environment variable veya Azure Key Vault gibi güvenli bir yerden alın
- En az 32 karakter uzunluğunda güçlü bir secret key kullanın
- `Issuer` ve `Audience` değerlerini projenize göre özelleştirin

## 🌐 API Gateway Yapılandırması

API Gateway yapılandırması `API_Gateway/ocelot.json` dosyasında tanımlanmıştır.

### Route Yapılandırması

#### Service A Route'ları (JWT Gerekli)

```json
{
  "UpstreamPathTemplate": "/servicea",
  "DownstreamPathTemplate": "/",
  "DownstreamHostAndPorts": [{"Host": "localhost", "Port": 7060}],
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer"
  }
}
```

#### Service B ve Service C Route'ları (Public)

```json
{
  "UpstreamPathTemplate": "/serviceb",
  "DownstreamPathTemplate": "/",
  "DownstreamHostAndPorts": [{"Host": "localhost", "Port": 7203}]
}
```

### Route Açıklamaları

- **UpstreamPathTemplate**: API Gateway'e gelen isteğin path'i
- **DownstreamPathTemplate**: Downstream servise gönderilecek path
- **DownstreamHostAndPorts**: Downstream servisin host ve port bilgisi
- **AuthenticationOptions**: JWT authentication gerektiren route'lar için yapılandırma

## 📝 Kullanım

### Servisleri Çalıştırma

1. **Service A'yı başlatın:**
   ```bash
   cd Resources/ServiceA
   dotnet run
   ```

2. **Service B'yi başlatın:**
   ```bash
   cd Resources/ServiceB
   dotnet run
   ```

3. **Service C'yi başlatın:**
   ```bash
   cd Resources/ServiceC
   dotnet run
   ```

4. **API Gateway'i başlatın:**
   ```bash
   cd API_Gateway
   dotnet run
   ```

**Not:** Tüm servislerin aynı anda çalışıyor olması gerekir.

### İstek Örnekleri

#### Public Endpoint'ler (JWT Gerektirmez)

```bash
# Service B
curl -X GET https://localhost:7056/serviceb

# Service C
curl -X GET https://localhost:7056/servicec
```

#### Protected Endpoint'ler (JWT Gerekli)

```bash
# Service A - Ana endpoint
curl -X GET https://localhost:7056/servicea \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Service A - Test endpoint
curl -X GET https://localhost:7056/servicea/test \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 🔗 Endpoint'ler

### API Gateway Endpoint'leri

| Method | Endpoint | Authentication | Açıklama |
|--------|----------|----------------|----------|
| GET | `/servicea` | ✅ JWT Gerekli | Service A ana endpoint'ine yönlendirir |
| GET | `/servicea/test` | ✅ JWT Gerekli | Service A test endpoint'ine yönlendirir |
| GET | `/serviceb` | ❌ Public | Service B endpoint'ine yönlendirir |
| GET | `/servicec` | ❌ Public | Service C endpoint'ine yönlendirir |

### Service A Endpoint'leri

| Method | Endpoint | Response |
|--------|----------|----------|
| GET | `/` | Service A bilgileri ve timestamp |
| GET | `/test` | "Bu bir Service A testtir" |

### Service B Endpoint'leri

| Method | Endpoint | Response |
|--------|----------|----------|
| GET | `/` | Service B bilgileri ve timestamp |

### Service C Endpoint'leri

| Method | Endpoint | Response |
|--------|----------|----------|
| GET | `/` | Service C bilgileri ve timestamp |

## 🔧 Geliştirme

### Yeni Endpoint Ekleme

1. İlgili servise endpoint ekleyin (örn: `ServiceA/Program.cs`)
2. `ocelot.json` dosyasına yeni route ekleyin
3. JWT gerekiyorsa `AuthenticationOptions` ekleyin

### Yeni Servis Ekleme

1. `Resources/` klasörü altına yeni servis ekleyin
2. `ocelot.json` dosyasına yeni route ekleyin
3. Gerekirse JWT authentication yapılandırması ekleyin

## 📚 Kaynaklar

- [Ocelot Documentation](https://ocelot.readthedocs.io/)
- [ASP.NET Core JWT Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [.NET 9.0 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)

## 📄 Lisans

Bu proje örnek amaçlı hazırlanmıştır.

## 👤 Yazar

Proje, Ocelot API Gateway ve mikroservis mimarisi öğrenmek için hazırlanmıştır.

---

**Not:** Production ortamında mutlaka güvenlik ayarlarını gözden geçirin ve güçlü secret key'ler kullanın.
