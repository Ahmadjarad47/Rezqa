namespace Rezqa.API.RateLimiting.Options;

public class RateLimitSettings
{
    // IP Rate Limiting
    public bool EnableIpRateLimiting { get; set; } = true;
    public string IpRateLimitPeriod { get; set; } = "1m";
    public int IpRateLimitLimit { get; set; } = 60;
    public string IpRateLimitHourlyPeriod { get; set; } = "1h";
    public int IpRateLimitHourlyLimit { get; set; } = 1000;

    // Client Rate Limiting
    public bool EnableClientRateLimiting { get; set; } = true;
    public string ClientRateLimitPeriod { get; set; } = "1m";
    public int ClientRateLimitLimit { get; set; } = 60;

    // Auth Rate Limiting
    public string AuthRateLimitPeriod { get; set; } = "1m";
    public int AuthRateLimitLimit { get; set; } = 20;

    // Localhost Rate Limiting
    public string LocalhostIpRule { get; set; } = "::1/128";
    public string LocalhostRateLimitPeriod { get; set; } = "1m";
    public int LocalhostRateLimitLimit { get; set; } = 100;

    // API Client Rate Limiting
    public string ApiClientId { get; set; } = "api-client";
    public string ApiClientRateLimitPeriod { get; set; } = "1m";
    public int ApiClientRateLimitLimit { get; set; } = 100;

    // General Settings
    public bool EnableEndpointRateLimiting { get; set; } = true;
    public bool StackBlockedRequests { get; set; } = false;
    public string ClientIdHeader { get; set; } = "X-ClientId";
    public int RateLimitHttpStatusCode { get; set; } = 429;

    // Advanced Settings
    public bool EnableRateLimitMonitoring { get; set; } = true;
    public bool EnableRateLimitLogging { get; set; } = true;
    public string RateLimitLogLevel { get; set; } = "Information";
} 