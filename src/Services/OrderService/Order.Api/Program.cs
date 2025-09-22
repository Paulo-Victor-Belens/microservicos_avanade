using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdGen;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Api.Consumers;
using Order.Api.Data;
using Order.Api.Repositories;
using Order.Api.Services;
using FluentValidation.AspNetCore;
using FluentValidation;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Prometheus;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Iniciando o Order API...");

var builder = WebApplication.CreateBuilder(args);
var workerId = 0;
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.WebHost.UseUrls("http://+:8080");

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341"));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:8000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
        });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = "http://identity-api:8080";
    options.Audience = "SeuEcommerce.Api";
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters.ValidateAudience = true;
    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            logger.LogError(context.Exception, "Falha na autenticação JWT.");
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<JwtBearerEvents>>();

            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) 
                        ?? context.Principal?.FindFirst("sub")?.Value;

            var safeClaims = context.Principal?.Claims
                .Where(c => c.Type == "role" || c.Type == ClaimTypes.Name)
                .ToDictionary(c => c.Type, c => c.Value);

            logger.LogInformation(
                "Token JWT validado com sucesso. UserId: {UserId}, Claims: {@Claims}",
                userId,
                safeClaims);

            await Task.CompletedTask;
        }
    };
});


builder.Services.AddSingleton(new IdGenerator(workerId));
builder.Services.AddHealthChecks();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseMySql(connectionString,  new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        })
    .UseSnakeCaseNamingConvention()
);

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StockUpdatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseCors(MyAllowSpecificOrigins);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Aplicando migrations do banco de dados de Pedidos...");

        var context = services.GetRequiredService<OrderDbContext>();

        await context.Database.MigrateAsync();

        logger.LogInformation("Migrations aplicadas com sucesso.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Um erro ocorreu durante a migração do banco de dados de Pedidos.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.MapMetrics();

app.Run();