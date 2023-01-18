// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Conglomo">
// Copyright 2020-2023 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

using System.IO;
using GoToBible.Model;
using GoToBible.Providers;
using GoToBible.Web.Server;
using GoToBible.Web.Server.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Pomelo.Extensions.Caching.MySql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Setup Razor and Web API
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new StringConverter()));
builder.Services.AddRazorPages();

// Add supported translation providers
builder.Services.AddSingleton<IProvider, BibleApi>();
builder.Services.AddSingleton<IProvider, BibliaApi>();
builder.Services.AddSingleton<IProvider, BundledTranslations>();
builder.Services.AddSingleton<IProvider, DigitalBiblePlatformApi>();
builder.Services.AddSingleton<IProvider, EsvBible>();
builder.Services.AddSingleton<IProvider, NetBible>();
builder.Services.AddSingleton<IProvider, NltBible>();

// Add options for providers that require them
builder.Services.Configure<BibleApiOptions>(builder.Configuration.GetSection("Providers:BibleApi"));
builder.Services.Configure<BibliaApiOptions>(builder.Configuration.GetSection("Providers:BibliaApi"));
builder.Services.Configure<DigitalBiblePlatformApiOptions>(builder.Configuration.GetSection("Providers:DigitalBiblePlatformApi"));
builder.Services.Configure<EsvBibleOptions>(builder.Configuration.GetSection("Providers:EsvBible"));
builder.Services.Configure<NltBibleOptions>(builder.Configuration.GetSection("Providers:NltBible"));

// Load the caching provider
CacheSettings? cacheConfig = builder.Configuration.GetSection("Providers:Cache").Get<CacheSettings>();
switch (cacheConfig?.DatabaseProvider.ToUpperInvariant())
{
    case "MSSQL":
        builder.Services.Configure<SqlServerCacheOptions>(builder.Configuration.GetSection("Providers:Cache"));
        builder.Services.AddSingleton<IDistributedCache, SqlServerCache>();
        break;
    case "MARIADB":
    case "MYSQL":
        builder.Services.Configure<MySqlCacheOptions>(builder.Configuration.GetSection("Providers:Cache"));
        builder.Services.AddSingleton<IDistributedCache, MySqlCache>();
        break;
    default:
        builder.Services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
        break;
}

// Load the statistics context
StatisticsSettings? statisticsConfig = builder.Configuration.GetSection("Providers:Statistics").Get<StatisticsSettings>();
ServerVersion serverVersion;
switch (statisticsConfig?.DatabaseProvider?.ToUpperInvariant())
{
    case "MARIADB":
        serverVersion = string.IsNullOrWhiteSpace(statisticsConfig.DatabaseVersion)
            ? MariaDbServerVersion.LatestSupportedServerVersion
            : new MariaDbServerVersion(statisticsConfig.DatabaseVersion);

        if (!string.IsNullOrWhiteSpace(statisticsConfig.ConnectionString))
        {
            builder.Services.AddDbContext<StatisticsContext>(options =>
            options.UseMySql(statisticsConfig.ConnectionString, serverVersion));
        }

        break;
    case "MSSQL":
        if (!string.IsNullOrWhiteSpace(statisticsConfig.ConnectionString))
        {
            builder.Services.AddDbContext<StatisticsContext>(options =>
            options.UseSqlServer(statisticsConfig.ConnectionString));
        }

        break;
    case "MYSQL":
        serverVersion = string.IsNullOrWhiteSpace(statisticsConfig.DatabaseVersion)
            ? MySqlServerVersion.LatestSupportedServerVersion
            : new MySqlServerVersion(statisticsConfig.DatabaseVersion);

        if (!string.IsNullOrWhiteSpace(statisticsConfig.ConnectionString))
        {
            builder.Services.AddDbContext<StatisticsContext>(options =>
            options.UseMySql(statisticsConfig.ConnectionString, serverVersion));
        }

        break;
}

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = { "index.html" },
    RequestPath = new PathString("/about"),
});
app.UseStaticFiles();

// Allow static files within the about directory to allow for automatic SSL renewal
app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true, // this was needed as IIS would not serve extensionless URLs from the directory without it
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "about")),
    RequestPath = new PathString("/about"),
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
});

app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapFallbackToFile("/{param?}", "index.html"); // Allow urls like https://localhost/John.1

app.Run();
