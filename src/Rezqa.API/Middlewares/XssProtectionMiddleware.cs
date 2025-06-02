using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Rezqa.API.Middlewares;

public class XssProtectionMiddleware
{
    private readonly RequestDelegate _next;

    public XssProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Sanitize query string
        if (context.Request.QueryString.HasValue)
        {
            var sanitizedQuery = SanitizeInput(context.Request.QueryString.Value);
            context.Request.QueryString = new QueryString(sanitizedQuery);
        }

        // Sanitize form data
        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();
            var sanitizedForm = new FormCollection(
                form.ToDictionary(
                    kvp => SanitizeInput(kvp.Key),
                    kvp => new StringValues(kvp.Value.Select(v => SanitizeInput(v)).ToArray())
                )
            );
            context.Request.Form = sanitizedForm;
        }

        // Sanitize headers
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.StartsWith("X-", StringComparison.OrdinalIgnoreCase))
            {
                context.Request.Headers[header.Key] = new StringValues(
                    header.Value.Select(v => SanitizeInput(v)).ToArray()
                );
            }
        }

        await _next(context);
    }

    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove any script tags and their contents
        input = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Remove any HTML tags
        input = Regex.Replace(input, @"<[^>]*>", "");

        // Remove any JavaScript event handlers
        input = Regex.Replace(input, @"on\w+=""[^""]*""", "");
        input = Regex.Replace(input, @"on\w+='[^']*'", "");

        // Remove any JavaScript protocol handlers
        input = Regex.Replace(input, @"javascript:", "", RegexOptions.IgnoreCase);
        input = Regex.Replace(input, @"vbscript:", "", RegexOptions.IgnoreCase);

        // Remove any data URLs
        input = Regex.Replace(input, @"data:", "", RegexOptions.IgnoreCase);

        // HTML encode special characters using WebEncoders
        input = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(input));
        input = System.Text.Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(input));

        return input;
    }
} 