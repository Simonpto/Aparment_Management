using Apartment_Manager.Web.Components;
using Apartment_Manager.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001";

// One shared cookie container so the admin auth cookie is visible to all API clients
var cookieContainer = new System.Net.CookieContainer();
HttpClientHandler SharedHandler() => new() { UseCookies = true, CookieContainer = cookieContainer };

builder.Services.AddHttpClient<IApartmentApiService, ApartmentApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .ConfigurePrimaryHttpMessageHandler(SharedHandler);
builder.Services.AddHttpClient<IBlogApiService, BlogApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .ConfigurePrimaryHttpMessageHandler(SharedHandler);
builder.Services.AddHttpClient<IReviewApiService, ReviewApiService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .ConfigurePrimaryHttpMessageHandler(SharedHandler);
builder.Services.AddHttpClient<IAdminAuthService, AdminAuthService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .ConfigurePrimaryHttpMessageHandler(SharedHandler);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
