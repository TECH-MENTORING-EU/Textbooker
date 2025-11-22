using static Booker.Services.SendMailSvc;
using System.Configuration;
using Booker.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.RateLimiting;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Booker.Authorization;
using System.Net;



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

            services.AddScoped<ItemManager>();
            services.AddScoped<FavoritesManager>();
            services.AddScoped<StaticDataManager>();
            services.AddScoped<PhotosManager>();
            services.AddScoped<IChatService, ChatService>(); // added chat service

            services.AddScoped<IAuthorizationHandler, AdminAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, ItemIsOwnerAuthorizationHandler>();

            services.AddScoped<SessionCacheManager>();
            services.AddHostedService<MaintenanceService>();

            return services;
        }

        public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;

                options.AddPolicy("IpRateLimit", context =>
                {
                    if (context.Request.Method == HttpMethods.Get ||
                        context.Request.Method == HttpMethods.Head ||
                        context.Request.Method == HttpMethods.Options
                    )
                    {
                        return RateLimitPartition.GetNoLimiter("");
                    }

                    return IpRateLimit(context);

                });

                options.AddPolicy("IpRateLimitAllMethods", context =>
                {
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

        public static IMvcBuilder AddCustomRoutes(this IMvcBuilder builder)
        {
            return builder.AddRazorPagesOptions(options =>
            {

                options.Conventions.AddPageRouteModelConvention("/Profile/Index", model =>
                {
                    model.Selectors.Clear();

                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = "Profile/{id:int}"
                        }
                    });
                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = "Profile/{id:int}/Index",
                            SuppressLinkGeneration = true
                        }
                    });
                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = "Profile"
                        }
                    });
                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = "Profile/Index",
                            SuppressLinkGeneration = true
                        }
                    });
                });

                options.Conventions.AddPageRouteModelConvention("/Profile/Favorites", model =>
                {
                    model.Selectors.Clear();

                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = "Profile/{id:int}/Favorites"
                        }
                    });

                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = "Profile/Favorites",
                            Order = 1 // Ensure this route is processed after the default route
                        }
                    });

                });

            });
        }

        public static IMvcBuilder AddAuthorizationPolicies(this IMvcBuilder builder)
        {
            return builder.AddRazorPagesOptions(options =>
            {
                options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminHidden");
            });
        }

        public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("AdminHidden", policy => policy.Requirements.Add(new AdminHiddenAuthorizationRequirement()));
            });

            services.ConfigureApplicationCookie(options =>
            {
                var redirectToAccessDenied = options.Events.OnRedirectToAccessDenied;
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.HttpContext.Items.TryGetValue("HideUnauthorized", out var hide)
                        && hide is true)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        context.RedirectUri = string.Empty;
                        return Task.CompletedTask;
                    }
                    return redirectToAccessDenied(context);
                };
                var redirectToLogin = options.Events.OnRedirectToLogin;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.HttpContext.Items.TryGetValue("HideUnauthorized", out var hide)
                        && hide is true)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        context.RedirectUri = string.Empty;
                        return Task.CompletedTask;
                    }
                    return redirectToLogin(context);
                };
            });

            return services;
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

        public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app, int itemsCount = 50, int usersCount = 5)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                await DevDbInitializer.Initialize(dbContext, itemsCount, usersCount);
                logger.LogInformation("Database initialized with {ItemsCount} items and {UsersCount} users.", itemsCount, usersCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something went wrong during database initialization.");
            }
            return app;
        }

        public static async Task<WebApplication> InitializeRolesAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole<int>("Admin"));

            return app;
        }


    }
}