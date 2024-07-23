using BookshelfBLL;
using BookshelfDAL;
using BookshelfDbContextDAL;
using InventoryBLL;
using InventoryBLL.Interfaces;
using InventoryDAL;
using InventoryDAL.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UniqueServer;
using UserBLL;
using UserBLL.Functions;
using UserManagementBLL.Functions;
using UserManagementDAL;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//IConfiguration configurationBuilder = new ConfigurationBuilder()
//                            .AddJsonFile("appsettings.json")
//    //.AddJsonFile($"appsettings.Development.json", true, true)
//    .AddEnvironmentVariables()
//.Build();

//var builder = WebApplication.CreateBuilder(new WebApplicationOptions
//{
//    EnvironmentName = configurationBuilder.GetSection("Hosting")["Environment"]
//});

//#if DEBUG

//var builder = WebApplication.CreateBuilder(new WebApplicationOptions
//{
//    EnvironmentName = configurationBuilder.GetSection("Hosting")["Environment"]
//});
//#else
////var builder = WebApplication.CreateBuilder(args);
//#endif

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    op =>
    {
        op.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = $"1.3",
            Title = "Unique Server",
            Description = "Routes of apis for Bookshelf, Users Management and Inventory projects",
        });
    }
    );

#region AppContexts

builder.Services.AddDbContexts(builder.Configuration);

#endregion

#region DI DAL

//bookshelf
builder.Services.AddScoped<IBookDAL, BookDAL>();
builder.Services.AddScoped<IBookHistoricDAL, BookHistoricDAL>();

//usermanagement
builder.Services.AddScoped<IUserDAL, UserManagementDAL.UserDAL>();
builder.Services.AddScoped<IUserHistoricDAL, UserHistoricDAL>();

//inventory
builder.Services.AddScoped<ISubCategoryDAL, SubCategoryDAL>();
builder.Services.AddScoped<ICategoryDAL, CategoryDAL>();
builder.Services.AddScoped<IAcquisitionTypeDAL, AcquisitionTypeDAL>();
builder.Services.AddScoped<IItemSituationDAL, ItemSituationDAL>();
builder.Services.AddScoped<IItemDAL, ItemDAL>();

#endregion

#region DI BLL

//usermanagement
builder.Services.AddScoped<IUserBLL, UserBLL.UserBLL>();
builder.Services.AddScoped<ISendRecoverPasswordEmailService, SendRecoverPasswordEmailService>(p =>
new SendRecoverPasswordEmailService(
    new BaseModels.Configs.SendEmailKeys(
        builder.Configuration["SendEmailKeys:Host"],
        builder.Configuration["SendEmailKeys:SenderEmail"],
        builder.Configuration["SendEmailKeys:SenderPassword"],
        builder.Configuration["SendEmailKeys:Url"]
    )));
builder.Services.AddScoped<IEncryptionService, EncryptionService>(p => 
new EncryptionService(
    builder.Configuration["Encryption:Key32"],
    builder.Configuration["Encryption:IV16"]
    ));

//bookshelf
builder.Services.AddScoped<IBookBLL, BookBLL>();
builder.Services.AddScoped<IBookHistoricBLL, BookHistoricBLL>();

//inventory
builder.Services.AddScoped<ICategoryBLL, CategoryBLL>();
builder.Services.AddScoped<ISubCategoryBLL, SubCategoryBLL>();
builder.Services.AddScoped<IAcquisitionTypeBLL, AcquisitionTypeBLL>();
builder.Services.AddScoped<IItemSituationBLL, ItemSituationBLL>();
builder.Services.AddScoped<IItemBLL, ItemBLL>();

#endregion



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

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
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

builder.Services.AddAuthorization();

#endregion

builder.Services.AddLimiterRules();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();

//}

app.UseRateLimiter();
app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");

app.Run();
