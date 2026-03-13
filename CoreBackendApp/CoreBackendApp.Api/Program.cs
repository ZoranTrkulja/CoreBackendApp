
using CoreBackendApp.Api.Endpoints;
using CoreBackendApp.Api.Middleware;
using CoreBackendApp.Infrastructure.Services;
using CoreBackendApp.Application.Auth;
using CoreBackendApp.Application.Interface;
using CoreBackendApp.Application.Common.Interfaces;
using CoreBackendApp.Infrastructure.Persistence;
using CoreBackendApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;

namespace CoreBackendApp.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();
            
            // Add FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

            builder.Services.AddScoped<ITenantProvider, TenantProvider>();

            // Add API Versioning
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CoreBackendApp API",
                    Version = "v1"
                });

                // JWT Bearer definition
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Enter: Bearer {your JWT token}"
                });

                // Security requirement
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


            builder.Services.AddDbContext<CoreDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
            });

            var jwtSettings = builder.Configuration.GetSection("Jwt");

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key not found."))),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                {
                    policy.RequireRole("Admin");
                });

                options.AddPolicy("RequireUsersReadPermission", policy =>
                {
                    policy.RequireClaim("permissions", "users.read");
                });

                options.AddPolicy("RequireUsersManagePermission", policy =>
                {
                    policy.RequireClaim("permissions", "users.manage");
                });

                options.AddPolicy("RequireUsersFeature", policy =>
                {
                    policy.RequireClaim("features", "users");
                });
            });


            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<CoreDbContext>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            var apiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var versionedGroup = app.MapGroup("api/v{version:apiVersion}")
                .WithApiVersionSet(apiVersionSet);

            versionedGroup.MapAuthEndpoints();
            versionedGroup.MapUserEndpoint();
            versionedGroup.MapRoleEndpoint();
            versionedGroup.MapPermissionEndpoint();
            versionedGroup.MapTenantEndpoint();
            versionedGroup.MapFeatureEndpoint();

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.MapHealthChecks("/health");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();

                db.Database.EnsureCreated();

                await CoreDbSeeder.SeedAsync(db);
            }

            app.Run();
        }
    }
}
