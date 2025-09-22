using FluentValidation;
using FluentValidation.AspNetCore;
using Identity.Api.Data;
using Identity.Api.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Api.Services;
using IdGen;
using Prometheus;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Iniciando a Identity API...");

var builder = WebApplication.CreateBuilder(args);
var workerId = 2;
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.WebHost.UseUrls("http://+:8080");

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .WriteTo.Console());

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

builder.Services.AddSingleton(new IdGenerator(workerId));
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        })
    .UseSnakeCaseNamingConvention()
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole<long>>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityServer(options =>
    {

        options.IssuerUri = builder.Configuration["Jwt:Issuer"];
    })
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddAspNetIdentity<ApplicationUser>()
    .AddProfileService<Duende.IdentityServer.AspNetIdentity.ProfileService<ApplicationUser>>();


builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<IdentityDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<long>>>();
        var idGenerator = services.GetRequiredService<IdGenerator>();

        logger.LogInformation("Aplicando migrations do banco de dados de Identidade...");

        await context.Database.MigrateAsync();
        logger.LogInformation("Migrations aplicadas com sucesso.");

        logger.LogInformation("Iniciando o seeding do banco de dados de Identidade...");

        await IdentityDbSeeder.Seed(userManager, roleManager, idGenerator, logger);
        logger.LogInformation("Seeding do banco de dados de Identidade concluído.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Um erro ocorreu durante a migração ou seeding do banco de dados de Identidade.");
    }
}

app.UseCors(MyAllowSpecificOrigins);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapMetrics();

app.Run();
