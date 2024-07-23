using BookshelfDbContextDAL;
using InventoryDbContextDAL;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using UserManagementDAL;

namespace UniqueServer
{
    public static class BuilderServicesCollection
    {
        public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration Configuration)
        {
            string? inventoryConn = Configuration["ConnectionStrings:InventoryConn"];
            string? bookshelfConn = Configuration["ConnectionStrings:BookshelfConn"];
            string? userManagementfConn = Configuration["ConnectionStrings:UserManagementConn"];

            services.AddMySql<InventoryDbContext>(inventoryConn, ServerVersion.AutoDetect(inventoryConn));
            services.AddMySql<BookshelfDbContext>(bookshelfConn, ServerVersion.AutoDetect(bookshelfConn));
            services.AddMySql<UserManagementDbContext>(userManagementfConn, ServerVersion.AutoDetect(userManagementfConn));

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
