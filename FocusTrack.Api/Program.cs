using System.Text;
using FocusTrack.Api.Data;
using FocusTrack.Api.Middleware;
using FocusTrack.Api.Services;
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

// ─── Auth Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();

// ─── JWT Bearer Authentication ───────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

// ─── OpenAPI (built-in .NET 10) ───────────────────────────────────────────────
builder.Services.AddOpenApi();

// ─── Build ───────────────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Auto-create DB on startup ───────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FocusDbContext>();
    db.Database.EnsureCreated();
}

// ─── Middleware Pipeline ─────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Available at /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseCors("FocusTrackCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
