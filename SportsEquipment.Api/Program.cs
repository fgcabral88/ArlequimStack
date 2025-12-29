using Serilog;
using System.Text;
using OpenTelemetry;
using Serilog.Events;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using SportsEquipment.Infrastructure.Data;
using SportsEquipment.Api.Presentation.IoC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SportsEquipment.Api.Presentation.Configuration.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Configuração especial para Docker
if (builder.Environment.IsEnvironment("Docker") || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    builder.Configuration.AddJsonFile("appsettings.Docker.json", optional: true);

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// JWT config - COM VALIDAÇÃO
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

// VALIDAÇÃO: Verifica se as configurações JWT foram carregadas corretamente
if (jwtSettings == null)
    throw new InvalidOperationException("Configurações JWT não encontradas no arquivo de configuração.");

if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
    throw new InvalidOperationException("Chave secreta JWT não configurada.");

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
    throw new InvalidOperationException("Emissor JWT não configurado.");

if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
    throw new InvalidOperationException("Audiência JWT não configurada.");

// IoC 
builder.Services.AddApplication(builder.Configuration);

// OpenTelemetry 
var jaegerEnabled = builder.Configuration.GetValue<bool>("Jaeger:Enabled", false);

if (jaegerEnabled)
{
    Sdk.CreateTracerProviderBuilder()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("SportsEquipment.Api"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter(options =>
        {
            options.AgentHost = builder.Configuration.GetValue<string>("Jaeger:Host") ?? "localhost";
            options.AgentPort = builder.Configuration.GetValue<int>("Jaeger:Port", 6831);
        })
        .Build();
}
else
{
    // Apenas console exporter para desenvolvimento
    Sdk.CreateTracerProviderBuilder()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("SportsEquipment.Api"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .Build();
}

// Authentication
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministrator", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("RequireSeller", policy => policy.RequireRole("Seller"));
});

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// SwaggerGen
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SportsEquipment API", Version = "v1", Contact = new() { Name = "Felipe Gabriel Cabral" } });

    // Anotações
    c.EnableAnnotations();

    // Configuração JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Migrations automaticas
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        Log.Information("Aplicando migrations do banco de dados...");
        context.Database.Migrate();
        Log.Information("Migrations aplicadas com sucesso!");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Erro ao aplicar migrations do banco de dados");
        throw;
    }
}

app.UseSerilogRequestLogging();

// Habilitar Swagger mesmo em produção para Docker
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SportsEquipment API v1");
    c.RoutePrefix = "swagger";
});

// Para Docker, não usamos HTTPS Redirection internamente
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();