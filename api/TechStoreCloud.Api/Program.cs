using Microsoft.EntityFrameworkCore;
using Serilog;
using TechStoreCloud.Api.Data;
using TechStoreCloud.Api.Middleware;
using TechStoreCloud.Api.Repositories;
using TechStoreCloud.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TechStoreCloud.Api")
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? "Host=localhost;Port=5432;Database=techstore;Username=techstore;Password=techstore123";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// DI
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();

// CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:8080", "http://localhost:3000", "http://127.0.0.1:8080" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "TechStore Cloud API",
        Version = "v1",
        Description = "API REST para cadastro de produtos - TechStore Cloud"
    });
});

var app = builder.Build();

// Middleware de exceções
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger (todos os ambientes para fins acadêmicos)
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechStore Cloud API v1"));

app.UseCors("Frontend");
app.MapControllers();

// Auto-migrate em desenvolvimento
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        Log.Information("Migrations aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Não foi possível aplicar migrations automaticamente. Verifique a conexão com o banco.");
    }
}

Log.Information("TechStore Cloud API iniciada em {Environment}", app.Environment.EnvironmentName);
app.Run();
