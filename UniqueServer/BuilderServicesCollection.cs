using BookshelfDbContextDAL;
using InventoryDbContextDAL;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using UserBLL.Functions;
using UserManagementBLL.Functions;
using UserManagementDAL;

namespace UniqueServer
{
    public static class BuilderServicesCollection
    {
        public static string GetConfigValue(IConfiguration Configuration, string key)
            => Configuration[key] ?? throw new ArgumentNullException(nameof(key));

        public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration Configuration)
        {
            string? inventoryConn = GetConfigValue(Configuration, "ConnectionStrings:InventoryConn");
            string? bookshelfConn = GetConfigValue(Configuration, "ConnectionStrings:BookshelfConn");
            string? userManagementfConn = GetConfigValue(Configuration, "ConnectionStrings:UserManagementConn");

            services.AddMySql<InventoryDbContext>(inventoryConn, ServerVersion.AutoDetect(inventoryConn));
            services.AddMySql<BookshelfDbContext>(bookshelfConn, ServerVersion.AutoDetect(bookshelfConn));
            services.AddMySql<UserManagementDbContext>(userManagementfConn, ServerVersion.AutoDetect(userManagementfConn));

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration Configuration)
        {
            #region User

            services.AddScoped<ISendRecoverPasswordEmailService, SendRecoverPasswordEmailService>(p =>
            new SendRecoverPasswordEmailService(
                new BaseModels.Configs.SendEmailKeys(
                    GetConfigValue(Configuration, "SendEmailKeys:Host"),
                    GetConfigValue(Configuration, "SendEmailKeys:SenderEmail"),
                    GetConfigValue(Configuration, "SendEmailKeys:SenderPassword"),
                    GetConfigValue(Configuration, "SendEmailKeys:Url")
                    )));

            services.AddScoped<IEncryptionService, EncryptionService>(p =>
            new EncryptionService(
                GetConfigValue(Configuration, "Encryption:Key32"),
                GetConfigValue(Configuration, "Encryption:IV16")
                ));

            services.AddScoped<IJwtTokenService, JwtTokenService>(p => new JwtTokenService(GetConfigValue(Configuration, "JwtKey")));

            #endregion


            return services;
        }

        public static IServiceCollection AddLimiterRules(this IServiceCollection services)
        {
            services.AddRateLimiter(options => options.AddFixedWindowLimiter(policyName: "fixed", options =>
            {
                options.PermitLimit = 4;
                options.Window = TimeSpan.FromSeconds(12);
                options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            }).OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    await context.HttpContext.Response.WriteAsync(
                        $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s).", cancellationToken: token
                        );
                }
                else
                {
                    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later");
                }
            }
            );

            return services;
        }
    }
}
