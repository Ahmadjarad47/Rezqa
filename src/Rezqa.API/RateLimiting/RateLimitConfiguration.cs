using AspNetCoreRateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rezqa.API.RateLimiting.Services;
using Rezqa.API.RateLimiting.Models;
using Rezqa.API.RateLimiting.Options;

namespace Rezqa.API.RateLimiting;

public static class RateLimitConfiguration
{
    public static IServiceCollection AddAdvancedRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration to options
        services.Configure<RateLimitSettings>(configuration.GetSection("RateLimit"));
        
        // Configure rate limiting from environment variables with fallback to configuration
        var settings = GetRateLimitSettings(configuration);

        // Configure IP Rate Limiting
        ConfigureIpRateLimiting(services, settings);

        // Configure Client Rate Limiting
        ConfigureClientRateLimiting(services, settings);

        // Configure User Rate Limiting (if needed)
        ConfigureUserRateLimiting(services, settings);

        // Add rate limit services
        RegisterRateLimitServices(services);

        return services;
    }

    private static RateLimitSettings GetRateLimitSettings(IConfiguration configuration)
    {
        return new RateLimitSettings
        {
            // IP Rate Limiting
            EnableIpRateLimiting = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_IP_RATE_LIMITING") ?? "true"),
            IpRateLimitPeriod = Environment.GetEnvironmentVariable("IP_RATE_LIMIT_PERIOD") ?? "1m",
            IpRateLimitLimit = int.Parse(Environment.GetEnvironmentVariable("IP_RATE_LIMIT_LIMIT") ?? "60"),
            IpRateLimitHourlyPeriod = Environment.GetEnvironmentVariable("IP_RATE_LIMIT_HOURLY_PERIOD") ?? "1h",
            IpRateLimitHourlyLimit = int.Parse(Environment.GetEnvironmentVariable("IP_RATE_LIMIT_HOURLY_LIMIT") ?? "1000"),
            
            // Client Rate Limiting
            EnableClientRateLimiting = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_CLIENT_RATE_LIMITING") ?? "true"),
            ClientRateLimitPeriod = Environment.GetEnvironmentVariable("CLIENT_RATE_LIMIT_PERIOD") ?? "1m",
            ClientRateLimitLimit = int.Parse(Environment.GetEnvironmentVariable("CLIENT_RATE_LIMIT_LIMIT") ?? "60"),
            
            // Auth Rate Limiting
            AuthRateLimitPeriod = Environment.GetEnvironmentVariable("AUTH_RATE_LIMIT_PERIOD") ?? "1m",
            AuthRateLimitLimit = int.Parse(Environment.GetEnvironmentVariable("AUTH_RATE_LIMIT_LIMIT") ?? "20"),
            
            // Localhost Rate Limiting
            LocalhostIpRule = Environment.GetEnvironmentVariable("LOCALHOST_IP_RULE") ?? "::1/128",
            LocalhostRateLimitPeriod = Environment.GetEnvironmentVariable("LOCALHOST_RATE_LIMIT_PERIOD") ?? "1m",
            LocalhostRateLimitLimit = int.Parse(Environment.GetEnvironmentVariable("LOCALHOST_RATE_LIMIT_LIMIT") ?? "100"),
            
            // API Client Rate Limiting
            ApiClientId = Environment.GetEnvironmentVariable("API_CLIENT_ID") ?? "api-client",
            ApiClientRateLimitPeriod = Environment.GetEnvironmentVariable("API_CLIENT_RATE_LIMIT_PERIOD") ?? "1m",
            ApiClientRateLimitLimit = int.Parse(Environment.GetEnvironmentVariable("API_CLIENT_RATE_LIMIT_LIMIT") ?? "100"),
            
            // General Settings
            EnableEndpointRateLimiting = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_ENDPOINT_RATE_LIMITING") ?? "true"),
            StackBlockedRequests = bool.Parse(Environment.GetEnvironmentVariable("STACK_BLOCKED_REQUESTS") ?? "false"),
            ClientIdHeader = Environment.GetEnvironmentVariable("CLIENT_ID_HEADER") ?? "X-ClientId",
            RateLimitHttpStatusCode = int.Parse(Environment.GetEnvironmentVariable("RATE_LIMIT_HTTP_STATUS_CODE") ?? "429")
        };
    }

    private static void ConfigureIpRateLimiting(IServiceCollection services, RateLimitSettings settings)
    {
        if (!settings.EnableIpRateLimiting) return;

        services.Configure<IpRateLimitOptions>(options =>
        {
            options.EnableEndpointRateLimiting = settings.EnableEndpointRateLimiting;
            options.StackBlockedRequests = settings.StackBlockedRequests;
            options.ClientIdHeader = settings.ClientIdHeader;
            options.HttpStatusCode = settings.RateLimitHttpStatusCode;
            options.GeneralRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = settings.IpRateLimitPeriod,
                    Limit = settings.IpRateLimitLimit
                },
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = settings.IpRateLimitHourlyPeriod,
                    Limit = settings.IpRateLimitHourlyLimit
                },
                new RateLimitRule
                {
                    Endpoint = "post:/api/auth/*",
                    Period = settings.AuthRateLimitPeriod,
                    Limit = settings.AuthRateLimitLimit
                }
            };
        });

        // Configure IP Rate Limit Policies
        services.Configure<IpRateLimitPolicies>(options =>
        {
            options.IpRules = new List<IpRateLimitPolicy>
            {
                new IpRateLimitPolicy
                {
                    Ip = settings.LocalhostIpRule,
                    Rules = new List<RateLimitRule>
                    {
                        new RateLimitRule
                        {
                            Endpoint = "*",
                            Period = settings.LocalhostRateLimitPeriod,
                            Limit = settings.LocalhostRateLimitLimit
                        }
                    }
                }
            };
        });
    }

    private static void ConfigureClientRateLimiting(IServiceCollection services, RateLimitSettings settings)
    {
        if (!settings.EnableClientRateLimiting) return;

        services.Configure<ClientRateLimitOptions>(options =>
        {
            options.EnableEndpointRateLimiting = settings.EnableEndpointRateLimiting;
            options.StackBlockedRequests = settings.StackBlockedRequests;
            options.ClientIdHeader = settings.ClientIdHeader;
            options.HttpStatusCode = settings.RateLimitHttpStatusCode;
            options.GeneralRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = settings.ClientRateLimitPeriod,
                    Limit = settings.ClientRateLimitLimit
                }
            };
        });

        // Configure Client Rate Limit Policies
        services.Configure<ClientRateLimitPolicies>(options =>
        {
            options.ClientRules = new List<ClientRateLimitPolicy>
            {
                new ClientRateLimitPolicy
                {
                    ClientId = settings.ApiClientId,
                    Rules = new List<RateLimitRule>
                    {
                        new RateLimitRule
                        {
                            Endpoint = "*",
                            Period = settings.ApiClientRateLimitPeriod,
                            Limit = settings.ApiClientRateLimitLimit
                        }
                    }
                }
            };
        });
    }

    private static void ConfigureUserRateLimiting(IServiceCollection services, RateLimitSettings settings)
    {
        // Future implementation for user-based rate limiting
        // This can be used for authenticated user rate limiting
    }

    private static void RegisterRateLimitServices(IServiceCollection services)
    {
        // Register memory cache for AspNetCoreRateLimit services
        services.AddMemoryCache();
        
        // Register built-in RateLimitConfiguration for AspNetCoreRateLimit
        services.AddSingleton<IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();
        
        // Register default AspNetCoreRateLimit services
        services.AddInMemoryRateLimiting();
        
        // Register custom rate limit counter store
        services.AddSingleton<IRateLimitCounterStore, AdvancedRateLimitCounterStore>();
        
        // Register rate limit monitoring service
        services.AddSingleton<IRateLimitMonitoringService, RateLimitMonitoringService>();
    }

    public static IApplicationBuilder UseAdvancedRateLimiting(this IApplicationBuilder app)
    {
        // Add IP rate limiting middleware
        app.UseIpRateLimiting();

        // Add client rate limiting middleware
        app.UseClientRateLimiting();

        return app;
    }
} 