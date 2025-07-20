using Microsoft.AspNetCore.Http.Features;
using SubtitleTranslator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISubtitleService, SubtitleService>();
builder.Services.AddScoped<ITranslationService, GoogleTranslationService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Configure Kestrel for large files (up to 50MB to be safe)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
    serverOptions.Limits.MaxRequestHeadersTotalSize = 65536; // 64KB
    serverOptions.Limits.MaxRequestHeaderCount = 200;
    serverOptions.Limits.MaxRequestBufferSize = 2 * 1024 * 1024; // 2MB
    serverOptions.Limits.MaxRequestLineSize = 16384; // 16KB
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

// Configure file upload for large files
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
    options.MultipartHeadersCountLimit = 32;
    options.MultipartHeadersLengthLimit = 32768;
    options.ValueCountLimit = 10000;
    options.ValueLengthLimit = 1024 * 1024; // 1MB per value
    options.MemoryBufferThreshold = 1024 * 1024; // 1MB buffer
});

// Configure IIS for large files
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
    options.AllowSynchronousIO = true;
});

// Add session with minimal storage (only for tracking keys)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Add distributed memory cache for session storage
builder.Services.AddDistributedMemoryCache();

// Add background service for file cleanup
builder.Services.AddHostedService<CleanupService>();

var app = builder.Build();

// Create required directories
var webRoot = app.Environment.WebRootPath;
var uploadsDir = Path.Combine(webRoot, "uploads");
var tempDir = Path.Combine(webRoot, "temp");

if (!Directory.Exists(uploadsDir))
    Directory.CreateDirectory(uploadsDir);
if (!Directory.Exists(tempDir))
    Directory.CreateDirectory(tempDir);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();