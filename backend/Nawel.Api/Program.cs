using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using System.Text;
using AspNetCoreRateLimit;
using Nawel.Api.Data;
using Nawel.Api.Services.Auth;
using Nawel.Api.Services.Email;
using Nawel.Api.Services.ProductInfo;
using Nawel.Api.Authorization;
using Nawel.Api.Configuration;
using Nawel.Api.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Include XML comments
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure form options for file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = ApplicationConstants.FileUpload.MaxFileSizeBytes;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configure Settings (with environment variable overrides)
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
jwtSettings.Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? jwtSettings.Secret;
jwtSettings.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwtSettings.Issuer;
jwtSettings.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwtSettings.Audience;
jwtSettings.Validate();
builder.Services.AddSingleton(jwtSettings);

var emailSettings = new EmailSettings();
builder.Configuration.GetSection(EmailSettings.SectionName).Bind(emailSettings);
emailSettings.SmtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? emailSettings.SmtpHost;
emailSettings.SmtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : emailSettings.SmtpPort;
emailSettings.SmtpUser = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? emailSettings.SmtpUsername;
emailSettings.SmtpPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? emailSettings.SmtpPassword;
emailSettings.FromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? emailSettings.FromEmail;
emailSettings.FromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? emailSettings.FromName;
emailSettings.UseSsl = bool.TryParse(Environment.GetEnvironmentVariable("SMTP_USE_SSL"), out var useSsl) ? useSsl : emailSettings.UseSsl;
emailSettings.Validate();
builder.Services.AddSingleton(emailSettings);

var fileStorageSettings = new FileStorageSettings();
builder.Configuration.GetSection(FileStorageSettings.SectionName).Bind(fileStorageSettings);
fileStorageSettings.Validate();
builder.Services.AddSingleton(fileStorageSettings);

// Register Exception Handler
builder.Services.AddExceptionHandler<Nawel.Api.Middleware.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<INotificationDebouncer, NotificationDebouncer>();
builder.Services.AddSingleton<IReservationNotificationDebouncer, ReservationNotificationDebouncer>();
builder.Services.AddScoped<IProductInfoExtractor, ProductInfoExtractor>();

// Register HttpClient for ProductInfoExtractor
builder.Services.AddHttpClient();

// Configure Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new AdminRequirement()));
});

// Register Authorization Handler
builder.Services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();
builder.Services.AddHttpContextAccessor();

// Configure Database (SQLite for dev, MySQL for prod)
var useSqlite = builder.Configuration.GetValue<bool>("UseSqlite");

if (useSqlite)
{
    builder.Services.AddDbContext<NawelDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

    Console.WriteLine("Using SQLite database for development");
}
else
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
    builder.Services.AddDbContext<NawelDbContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            serverVersion));

    Console.WriteLine("Using MySQL database");
}

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5174", "http://localhost:5175")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Run migrations automatically on startup (for all environments)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NawelDbContext>();
    try
    {
        app.Logger.LogInformation("Running database migrations...");
        db.Database.Migrate();
        app.Logger.LogInformation("Database migrations completed successfully.");

        // Seed test data in development mode
        if (useSqlite && app.Environment.IsDevelopment())
        {
            DbSeeder.SeedTestData(db);
            Console.WriteLine("SQLite database initialized with test data");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// Configure the HTTP request pipeline.

// Exception handler must be first
app.UseExceptionHandler();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nawel API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Nawel API Documentation";
});

// Rate limiting must be before authentication/authorization
app.UseIpRateLimiting();

app.UseCors("AllowFrontend");

// Serve static files from wwwroot (default)
app.UseStaticFiles();

// Serve uploads directory
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
