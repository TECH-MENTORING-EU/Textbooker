using static Booker.Services.SendMailSvc;
using System.Configuration;
using Booker.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.RateLimiting;
using Azure.Storage.Blobs;

namespace Booker.Services
{
    public static partial class StartupUtilities
    {
        public static IServiceCollection AddBookerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(x => new BlobServiceClient(configuration["AzureStorage:ConnectionString"]));
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));           
            services.AddTransient<SendMailSvc>();
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddSingleton<IEmailSender, SendMailSvc>();
            
            return services;
        }

        public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;

                options.AddPolicy("IpRateLimit", context => {
                    if (context.Request.Method == HttpMethods.Get ||
                        context.Request.Method == HttpMethods.Head ||
                        context.Request.Method == HttpMethods.Options
                    )
                    {
                        return RateLimitPartition.GetNoLimiter("");
                    }

                    return IpRateLimit(context);

                });

                options.AddPolicy("IpRateLimitAllMethods", context => {
                    return IpRateLimit(context);
                });
            });

            return services;
        }

        private static RateLimitPartition<string> IpRateLimit(HttpContext ctx)
        {
            var ipAddress = ctx.Connection.RemoteIpAddress?.ToString();

            if (ipAddress == null)
            {
                return RateLimitPartition.GetNoLimiter(""); // No IP address, no rate limiting
            }

            return RateLimitPartition.GetFixedWindowLimiter(ipAddress,
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10,
                    QueueLimit = 0, // No queueing, reject requests immediately
                    Window = TimeSpan.FromMinutes(1)
                });
        }

        public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app, IConfiguration configuration)
        {
            using var scope = app.Services.CreateScope();
            
                bool clearDatabase = configuration.GetValue<bool>("DatabaseSettings:ClearDatabaseOnStartup");
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); 

                try
                {                    
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                    var migrator = dbContext.Database.GetService<IMigrator>();


                    if (clearDatabase)
                    {
                        logger.LogWarning("WARNING: ClearDatabaseOnStartup is set to true. We will try to revert all migrations, and apply them again");

                        try
                        {                           
                            await migrator.MigrateAsync("0"); // "0" means state before migration
                            logger.LogInformation("All migrations reverted.");    

                        }
                        catch (Exception exMigrate)
                        {
                            logger.LogError(exMigrate, "Something went wrong :(");
                            throw;
                        }
                    }

                    await migrator.MigrateAsync(); 
                    logger.LogInformation("All migrations executed.");
                    
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Something went wrong :(");
                }
            

            return app;
        }


    }
}
