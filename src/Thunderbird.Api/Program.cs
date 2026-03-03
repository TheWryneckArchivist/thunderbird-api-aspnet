using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Thunderbird.Api.Infrastructure.Messaging;
using Thunderbird.Api.Infrastructure.Persistence;
using Thunderbird.Api.Infrastructure.Security;
using Thunderbird.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddHealthChecks();

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    var signingKey = builder.Configuration["Auth:SigningKey"] ?? "dev-signing-key-change-me";
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateIssuerSigningKey = true,
      ValidateLifetime = true,
      ValidIssuer = builder.Configuration["Auth:Issuer"] ?? "thunderbird.local",
      ValidAudience = builder.Configuration["Auth:Audience"] ?? "thunderbird-clients",
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
    };
  });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<PlatformDbContext>(options =>
{
  var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Port=5432;Database=thunderbird;Username=thunderbird;Password=thunderbird";
  options.UseNpgsql(connectionString);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
  var redis = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
  return ConnectionMultiplexer.Connect(redis);
});

builder.Services.AddSingleton<RabbitChannelFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapGrpcReflectionService();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapGrpcService<AuthGrpcService>();
app.MapGrpcService<ProfileGrpcService>();
app.MapGrpcService<CatalogGrpcService>();
app.MapGrpcService<SessionGrpcService>();
app.MapGrpcService<OfflineSyncGrpcService>();
app.MapGrpcService<ModerationGrpcService>();

app.MapGet("/", () => "Thunderbird gRPC API. Use a gRPC or gRPC-Web client.");

app.Run();
