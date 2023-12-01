using BaseModels;
using BookshelfBLL;
using BookshelfDAL;
using BookshelfDbContextDAL;
using InventoryBLL;
using InventoryDAL;
using InventoryDbContextDAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserBLL;
using UserManagementDAL;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region DI DAL

builder.Services.AddScoped<IBookDAL, BookDAL>();
builder.Services.AddScoped<IBookHistoricDAL, BookHistoricDAL>();

builder.Services.AddScoped<IUserDAL, UserManagementDAL.UserDAL>();
builder.Services.AddScoped<IUserHistoricDAL, UserHistoricDAL>();

builder.Services.AddScoped<ISubCategoryDAL, SubCategoryDAL>();

#region DI BLL

builder.Services.AddScoped<IUserBLL, UserBLL.UserBLL>();

builder.Services.AddScoped<IBookBLL, BookBLL>();
builder.Services.AddScoped<IBookHistoricBLL, BookHistoricBLL>();

builder.Services.AddScoped<ICategoryBLL, CategoryBLL>();
builder.Services.AddScoped<ISubCategoryBLL, SubCategoryBLL>();

#endregion

#endregion

#region AppContexts

builder.Services.AddDbContext<UserManagementDbContext>();
builder.Services.AddDbContext<BookshelfDbContext>();
builder.Services.AddDbContext<InventoryDbContext>();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(PrivateKeys.JwtKey))
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

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
