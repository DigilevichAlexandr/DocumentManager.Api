using DocumentManager.Ui.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Базовый адрес API (Kestrel из backend-проекта)
var apiBaseAddress = new Uri("https://localhost:7153/");

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = apiBaseAddress;
});

// Добавляем базовый URL API в конфигурацию для использования в Razor Pages
builder.Configuration["ApiBaseUrl"] = apiBaseAddress.ToString().TrimEnd('/');

// Сессии для хранения токена авторизации
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Игнорируем запросы от DevTools к Blazor Server
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/_framework"))
    {
        context.Response.StatusCode = 404;
        return;
    }
    await next();
});

app.UseSession();

app.MapRazorPages();

app.Run();
