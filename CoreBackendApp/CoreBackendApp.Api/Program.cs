using CoreBackendApp.Api.Authorization;
using CoreBackendApp.Api.Common;
using CoreBackendApp.Api.Endpoints;
using CoreBackendApp.Api.Middleware;
using CoreBackendApp.Application;
using CoreBackendApp.Infrastructure;
using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;

namespace CoreBackendApp.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    new Serilog.Formatting.Compact.CompactJsonFormatter(),
                    "logs/logs.json",
                    rollingInterval: RollingInterval.Day)
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                MapsterConfig.RegisterMappings();
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                // Add services to the container.
                builder.Services.AddControllers();
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddMemoryCache();
                
                builder.Services.AddApplication();
                builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsEnvironment("Testing"));

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

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreBackendApp API", Version = "v1" });
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Description = "Enter: Bearer {your JWT token}"
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                            },
                            Array.Empty<string>()
                        }
                    });
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
                                Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "YourSuperSecretKeyThatIsVeryLong123!")),
                            ClockSkew = TimeSpan.Zero
                        };
                    });

                builder.Services.AddAuthorization();
                builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
                builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

                builder.Services.AddHealthChecks();

                var app = builder.Build();

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

                app.UseMiddleware<LogEnrichmentMiddleware>();
                app.UseMiddleware<GlobalExceptionMiddleware>();
                app.MapHealthChecks("/health");

                // Seed only if NOT in Testing
                if (app.Environment.EnvironmentName != "Testing")
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
                    db.Database.EnsureCreated();
                    await CoreDbSeeder.SeedAsync(db);
                }

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
