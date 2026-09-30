using WEBAPP_FitMatch;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Vercel container runtime routes traffic to $PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (port != null) builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllersWithViews();

//  2) Add neon db
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddScoped<NotificationService>();
// Keys that encrypt the session cookie live in the DB, so every container instance can read it
builder.Services.AddDataProtection()
    .SetApplicationName("FitMatch")
    .PersistKeysToDbContext<AppDbContext>();
//  3) Add Session
builder.Services.AddHttpContextAccessor();
// Sessions live in Postgres, not memory: Vercel scales containers to zero and runs several at once
builder.Services.AddDistributedPostgresCache(options =>
{
    options.ConnectionString = connectionString;
    options.SchemaName = "public";
    options.TableName = "Session";
    options.CreateIfNotExists = true;
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}")
    .WithStaticAssets();

app.Run();
