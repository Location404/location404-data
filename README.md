# 📍 Location404 Data Service

Serviço de dados geográficos e estatísticas para o Location404 - gerenciamento de locations, partidas, estatísticas de jogadores e ranking global.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Database-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Consumer-FF6600?logo=rabbitmq&logoColor=white)](https://www.rabbitmq.com/)
[![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📋 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Arquitetura](#-arquitetura)
- [Tecnologias](#-tecnologias)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação](#-instalação)
- [Configuração](#%EF%B8%8F-configuração)
- [Como Usar](#-como-usar)
- [API Endpoints](#-api-endpoints)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Testes](#-testes)
- [Observabilidade](#-observabilidade)
- [Licença](#-licença)

## 🎯 Sobre o Projeto

O **Location404 Data Service** é o repositório central de dados do Location404. Gerencia locations geográficas, processa eventos de partidas via RabbitMQ e fornece estatísticas e ranking de jogadores.

- **Locations**: Banco de ~100 locations mundiais (Street View)
- **Matches**: Histórico completo de partidas jogadas
- **Player Stats**: ELO-style ranking com pontos, vitórias, derrotas
- **Event-Driven**: Consome eventos do game-engine via RabbitMQ
- **Cache**: Redis para performance (estatísticas e ranking)

### Como Funciona

1. **Game Engine finaliza partida** → Publica evento `match.ended` no RabbitMQ
2. **Data Service consome evento** → MatchConsumerService processa mensagem
3. **Match persistido** → Salva no PostgreSQL com todos os rounds
4. **Stats atualizadas** → Recalcula pontos ELO, win rate, médias
5. **Cache invalidado** → Remove cache do Redis para próxima consulta
6. **API consultada** → Frontend busca stats/ranking atualizadas

## ✨ Funcionalidades

### Locations (Geo Data)
- ✅ Banco de 96+ locations (5 continentes)
- ✅ Seleção aleatória para rounds
- ✅ Metadados (country, region, tags)
- ✅ Parâmetros Street View (heading, pitch)
- ✅ Coordenadas X/Y (Latitude/Longitude)

### Matches & History
- ✅ Processamento de eventos RabbitMQ
- ✅ Histórico completo de partidas
- ✅ Detalhes de todos os rounds
- ✅ Guesses de ambos jogadores
- ✅ Cálculo de distância e pontos

### Player Stats & Ranking
- ✅ Estatísticas individuais (vitórias, derrotas, empates)
- ✅ Sistema de pontos ELO
- ✅ Win rate e médias de pontos
- ✅ Ranking global (top 10-100)
- ✅ Cache Redis (performance)

### Infrastructure
- ✅ RabbitMQ Consumer (background service)
- ✅ Redis cache com invalidação automática
- ✅ EF Core migrations
- ✅ Data seeding (96 locations)

## 🏗️ Arquitetura

O projeto segue **Clean Architecture** com separação clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────────┐
│                      API Layer (REST)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │  Locations   │  │   Matches    │  │    Players      │   │
│  │  Controller  │  │  Controller  │  │   Controller    │   │
│  └──────────────┘  └──────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  ┌──────────────────┐  ┌──────────────────────────────┐    │
│  │    Services      │  │         Interfaces           │    │
│  │ - LocationService│  │  - ILocationService          │    │
│  │ - MatchService   │  │  - IMatchService             │    │
│  │ - PlayerStats    │  │  - IPlayerStatsService       │    │
│  └──────────────────┘  └──────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │  Entities    │  │ Value Objects│  │  Domain Logic   │   │
│  │  - Location  │  │  - Coordinate│  │  - ELO Calc     │   │
│  │  - GameMatch │  │              │  │  - Stats Logic  │   │
│  │  - PlayerStat│  │              │  │                 │   │
│  └──────────────┘  └──────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────────┐ │
│  │PostgreSQL│  │ RabbitMQ │  │  Redis   │  │DataSeeder  │ │
│  │ (EF Core)│  │(Consumer)│  │ (Cache)  │  │(96 Loc's)  │ │
│  └──────────┘  └──────────┘  └──────────┘  └────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Fluxo de Processamento de Match

```
RabbitMQ (game-events exchange)
    │
    ├─ match.ended ──────► MatchConsumerService
    │   (routing key)      │
    │                      ├─ Deserialize GameMatchEndedEventDto
    │                      │
    │                      ├─ MatchService.ProcessMatchEndedEventAsync()
    │                      │   │
    │                      │   ├─ Create GameMatch entity
    │                      │   ├─ Add GameRounds
    │                      │   ├─ Save to PostgreSQL
    │                      │   │
    │                      │   ├─ Update PlayerStats (Player A)
    │                      │   │   ├─ Increment matches/wins/losses
    │                      │   │   ├─ Calculate new ranking points (ELO)
    │                      │   │   ├─ Update averages
    │                      │   │   └─ Save to DB
    │                      │   │
    │                      │   └─ Update PlayerStats (Player B)
    │                      │       └─ (same process)
    │                      │
    │                      └─ Cache.Remove("player:stats:{playerId}")
    │                          Cache.Remove("players:ranking")
    │
    └─ ACK ◄─────────────── Message acknowledged
```

## 🛠️ Tecnologias

### Backend
- **.NET 9.0** - Framework principal
- **ASP.NET Core Web API** - RESTful endpoints
- **Entity Framework Core 9** - ORM

### Database & Cache
- **PostgreSQL 16** - Banco de dados principal
- **Redis** - Cache de estatísticas e ranking
- **Npgsql** - Provider PostgreSQL

### Messaging
- **RabbitMQ** - Event-driven architecture
- **RabbitMQ.Client** - Consumer de eventos

### Observability
- **OpenTelemetry** - Distributed tracing
- **Shared.Observability** - Pacote NuGet customizado
- **Prometheus** - Métricas
- **Grafana Loki** - Logs estruturados

### Testing
- **xUnit** - Framework de testes
- **FluentAssertions** - Assertions expressivas
- **Moq** - Mocking
- **EF Core InMemory** - Testes de repositório

## 📦 Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (porta 5672)

**Opcional:**
- [Redis](https://redis.io/download) ou [Dragonfly](https://www.dragonflydb.io/) (porta 6379)
- [Docker](https://www.docker.com/) - Para rodar dependências

## 🚀 Instalação

### 1. Clone o repositório

```bash
git clone https://github.com/Location404/location404-data.git
cd location404-data
```

### 2. Restaurar dependências

```bash
dotnet restore
```

### 3. Build do projeto

```bash
dotnet build
```

### 4. Aplicar migrations e seed

```bash
cd src/Location404.Data.API
dotnet ef database update --project ../Location404.Data.Infrastructure

# O seed de 96 locations é executado automaticamente na inicialização
```

## ⚙️ Configuração

### appsettings.json

Edite `src/Location404.Data.API/appsettings.json` ou use **variáveis de ambiente**:

```json
{
  "ConnectionStrings": {
    "GeoDataDatabase": "Host=localhost;Port=5432;Database=location404_data;Username=postgres;Password=your_password"
  },

  "RabbitMQ": {
    "Enabled": true,
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "admin",
    "Password": "your_password_here",
    "VirtualHost": "/",
    "ExchangeName": "game-events",
    "MatchEndedQueue": "match-ended",
    "RoundEndedQueue": "round-ended"
  },

  "Cors": {
    "AllowedOrigins": "http://localhost:5173,http://localhost:4200"
  },

  "JwtSettings": {
    "Issuer": "location404",
    "Audience": "location404",
    "SigningKey": "your-super-secret-key-min-32-chars-here",
    "AccessTokenMinutes": 60
  }
}
```

### Variáveis de Ambiente (Docker/Produção)

```bash
# Database
ConnectionStrings__GeoDataDatabase=Host=postgres;Port=5432;Database=location404_data;Username=location404;Password=secure_password

# RabbitMQ
RabbitMQ__Enabled=true
RabbitMQ__HostName=rabbitmq
RabbitMQ__Password=secure_password

# JWT
JwtSettings__SigningKey=your-super-secret-signing-key-here

# CORS
Cors__AllowedOrigins=https://location404.com

# OpenTelemetry
OpenTelemetry__CollectorEndpoint=http://otel-collector:4317
OpenTelemetry__Tracing__SamplingRatio=0.1
```

## 🎮 Como Usar

### Desenvolvimento Local

```bash
# 1. Inicie o PostgreSQL
docker run -d \
  --name postgres-data \
  -e POSTGRES_DB=location404_data \
  -e POSTGRES_USER=location404 \
  -e POSTGRES_PASSWORD=dev_password \
  -p 5432:5432 \
  postgres:16-alpine

# 2. Inicie o RabbitMQ (opcional, se quiser processar eventos)
docker run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=admin \
  -e RABBITMQ_DEFAULT_PASS=admin \
  rabbitmq:3-management

# 3. Aplique as migrations
cd src/Location404.Data.API
dotnet ef database update --project ../Location404.Data.Infrastructure

# 4. Execute o serviço
dotnet run
```

A API estará disponível em:
- **Base URL**: `http://localhost:5000`
- **Swagger/Scalar**: `http://localhost:5000/scalar/v1`
- **Health Check**: `http://localhost:5000/health`
- **Metrics**: `http://localhost:5000/metrics`

### Docker Compose (Recomendado)

```bash
cd location404-utils/deploy/dev
docker-compose up -d location404-data postgres rabbitmq
```

## 📡 API Endpoints

### Locations

#### GET `/api/locations`

Lista todas as locations (ou apenas ativas).

**Query Parameters:**
- `activeOnly` (bool, default: true) - Retornar apenas locations ativas

**Response (200 OK):**
```json
[
  {
    "id": "guid-here",
    "coordinate": { "x": -23.55, "y": -46.63 },
    "name": "São Paulo, Brazil",
    "country": "Brazil",
    "region": "South America",
    "heading": 180,
    "pitch": 5,
    "timesUsed": 42,
    "averagePoints": 3200.5,
    "difficultyRating": 3,
    "tags": ["urban", "metropolitan"],
    "isActive": true
  }
]
```

---

#### GET `/api/locations/{id}`

Busca uma location por ID.

**Response (200 OK):**
```json
{
  "id": "guid-here",
  "coordinate": { "x": -23.55, "y": -46.63 },
  "name": "São Paulo, Brazil",
  "country": "Brazil",
  "region": "South America",
  "heading": 180,
  "pitch": 5,
  "isActive": true
}
```

**Status Codes:**
- `200 OK` - Location encontrada
- `404 Not Found` - Location não existe

---

#### GET `/api/locations/random`

Retorna uma location aleatória (para iniciar rounds).

**Auth:** Não requer autenticação

**Response (200 OK):**
```json
{
  "id": "guid-here",
  "coordinate": { "x": 48.8566, "y": 2.3522 },
  "name": "Paris, France",
  "country": "France",
  "region": "Europe",
  "heading": 90,
  "pitch": 0
}
```

---

#### POST `/api/locations`

Cria uma nova location.

**Request:**
```json
{
  "latitude": 40.7580,
  "longitude": -73.9855,
  "name": "New York, USA",
  "country": "United States",
  "region": "North America",
  "heading": 270,
  "pitch": 10,
  "tags": ["urban", "metropolitan"]
}
```

**Response (201 Created):**
```json
{
  "id": "new-guid-here",
  "coordinate": { "x": 40.7580, "y": -73.9855 },
  "name": "New York, USA",
  "isActive": true
}
```

---

#### DELETE `/api/locations/{id}`

Remove uma location (soft delete).

**Response (204 No Content)**

---

### Matches

#### GET `/api/matches/{id}`

Busca uma match por ID.

**Response (200 OK):**
```json
{
  "id": "match-guid",
  "playerAId": "player-a-guid",
  "playerBId": "player-b-guid",
  "playerATotalPoints": 12500,
  "playerBTotalPoints": 11200,
  "winnerId": "player-a-guid",
  "loserId": "player-b-guid",
  "startedAt": "2025-11-25T12:00:00Z",
  "endedAt": "2025-11-25T12:15:00Z",
  "isCompleted": true,
  "rounds": [
    {
      "roundNumber": 1,
      "locationId": "location-guid",
      "correctAnswer": { "x": -23.55, "y": -46.63 },
      "playerAGuess": { "x": -23.56, "y": -46.64 },
      "playerADistance": 1.2,
      "playerAPoints": 4800,
      "playerBGuess": { "x": -23.54, "y": -46.62 },
      "playerBDistance": 2.1,
      "playerBPoints": 4200
    }
  ]
}
```

---

#### GET `/api/matches/player/{playerId}`

Lista matches de um jogador (paginado).

**Query Parameters:**
- `skip` (int, default: 0) - Pular N matches
- `take` (int, default: 20) - Retornar N matches (max: 100)

**Response (200 OK):**
```json
[
  {
    "id": "match-guid",
    "playerATotalPoints": 12500,
    "playerBTotalPoints": 11200,
    "winnerId": "player-guid",
    "endedAt": "2025-11-25T12:15:00Z"
  }
]
```

---

#### POST `/api/matches/ended`

Processa evento de match finalizado (webhook do RabbitMQ fallback).

**Auth:** Não requer autenticação

**Request:**
```json
{
  "matchId": "guid-here",
  "playerAId": "player-a-guid",
  "playerBId": "player-b-guid",
  "winnerId": "player-a-guid",
  "loserId": "player-b-guid",
  "playerATotalPoints": 12500,
  "playerBTotalPoints": 11200,
  "pointsEarned": 25,
  "pointsLost": 12,
  "startTime": "2025-11-25T12:00:00Z",
  "endTime": "2025-11-25T12:15:00Z",
  "rounds": [...]
}
```

**Response (200 OK):**
```json
{
  "message": "Match processed successfully",
  "matchId": "guid-here"
}
```

---

### Players

#### GET `/api/players/{playerId}/stats`

Retorna estatísticas de um jogador.

**Response (200 OK):**
```json
{
  "playerId": "guid-here",
  "totalMatches": 150,
  "wins": 85,
  "losses": 60,
  "draws": 5,
  "winRate": 56.67,
  "totalRoundsPlayed": 450,
  "totalPoints": 675000,
  "highestScore": 15000,
  "averagePointsPerRound": 1500.0,
  "averageDistanceErrorKm": 120.5,
  "rankingPoints": 1250,
  "lastMatchAt": "2025-11-25T12:15:00Z"
}
```

**Cache:** Resultado é cacheado no Redis por 5 minutos

---

#### GET `/api/players/ranking`

Retorna ranking global de jogadores.

**Query Parameters:**
- `count` (int, default: 10, max: 100) - Quantidade de jogadores

**Response (200 OK):**
```json
[
  {
    "playerId": "top-player-guid",
    "totalMatches": 500,
    "wins": 320,
    "winRate": 64.0,
    "rankingPoints": 1850,
    "averagePointsPerRound": 1800.0
  },
  {
    "playerId": "second-player-guid",
    "rankingPoints": 1720,
    "...": "..."
  }
]
```

**Cache:** Resultado é cacheado no Redis por 10 minutos

**Status Codes:**
- `200 OK` - Ranking retornado
- `400 Bad Request` - Count fora do range (1-100)

---

## 📂 Estrutura do Projeto

```
location404-data/
├── src/
│   ├── Location404.Data.API/                    # API Layer
│   │   ├── Controllers/
│   │   │   ├── LocationsController.cs           # CRUD locations
│   │   │   ├── MatchesController.cs             # Match history
│   │   │   └── PlayersController.cs             # Stats & ranking
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── Location404.Data.Application/            # Application Layer
│   │   ├── Services/
│   │   │   ├── LocationService.cs               # Business logic
│   │   │   ├── MatchService.cs                  # Match processing
│   │   │   └── PlayerStatsService.cs            # Stats calculation
│   │   │
│   │   ├── Common/
│   │   │   └── Interfaces/
│   │   │       ├── ILocationService.cs
│   │   │       ├── IMatchService.cs
│   │   │       └── IPlayerStatsService.cs
│   │   │
│   │   └── DTOs/
│   │       ├── Requests/
│   │       │   └── CreateLocationRequest.cs
│   │       ├── Responses/
│   │       │   ├── LocationResponse.cs
│   │       │   ├── GameMatchResponse.cs
│   │       │   └── PlayerStatsResponse.cs
│   │       └── Events/
│   │           ├── GameMatchEndedEventDto.cs
│   │           └── GameRoundEventDto.cs
│   │
│   ├── Location404.Data.Domain/                 # Domain Layer
│   │   ├── Entities/
│   │   │   ├── Location.cs                      # Aggregate root
│   │   │   ├── GameMatch.cs
│   │   │   ├── GameRound.cs
│   │   │   └── PlayerStats.cs
│   │   │
│   │   └── ValueObjects/
│   │       └── Coordinate.cs                    # X/Y (Lat/Lng)
│   │
│   └── Location404.Data.Infrastructure/         # Infrastructure
│       ├── Persistence/
│       │   ├── GeoDataDbContext.cs              # EF Core DbContext
│       │   ├── Configurations/
│       │   │   ├── LocationConfiguration.cs
│       │   │   ├── GameMatchConfiguration.cs
│       │   │   └── PlayerStatsConfiguration.cs
│       │   └── Repositories/
│       │       ├── LocationRepository.cs
│       │       ├── GameMatchRepository.cs
│       │       └── PlayerStatsRepository.cs
│       │
│       ├── Messaging/
│       │   └── MatchConsumerService.cs          # RabbitMQ consumer
│       │
│       ├── Cache/
│       │   ├── RedisCacheService.cs
│       │   └── NullCacheService.cs              # Null object pattern
│       │
│       ├── Data/
│       │   └── DataSeeder.cs                    # 96 locations seed
│       │
│       ├── Migrations/
│       │   ├── 20250101000000_InitialCreate.cs
│       │   └── GeoDataDbContextModelSnapshot.cs
│       │
│       └── DependencyInjection.cs
│
├── tests/
│   ├── Location404.Data.Domain.UnitTests/
│   │   ├── Entities/
│   │   │   ├── LocationTests.cs
│   │   │   ├── GameMatchTests.cs
│   │   │   └── PlayerStatsTests.cs
│   │   └── ValueObjects/
│   │       └── CoordinateTests.cs
│   │
│   ├── Location404.Data.Application.UnitTests/
│   │   ├── Services/
│   │   │   ├── LocationServiceTests.cs
│   │   │   ├── MatchServiceTests.cs
│   │   │   └── PlayerStatsServiceTests.cs
│   │   └── DTOs/
│   │       └── DtoTests.cs
│   │
│   └── Location404.Data.Infrastructure.UnitTests/
│       ├── Repositories/
│       │   ├── LocationRepositoryTests.cs
│       │   ├── GameMatchRepositoryTests.cs
│       │   └── PlayerStatsRepositoryTests.cs
│       ├── Cache/
│       │   ├── RedisCacheServiceTests.cs
│       │   └── NullCacheServiceTests.cs
│       ├── Data/
│       │   └── DataSeederTests.cs
│       └── Messaging/
│           └── MatchConsumerServiceTests.cs
│
├── Location404.Data.sln
├── README.md
└── .gitignore
```

## 🧪 Testes

### Executar Todos os Testes

```bash
dotnet test
```

### Por Camada

```bash
# Domain
dotnet test tests/Location404.Data.Domain.UnitTests

# Application
dotnet test tests/Location404.Data.Application.UnitTests

# Infrastructure
dotnet test tests/Location404.Data.Infrastructure.UnitTests
```

### Cobertura de Código

```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"TestResults/Report"
```

Abra `TestResults/Report/index.html` no navegador.

**Cobertura Atual:** 85.9%
- Domain: 98%
- Application: 91.5%
- Infrastructure: 77.9%

## 📊 Observabilidade

### Métricas (Prometheus)

Endpoint: `http://localhost:5000/metrics`

**Métricas customizadas:**
- `data_locations_total` - Total de locations cadastradas
- `data_matches_processed_total` - Matches processados via RabbitMQ
- `data_stats_updated_total` - Stats de jogadores atualizadas
- `data_cache_hits_total` - Cache hits no Redis
- `data_cache_misses_total` - Cache misses

### Traces (OpenTelemetry)

Configurado para exportar para coletor OTLP:
- Endpoint: `http://181.215.135.221:4317`
- Sampling: 10% em produção, 100% em desenvolvimento

**Traces automáticos:**
- HTTP requests (API calls)
- Database queries (EF Core)
- RabbitMQ message processing
- Redis cache operations

### Logs (Structured)

Logs estruturados exportados para Grafana Loki:
- Formato: JSON
- Trace correlation: `trace_id`, `span_id`
- Enriched com properties: `match_id`, `player_id`, `location_id`

### Health Checks

```bash
# Health geral
curl http://localhost:5000/health

# Readiness (dependências prontas?)
curl http://localhost:5000/health/ready

# Liveness (processo vivo?)
curl http://localhost:5000/health/live
```

**Dependências verificadas:**
- PostgreSQL (timeout: 5s)
- RabbitMQ (timeout: 5s, se enabled)
- Redis (timeout: 2s, se enabled)

## 📄 Licença

Este projeto está sob a licença **MIT**. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 🔗 Links Relacionados

- [location404-web](https://github.com/Location404/location404-web) - Frontend Vue.js
- [location404-game](https://github.com/Location404/location404-game) - Game engine SignalR
- [location404-auth](https://github.com/Location404/location404-auth) - Serviço de autenticação
- [shared-observability](https://github.com/Location404/shared-observability) - Pacote de observabilidade

## 📞 Suporte

- **Issues**: [GitHub Issues](https://github.com/Location404/location404-data/issues)
- **Discussões**: [GitHub Discussions](https://github.com/Location404/location404-data/discussions)

---

<p align="center">
  Desenvolvido por <a href="https://github.com/ryanbromati">ryanbromati</a>
</p>
