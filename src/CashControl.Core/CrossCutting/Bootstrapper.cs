using CashControl.Core.API;
using CashControl.Core.HealthCheck;
using CashControl.Core.Infra;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.Extensions.Hosting;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Configuration;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace CashControl.Core.CrossCutting;

public static class Bootstrapper
{
    public static IServiceCollection RegisterCore(this IServiceCollection services, CoreSettings settings)
    {
        services.AddSingleton<CoreSettings, CoreSettings>();

        services.RegisterCrossCutting(settings);

        services.RegisterApi(settings);

        services.RegisterApplication(settings);

        return services;
    }

    private static IServiceCollection RegisterCrossCutting(this IServiceCollection services, CoreSettings settings)
    {
        // IHttpContextAccessor
        if (settings.ConfigureIHttpContextAccessor)
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // IEnvironment
        if (settings.ConfigureIEnvironment)
        {
            if (settings.HostEnvironment == null)
                throw new ArgumentException(nameof(settings.HostEnvironment));

            if (settings.Configuration == null)
                throw new ArgumentException(nameof(settings.Configuration));

            var environmentName = settings.HostEnvironment.EnvironmentName;
            var parameters = settings.Configuration.AsEnumerable().ToList();

            var environment = new Environment(environmentName, parameters);
            services.AddSingleton<IEnvironment>(environment);
        }

        // ILoggedUserProvider
        if (settings.ConfigureILoggedUserProvider)
            services.AddScoped<ILoggedUserProvider, LoggedUserProvider>();

        return services;
    }

    private static IServiceCollection RegisterApi(this IServiceCollection services, CoreSettings settings)
    {
        // Memory Cache
        if (settings.ConfigureMemoryCache)
            services.AddMemoryCache();

        // Health Checks
        if (settings.ConfigureHealthChecks)
        {
            var healthChecksBuilder = services.AddHealthChecks();

            if (settings.IsMultiTenancyEnabled)
            {
                healthChecksBuilder
                    .AddCheck<TenantsDbContextHealthCheck>("Conexão com database - Tenants");
            }
            else if (!string.IsNullOrEmpty(settings.Configuration?[EFCoreContext.ConnStringKey]))
            {
                healthChecksBuilder
                    .AddCheck<DbContextHealthCheck>("Conexão com database - DefaultConnection");
            }
        }

        // Gzip Compression
        if (settings.ConfigureGzipCompression)
            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; })
                .AddResponseCompression(options =>
                {
                    options.Providers.Add<GzipCompressionProvider>();
                    options.EnableForHttps = true;
                });

        if (settings.UseTelemetry)
            services.AddElasticApmForAspNetCore();

        if (settings.RegisterSwagger && settings.SwaggerSettings != null)
        {
            services.AddSwaggerGen(c =>
            {
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Description = settings.SwaggerSettings.Description,
                    Title = "Serviço " + settings.SwaggerSettings.Name,
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Por favor, insira o token JWT Bearer no campo.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
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
                        new string[] { }
                    }
                });
            });
        }

        if (settings.RegisterAuthenticationAndAuthorization)
        {
            services.Configure<JwtOptions>(settings.Configuration!.GetSection("Jwt"));
            var jwt = settings.Configuration!.GetSection("Jwt").Get<JwtOptions>()!;
            var key = Encoding.UTF8.GetBytes(jwt.Key);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder("Bearer");
                defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });
        }

        IMvcBuilder? builder = null;
        if (settings.AddDefaultControllers)
            builder = services.AddControllers();

        return services;
    }

    private static IServiceCollection RegisterApplication(this IServiceCollection services, CoreSettings settings)
    {
        // Core Mediator
        if (settings.ConfigureCoreMediator)
            services.AddScoped<IMediator, Mediator>();

        // Mediator Handlers
        if (settings.ConfigureMediatorHandlers)
        {
            if (string.IsNullOrEmpty(settings.ApplicationAssemblyName))
                throw new ArgumentException(nameof(settings.ApplicationAssemblyName));

            services.AddMediatorHandlers(settings.ApplicationAssemblyName);
        }

        // Mediator Pipeline Behaviors
        // ReSharper disable once InvertIf
        if (settings.ConfigureMediatorPipelineBehaviors)
        {
            if (string.IsNullOrEmpty(settings.ApplicationAssemblyName))
                throw new ArgumentException(nameof(settings.ApplicationAssemblyName));

            services.AddMediatorPipelineBehaviors(settings.ApplicationAssemblyName);
        }

        return services;
    }

    private static IServiceCollection AddMediatorHandlers(this IServiceCollection services, string applicationAssemblyName)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.Load(applicationAssemblyName));
        });

        return services;
    }

    private static IServiceCollection AddMediatorPipelineBehaviors(this IServiceCollection services, string applicationAssemblyName)
    {
        var assembly = AppDomain.CurrentDomain.Load(applicationAssemblyName);

        AssemblyScanner
            .FindValidatorsInAssembly(assembly)
            .ForEach(result => services.AddScoped(result.InterfaceType, result.ValidatorType));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FailFastPipelineBehavior<,>));

        return services;
    }

    public static IApplicationBuilder UseCore(this IApplicationBuilder app, CoreSettings settings)
    {
        if (settings.ConfigureExceptionMiddleware)
            app.UseMiddleware<ExceptionMiddleware>();

        if (settings.HostEnvironment == null)
            return app;

        if (settings.HostEnvironment.IsDevelopment() && settings.ConfigureUseDeveloperExceptionPageWhenInDevelopmentEnvironment)
            app.UseDeveloperExceptionPage();

        if (settings.ConfigureHealthChecks)
            app.UseHealthChecks(settings.HealthCheckEndpoint, new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponse.WriteDetailed,
            });

        if (settings.RegisterSwagger && settings.SwaggerSettings != null)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint(settings.SwaggerSettings.EndpointUrl, settings.SwaggerSettings.EndpointName));
        }

        if (settings.UseDefaultRouting)
            app.UseRouting();

        if (settings.RegisterAuthenticationAndAuthorization)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        if (settings.UseDefaultEndpoints)
            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

        return app;
    }
}
