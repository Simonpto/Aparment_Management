using Apartment_Manager.Api.Infrastructure;
using Apartment_Manager.Api.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;

Env.Load();
DapperConfig.RegisterTypeHandlers();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database
builder.Services.AddSingleton<DatabaseConnectionFactory>();

// Repositories
builder.Services.AddScoped<ApartmentRepository>();
builder.Services.AddScoped<BlogRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewTokenRepository, ReviewTokenRepository>();

// Services (only where real logic exists beyond data access)
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReviewTokenService, ReviewTokenService>();

// Cookie auth for admin
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "LimnosaAdmin";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
    });

builder.Services.AddAuthorization();

// CORS for Blazor frontend
var blazorOrigin = builder.Configuration["BlazorOrigin"] ?? "https://localhost:7002";
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(blazorOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
