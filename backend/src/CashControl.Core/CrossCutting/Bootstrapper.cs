using CashControl.Core.API;
using CashControl.Core.HealthCheck;
using CashControl.Core.Infra;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.Extensions.Hosting;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
using System.Reflection;

namespace CashControl.Core.CrossCutting;

public static class Bootstrapper
{
    public static IServiceCollection RegisterCore(this IServiceCollection services, CoreSettings settings)
    {
        services.AddSingleton(settings);

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
        var isLocalLikeEnvironment = settings.HostEnvironment?.IsDevelopment() == true || string.Equals(settings.HostEnvironment?.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase);
        var sessionCookieOptions = settings.Configuration?.GetSection("SessionCookie").Get<SessionCookieOptions>() ?? new SessionCookieOptions();
        var antiforgeryOptions = settings.Configuration?.GetSection("Antiforgery").Get<AntiforgeryOptionsSettings>() ?? new AntiforgeryOptionsSettings();
        ValidateHostCookieConfiguration(sessionCookieOptions.Name, sessionCookieOptions.Domain, "SessionCookie");
        ValidateHostCookieConfiguration(antiforgeryOptions.CookieName, antiforgeryOptions.CookieDomain, "Antiforgery");

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
                c.AddSecurityDefinition("CookieAuth", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Cookie,
                    Description = "Sessao autenticada via cookie da aplicacao.",
                    Name = sessionCookieOptions.Name,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityDefinition("CsrfHeader", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Header antiforgery obrigatorio para operacoes autenticadas de escrita.",
                    Name = antiforgeryOptions.HeaderName,
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
                                Id = "CookieAuth"
                            }
                        },
                        new string[] { }
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "CsrfHeader"
                            }
                        },
                        new string[] { }
                    }
                });
            });
        }

        if (settings.RegisterAuthenticationAndAuthorization)
        {
            services.Configure<SessionCookieOptions>(settings.Configuration!.GetSection("SessionCookie"));
            services.Configure<AntiforgeryOptionsSettings>(settings.Configuration!.GetSection("Antiforgery"));
            services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddCookie(IdentityConstants.ApplicationScheme, options =>
                {
                    options.Cookie.Name = sessionCookieOptions.Name;
                    options.Cookie.Domain = NormalizeCookieDomain(sessionCookieOptions.Domain);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = isLocalLikeEnvironment ? CookieSecurePolicy.SameAsRequest : sessionCookieOptions.SecurePolicy;
                    options.Cookie.SameSite = sessionCookieOptions.SameSite;
                    options.Cookie.Path = "/";
                    options.SlidingExpiration = sessionCookieOptions.SlidingExpiration;
                    options.ExpireTimeSpan = TimeSpan.FromHours(sessionCookieOptions.ExpireHours);
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return context.Response.WriteAsJsonAsync(ApiErrorResponse.Unauthorized("Autenticacao obrigatoria."));
                        },
                        OnRedirectToAccessDenied = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return context.Response.WriteAsJsonAsync(ApiErrorResponse.Forbidden("Voce nao tem permissao para acessar este recurso."));
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(IdentityConstants.ApplicationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

                options.AddPolicy("AdminOrSuperUser", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                            context.User.IsInRole("Admin") ||
                            string.Equals(context.User.FindFirst(CustomClaimTypes.IsSuperUser)?.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase)));

                options.AddPolicy("SuperUserOnly", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                            string.Equals(context.User.FindFirst(CustomClaimTypes.IsSuperUser)?.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase)));
            });
        }

        services.AddAntiforgery(options =>
        {
            options.HeaderName = antiforgeryOptions.HeaderName;
            options.Cookie.Name = antiforgeryOptions.CookieName;
            options.Cookie.Domain = NormalizeCookieDomain(antiforgeryOptions.CookieDomain);
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = isLocalLikeEnvironment ? CookieSecurePolicy.SameAsRequest : antiforgeryOptions.SecurePolicy;
            options.Cookie.SameSite = antiforgeryOptions.SameSite;
            options.Cookie.Path = "/";
            options.SuppressXFrameOptionsHeader = false;
        });

        IMvcBuilder? builder = null;
        if (settings.AddDefaultControllers)
            builder = services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new ProducesAttribute("application/json"));
            });

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

        if (settings.ConfigureRateLimiter)
            app.UseRateLimiter();

        if (settings.RegisterAuthenticationAndAuthorization)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        if (settings.UseDefaultEndpoints)
            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

        return app;
    }

    private static void ValidateHostCookieConfiguration(string cookieName, string? cookieDomain, string sectionName)
    {
        if (cookieName.StartsWith("__Host-", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(cookieDomain))
            throw new InvalidOperationException($"A configuracao '{sectionName}' nao pode definir Domain quando o cookie usa o prefixo __Host-.");
    }

    private static string? NormalizeCookieDomain(string? cookieDomain)
        => string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain;
}
