// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Conglomo">
// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Web.Server
{
    using GoToBible.Model;
    using GoToBible.Providers;
    using GoToBible.Web.Server.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.SqlServer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Pomelo.Extensions.Caching.MySql;

    /// <summary>
    /// Start up configuration.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940.
        /// </remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            // Setup Razor and Web API
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Add supported translation providers
            services.AddSingleton<IProvider, BibleApi>();
            services.AddSingleton<IProvider, BibliaApi>();
            services.AddSingleton<IProvider, CoverdalePsalter>();
            services.AddSingleton<IProvider, DigitalBiblePlatformApi>();
            services.AddSingleton<IProvider, EsvBible>();
            services.AddSingleton<IProvider, Laodiceans>();
            services.AddSingleton<IProvider, NetBible>();
            services.AddSingleton<IProvider, NltBible>();
            services.AddSingleton<IProvider, SblGnt>();

            // Add options for providers that require them
            services.Configure<BibleApiOptions>(this.Configuration.GetSection("Providers:BibleApi"));
            services.Configure<BibliaApiOptions>(this.Configuration.GetSection("Providers:BibliaApi"));
            services.Configure<DigitalBiblePlatformApiOptions>(this.Configuration.GetSection("Providers:DigitalBiblePlatformApi"));
            services.Configure<EsvBibleOptions>(this.Configuration.GetSection("Providers:EsvBible"));
            services.Configure<NltBibleOptions>(this.Configuration.GetSection("Providers:NltBible"));

            // Load the caching provider
            CacheSettings? cacheConfig = this.Configuration.GetSection("Providers:Cache").Get<CacheSettings>();
            switch (cacheConfig?.DatabaseProvider.ToUpperInvariant())
            {
                case "MSSQL":
                    services.Configure<SqlServerCacheOptions>(this.Configuration.GetSection("Providers:Cache"));
                    services.AddSingleton<IDistributedCache, SqlServerCache>();
                    break;
                case "MARIADB":
                case "MYSQL":
                    services.Configure<MySqlCacheOptions>(this.Configuration.GetSection("Providers:Cache"));
                    services.AddSingleton<IDistributedCache, MySqlCache>();
                    break;
                default:
                    services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
                    break;
            }

            // Load the statistics context
            StatisticsSettings? statisticsConfig = this.Configuration.GetSection("Providers:Statistics").Get<StatisticsSettings>();
            ServerVersion serverVersion;
            switch (statisticsConfig?.DatabaseProvider?.ToUpperInvariant())
            {
                case "MARIADB":
                    if (string.IsNullOrWhiteSpace(statisticsConfig.DatabaseVersion))
                    {
                        serverVersion = MariaDbServerVersion.LatestSupportedServerVersion;
                    }
                    else
                    {
                        serverVersion = new MariaDbServerVersion(statisticsConfig.DatabaseVersion);
                    }

                    if (!string.IsNullOrWhiteSpace(statisticsConfig.ConnectionString))
                    {
                        services.AddDbContext<StatisticsContext>(options =>
                            options.UseMySql(statisticsConfig.ConnectionString, serverVersion));
                    }

                    break;
                case "MSSQL":
                    if (!string.IsNullOrWhiteSpace(statisticsConfig.ConnectionString))
                    {
                        services.AddDbContext<StatisticsContext>(options =>
                            options.UseSqlServer(statisticsConfig.ConnectionString));
                    }

                    break;
                case "MYSQL":
                    if (string.IsNullOrWhiteSpace(statisticsConfig.DatabaseVersion))
                    {
                        serverVersion = MySqlServerVersion.LatestSupportedServerVersion;
                    }
                    else
                    {
                        serverVersion = new MySqlServerVersion(statisticsConfig.DatabaseVersion);
                    }

                    if (!string.IsNullOrWhiteSpace(statisticsConfig.ConnectionString))
                    {
                        services.AddDbContext<StatisticsContext>(options =>
                            options.UseMySql(statisticsConfig.ConnectionString, serverVersion));
                    }

                    break;
            }
        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </remarks>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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
            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
                endpoints.MapFallbackToFile("/{param?}", "index.html"); // Allow urls like https://localhost/John.1
            });
        }
    }
}
