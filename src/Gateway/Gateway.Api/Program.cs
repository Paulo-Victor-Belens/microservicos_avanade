using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text.Json;
using System.Security.Claims;
using Prometheus;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Iniciando a Identity API...");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(policyName: "fixed", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });
});

var seqUrl = builder.Configuration["SEQ_URL"] 
    ?? throw new InvalidOperationException("URL do Seq não encontrada (SEQ_URL).");

Log.Information("Essa é a URL do Seq: {SeqUrl}", seqUrl);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .WriteTo.Console()
    .WriteTo.Seq(seqUrl));

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://identity-api:8080";
        options.Audience = "SeuEcommerce.Api";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                logger.LogError(context.Exception, "Gateway: Falha na autenticação JWT.");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                var subjectId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                logger.LogInformation("Gateway: Token JWT validado com sucesso para o usuário {UserId}", subjectId);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Commerce API Gateway", Version = "v1" });
});

builder.WebHost.UseUrls("http://*:8080");

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseRateLimiter();

try
{
    var prometheusTargetsPath = "/app/prometheus_targets/targets.json";
    var servicesToMonitor = new[]
    {
        new { targets = new[] { "gateway-api:8080" }, labels = new { job = "gateway-api" } },
        new { targets = new[] { "stock-api:8080" }, labels = new { job = "stock-api" } },
        new { targets = new[] { "order-api:8080" }, labels = new { job = "order-api" } },
        new { targets = new[] { "identity-api:8080" }, labels = new { job = "identity-api" } }
    };
    var jsonContent = JsonSerializer.Serialize(servicesToMonitor);

    var directoryPath = Path.GetDirectoryName(prometheusTargetsPath);
    if (directoryPath != null)
    {
        Directory.CreateDirectory(directoryPath);
    }

    File.WriteAllText(prometheusTargetsPath, jsonContent);
    Console.WriteLine("Arquivo de alvos do Prometheus gerado com sucesso em: " + prometheusTargetsPath);
}
catch (Exception ex)
{
    Console.WriteLine("ERRO ao gerar arquivo de alvos do Prometheus: " + ex.Message);
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("http://localhost:8080/swagger/v1/swagger.json", "Stock API v1");
    c.SwaggerEndpoint("http://localhost:8082/swagger/v1/swagger.json", "Order API v1");
    c.SwaggerEndpoint("http://localhost:8084/swagger/v1/swagger.json", "Identity API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

app.UseHttpMetrics();
app.UseAuthentication();

app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();
});

await app.UseOcelot();

app.Run();