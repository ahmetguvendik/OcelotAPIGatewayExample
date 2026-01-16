# Ocelot API Gateway Örneği

Bu proje, Ocelot API Gateway kullanarak mikroservis mimarisini gösteren bir örnek uygulamadır. Proje, bir API Gateway ve üç downstream servisten (Service A, Service B, Service C) oluşmaktadır.

## 📋 İçindekiler

- [Proje Yapısı](#proje-yapısı)
- [Teknolojiler](#teknolojiler)
- [Kurulum](#kurulum)
- [Servisler](#servisler)
- [JWT Authentication](#jwt-authentication)
- [API Gateway Yapılandırması](#api-gateway-yapılandırması)
- [Load Balancing](#load-balancing)
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
- **Portlar**: 
  - Instance 1: `7060` (HTTPS), `5274` (HTTP)
  - Instance 2: `7061` (HTTPS), `5275` (HTTP)
  - Instance 3: `7062` (HTTPS), `5276` (HTTP)
- **Durum**: JWT Authentication ile korumalı
- **Load Balancing**: RoundRobin (3 instance arasında istekler dağıtılır)
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

#### Service A Route'ları (JWT Gerekli - Load Balanced)

Service A için 3 instance yapılandırılmıştır ve RoundRobin load balancing kullanılmaktadır:

```json
{
  "UpstreamPathTemplate": "/servicea",
  "DownstreamPathTemplate": "/",
  "DownstreamHostAndPorts": [
    {"Host": "localhost", "Port": 7060},
    {"Host": "localhost", "Port": 7061},
    {"Host": "localhost", "Port": 7062}
  ],
  "UpstreamHttpMethod": ["GET"],
  "LoadBalancerOptions": {
    "Type": "RoundRobin"
  },
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
- **DownstreamHostAndPorts**: Downstream servisin host ve port bilgisi (birden fazla instance için array)
- **LoadBalancerOptions**: Load balancing stratejisi (RoundRobin, LeastConnection, CookieBasedStickySessions)
- **AuthenticationOptions**: JWT authentication gerektiren route'lar için yapılandırma

## ⚖️ Load Balancing

Bu projede Service A için **RoundRobin Load Balancing** yapılandırılmıştır. API Gateway, gelen istekleri 3 Service A instance'ı arasında sırayla dağıtır.

### Yapılandırma

Service A için 3 instance tanımlanmıştır:
- **Instance 1**: Port 7060
- **Instance 2**: Port 7061
- **Instance 3**: Port 7062

### Load Balancing Stratejisi

- **RoundRobin**: İstekler instance'lar arasında sırayla dağıtılır (1→2→3→1→2→3...)
- Her istek bir sonraki instance'a yönlendirilir
- Yük eşit şekilde dağıtılır

### Test Etme

Load balancing'in çalıştığını test etmek için:

```bash
# JWT token oluşturun (GenerateJWT aracı ile)
cd GenerateJWT
dotnet run

# Token'ı kullanarak birden fazla istek gönderin
TOKEN="YOUR_JWT_TOKEN"
for i in {1..9}; do
  echo "Test $i:"
  curl -k -H "Authorization: Bearer $TOKEN" https://localhost:7056/servicea
  echo ""
done
```

Her istek farklı bir instance'dan yanıt alacaktır (timestamp'ler farklı olacaktır).

## 📝 Kullanım

### Servisleri Çalıştırma

1. **Service A'nın 3 instance'ını başlatın (farklı terminal pencerelerinde):**
   ```bash
   # Terminal 1 - Instance 1
   cd Resources/ServiceA
   dotnet run --launch-profile instance1
   
   # Terminal 2 - Instance 2
   cd Resources/ServiceA
   dotnet run --launch-profile instance2
   
   # Terminal 3 - Instance 3
   cd Resources/ServiceA
   dotnet run --launch-profile instance3
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
   dotnet run --launch-profile https
   ```

**Not:** Tüm servislerin aynı anda çalışıyor olması gerekir. Service A için 3 instance'ın da çalışması load balancing için gereklidir.

### İstek Örnekleri

#### Public Endpoint'ler (JWT Gerektirmez)

```bash
# Service B
curl -X GET https://localhost:7056/serviceb

# Service C
curl -X GET https://localhost:7056/servicec
```

#### Protected Endpoint'ler (JWT Gerekli)

**JWT Token Oluşturma:**

Önce JWT token oluşturmanız gerekir:

```bash
cd GenerateJWT
dotnet run
```

Bu komut size bir JWT token verecektir. Bu token'ı aşağıdaki isteklerde kullanın.

**İstek Örnekleri:**

```bash
# Service A - Ana endpoint (Load balanced - 3 instance arasında dağıtılır)
curl -k -X GET https://localhost:7056/servicea \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Service A - Test endpoint (Load balanced)
curl -k -X GET https://localhost:7056/servicea/test \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Not:** `-k` parametresi self-signed SSL sertifikaları için gerekir.

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

### JWT Token Oluşturma Aracı

Projede test için JWT token oluşturan bir araç bulunmaktadır:

```bash
cd GenerateJWT
dotnet run
```

Bu araç, `appsettings.json` dosyasındaki JWT ayarlarını kullanarak geçerli bir token oluşturur.

### Yeni Endpoint Ekleme

1. İlgili servise endpoint ekleyin (örn: `ServiceA/Program.cs`)
2. `ocelot.json` dosyasına yeni route ekleyin
3. JWT gerekiyorsa `AuthenticationOptions` ekleyin
4. Load balancing gerekiyorsa `LoadBalancerOptions` ekleyin

### Yeni Servis Ekleme

1. `Resources/` klasörü altına yeni servis ekleyin
2. `ocelot.json` dosyasına yeni route ekleyin
3. Gerekirse JWT authentication yapılandırması ekleyin
4. Gerekirse load balancing yapılandırması ekleyin

### Load Balancing Yapılandırması

Birden fazla instance için load balancing eklemek için:

1. `ocelot.json` dosyasında `DownstreamHostAndPorts` array'ine tüm instance'ları ekleyin
2. `LoadBalancerOptions` ekleyin:
   ```json
   "LoadBalancerOptions": {
     "Type": "RoundRobin"
   }
   ```
3. Her instance için `launchSettings.json` dosyasında farklı port profilleri oluşturun

## 📚 Kaynaklar

- [Ocelot Documentation](https://ocelot.readthedocs.io/)
- [Ocelot Load Balancing](https://ocelot.readthedocs.io/en/latest/features/loadbalancer.html)
- [ASP.NET Core JWT Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [.NET 9.0 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)

## 📄 Lisans

Bu proje örnek amaçlı hazırlanmıştır.

## 👤 Yazar

Proje, Ocelot API Gateway ve mikroservis mimarisi öğrenmek için hazırlanmıştır.

---

**Not:** Production ortamında mutlaka güvenlik ayarlarını gözden geçirin ve güçlü secret key'ler kullanın.
