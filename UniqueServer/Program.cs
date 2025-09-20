using BookshelfRepo;
using BookshelfServices;
using InventoryBLL;
using InventoryBLL.Interfaces;
using InventoryRepos;
using InventoryRepos.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.VisualBasic;
using System.Text;
using UniqueServer;
using UserManagementRepo;
using UserManagementService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = $"1.23",
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

#region AppContexts

builder.Services.AddDbContexts(builder.Configuration);

#endregion

#region DI DAL

//bookshelf
builder.Services.AddScoped<IBookRepo, BookRepo>();
builder.Services.AddScoped<IBookHistoricRepo, BookHistoricRepo>();

//usermanagement
builder.Services.AddScoped<IUserRepo, UserManagementRepo.UserRepo>();
builder.Services.AddScoped<IUserHistoricRepo, UserHistoricRepo>();

//inventory
builder.Services.AddScoped<ISubCategoryRepo, SubCategoryRepo>();
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
builder.Services.AddScoped<IAcquisitionTypeRepo, AcquisitionTypeRepo>();
builder.Services.AddScoped<IItemSituationRepo, ItemSituationRepo>();
builder.Services.AddScoped<IItemRepo, ItemRepo>();

#endregion

#region DI Service Layer

//usermanagement
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserDataDeleteService, UserDataDeleteService>();

builder.Services.AddServices(builder.Configuration);

//bookshelf
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBookHistoricService, BookHistoricService>();

//inventory
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
builder.Services.AddScoped<IAcquisitionTypeService, AcquisitionTypeService>();
builder.Services.AddScoped<IItemSituationService, ItemSituationService>();
builder.Services.AddScoped<IItemService, ItemService>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy => policy
           .WithOrigins("https://localhost:7223", "https://xpem.vps-kinghost.net") // Adicionado o domínio de produção
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod());
});



builder.Services.AddAuthorization();

#endregion

builder.Services.AddLimiterRules();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRateLimiter();
app.UseAuthentication();

app.UseHttpsRedirection();

app.UseCors("AllowLocalhost");

app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");


app.Run();
