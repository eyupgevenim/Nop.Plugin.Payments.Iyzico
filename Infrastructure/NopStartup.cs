namespace Nop.Plugin.Payments.Iyzico.Infrastructure
{
    using FluentMigrator.Runner;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Nop.Core.Infrastructure;
    using Nop.Data;

    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            var dataSettings = DataSettingsManager.LoadSettings();
            if (!dataSettings?.IsValid ?? true)
                return;

            //application.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("Content-Security-Policy", $"frame-src * data:;");
            //    await next();
            //});

            ApplyMigration(application);
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 101;

        /// <summary>
        /// Apply Migrations
        /// </summary>
        /// <param name="application">IApplicationBuilder</param>
        private void ApplyMigration(IApplicationBuilder application)
        {
            try
            {
                using (var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                    try
                    {
                        runner.MigrateUp();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}