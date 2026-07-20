using FluentValidation;
using IslaNova.Core.Application.Dtos.Account;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Settings;
using IslaNova.Infrastructure.Identity.Contexts;
using IslaNova.Infrastructure.Identity.Entities;
using IslaNova.Infrastructure.Identity.Seeds;
using IslaNova.Infrastructure.Identity.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace IslaNova.Infrastructure.Identity.IOC
{
    public static class ServiceRegistration
    {

        public static void AddIdentityLayerIocForWebApi(this IServiceCollection services, IConfiguration config)
        {

            GeneralConfiguration(services, config);

            #region Configurations
            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
            services.AddMediatR(opt => opt.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            #endregion

            #region Identity 
            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;

                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opt.Lockout.MaxFailedAccessAttempts = 5;

                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
            });

            services.AddIdentityCore<User>()
                .AddSignInManager()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(12);
            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = false;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidAudience = config["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"] ?? ""))
                };
                opt.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        // Read the JWT from the HttpOnly cookie instead of (or in addition to) the Authorization header
                        if (context.Request.Cookies.TryGetValue("accessToken", out var token) && !string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = af =>
                    {
                        af.NoResult();
                        if (af.Response.HasStarted) return Task.CompletedTask;
                        af.Response.StatusCode = 500;
                        af.Response.ContentType = "text/plain";
                        return af.Response.WriteAsync(af.Exception.Message.ToString());
                    },
                    OnChallenge = c =>
                    {
                        c.HandleResponse();
                        if (c.Response.HasStarted) return Task.CompletedTask;
                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new JwtResponseDto { HasError = true, Error = "You are not Authorized" });
                        return c.Response.WriteAsync(result);
                    },
                    OnForbidden = c =>
                    {
                        c.Response.StatusCode = 403;
                        c.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new JwtResponseDto { HasError = true, Error = "You are not Authorized to access this resource" });
                        return c.Response.WriteAsync(result);
                    }
                };
            }).AddCookie(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(180);
            });

            #endregion


            #region Services
            services.AddScoped<IAuthServiceForWebApi, AuthServiceForWebApi>();
            #endregion

        }

        public static async Task RunIdentitySeedAsync(this IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var servicesProvider = scope.ServiceProvider;

            var userManager = servicesProvider.GetRequiredService<UserManager<User>>();
            var roleManager = servicesProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await DefaultRoles.SeedAsync(roleManager);
            await DefaultAdminUser.SeedAsync(userManager);
            await DefaultAgentUser.SeedAsync(userManager);
        }

        #region Private methods
        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {

            #region Contexts
            var useMemoryDb = config.GetValue<bool>("UseInMemoryDatabase");
            var currentDev = config.GetValue<string>("CurrentDev");
            var connectionString = config.GetConnectionString(currentDev ?? "Default");

            if (useMemoryDb)
            {
                services.AddDbContext<IdentityContext>(opt =>
                    opt.UseInMemoryDatabase("AppDb"));
            }
            else
            {
                services.AddDbContext<IdentityContext>(
                    (serviceProvider, options) =>
                    {
                        options.EnableSensitiveDataLogging();
                        options.UseNpgsql(
                            connectionString,
                            npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                                typeof(IdentityContext).Assembly.FullName)
                        );
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                );

            }

            #endregion

        }
        #endregion

    }
}
