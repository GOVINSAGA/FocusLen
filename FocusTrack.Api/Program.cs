using System.Text;
using FocusTrack.Api.Data;
using FocusTrack.Api.Middleware;
using FocusTrack.Api.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ─── Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Database (SQLite for local dev) ────────────────────────────────────────
builder.Services.AddDbContext<FocusDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Data Source=focustrack.db"));

// ─── Application Services ────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ActivityIngestionService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<LimitsService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<WeeklyReportJob>();

// ─── Hangfire (in-memory for local dev) ─────────────────────────────────────
// NOTE: For production, replace MemoryStorage with Hangfire.SqlServer or similar.
builder.Services.AddHangfire(config =>
    config.UseMemoryStorage());
builder.Services.AddHangfireServer();

// ─── JWT Bearer Authentication ───────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = "ExternalCookie";
    })
    .AddCookie("ExternalCookie")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "dummy";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "dummy";
        options.SaveTokens = true;
    });

builder.Services.AddAuthorization();

// ─── CORS ────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FocusTrackCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ─── OpenAPI ─────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ─── Build ───────────────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Auto-migrate DB on startup ──────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FocusDbContext>();
    db.Database.Migrate(); // Applies all pending migrations (replaces EnsureCreated)
}

// ─── Middleware Pipeline ─────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // ⚠️ Hangfire dashboard is open (no auth) in dev only.
    // TODO: Add authorization filter before any production deployment.
    app.UseHangfireDashboard("/hangfire");
}

app.UseHttpsRedirection();
app.UseCors("FocusTrackCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ─── Schedule Recurring Jobs ─────────────────────────────────────────────────
// Runs every Monday at 08:00 UTC. Trigger manually at /hangfire > Recurring Jobs.
RecurringJob.AddOrUpdate<WeeklyReportJob>(
    recurringJobId: "weekly-focus-report",
    methodCall: job => job.ExecuteAsync(),
    cronExpression: Cron.Weekly(DayOfWeek.Monday, 8));

app.Run();
