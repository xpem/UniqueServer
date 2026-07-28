using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using UniqueServer;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Error()
    .MinimumLevel.Information()
    .WriteTo.File(Path.Combine("logs", "uniqueServer.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.AddControllers();

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Version = "1.34.3",
            Title = "Unique Server",
            Description = "Routes of apis for Bookshelf, Users Management and Inventory projects",
        };

        string baseUrl = builder.Environment.IsDevelopment() ? "/" : "/api";
        document.Servers = [new() { Url = baseUrl }];

        return Task.CompletedTask;
    });

    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddDbContexts(builder.Configuration);

builder.Services.AddRepos(builder.Configuration);

builder.Services.AddServices(builder.Configuration);

#region Auth configs

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtKey"]))
    };
    options.SaveToken = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigins",
        policy => policy
           .WithOrigins("https://localhost:7223", "https://xpem.vps-kinghost.net")
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod());
});

builder.Services.AddAuthorization();

#endregion

builder.Services.AddLimiterRules();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // Força a API inteira a escutar embaixo de /api em produção
    app.UsePathBase("/api");
}

app.UseCors("AllowFrontendOrigins");
app.UseForwardedHeaders();

app.UseHsts();
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Unique Server");
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.Use(async (context, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        throw;
    }
});

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");

app.Run();
