# thunderbird-api-aspnet

Authoritative gRPC backend for Thunderbird.

## Services

- AuthService
- ProfileService
- CatalogService
- SessionService
- OfflineSyncService
- ModerationService

## Local Development

```bash
dotnet tool restore
dotnet restore
cd src/Thunderbird.Api
dotnet run
```

## Contract Source

This repo compiles protobuf definitions from `../thunderbird-contracts/proto` during development bootstrap.
In separated GitHub repos, consume the published `Thunderbird.Contracts` NuGet package.

## Local Infra Stack

```bash
docker compose up --build
```

- API gRPC endpoint: `localhost:5001`
- Envoy gRPC-Web endpoint: `localhost:8080`
- RabbitMQ management: `http://localhost:15672`

## Database Migrations

Generated migrations live in `src/Thunderbird.Api/Infrastructure/Persistence/Migrations`.

```bash
dotnet dotnet-ef database update \
  --project src/Thunderbird.Api/Thunderbird.Api.csproj \
  --startup-project src/Thunderbird.Api/Thunderbird.Api.csproj
```

Idempotent SQL scripts are emitted under `infra/sql/`.

## Private NuGet Feed

`nuget.config` expects:

- `NUGET_FEED_URL`
- `NUGET_FEED_USERNAME`
- `NUGET_FEED_TOKEN`
