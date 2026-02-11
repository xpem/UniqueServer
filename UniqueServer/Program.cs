using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using UniqueServer;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Error()
    //.MinimumLevel.Information()
    .WriteTo.File(Path.Combine("logs", "uniqueServer.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = $"1.31.1",
        Title = "Unique Server",
        Description = "Routes of apis for Bookshelf, Users Management and Inventory projects",
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    if (!builder.Environment.IsDevelopment())
    {
        c.AddServer(new OpenApiServer { Url = "/api" });
    }
    else
    {
        // Em desenvolvimento, o Swagger pode usar a raiz como base
        c.AddServer(new OpenApiServer { Url = "/" });
    }

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
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

app.UseCors("AllowFrontendOrigins");
app.UseForwardedHeaders();

app.UseHsts();
app.UseSwagger();
app.UseSwaggerUI();

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");


app.Run();
