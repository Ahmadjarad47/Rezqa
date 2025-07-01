using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Text;
using System.Text.Json;

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
            var queryDictionary = Microsoft.AspNetCore.WebUtilities
                .QueryHelpers.ParseQuery(context.Request.QueryString.Value);

            var sanitizedQuery = new Dictionary<string, StringValues>();

            foreach (var pair in queryDictionary)
            {
                var key = SanitizeInput(pair.Key);
                var value = new StringValues(pair.Value.Select(SanitizeInput).ToArray());
                sanitizedQuery[key] = value;
            }

            var newQuery = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString("", sanitizedQuery);
            context.Request.QueryString = new QueryString(newQuery);
        }

        // Sanitize form fields (do NOT overwrite the form)
        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();

            foreach (var field in form)
            {
                // Skip file fields
                if (!form.Files.Any(f => f.Name == field.Key))
                {
                    var sanitizedValues = new StringValues(field.Value.Select(SanitizeInput).ToArray());
                    context.Items[$"Sanitized_{field.Key}"] = sanitizedValues;
                }
                else
                {
                    context.Items[$"Sanitized_{field.Key}"] = field.Value;
                }
            }
        }

        // Sanitize custom headers (optional, only for X- headers)
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.StartsWith("X-", System.StringComparison.OrdinalIgnoreCase))
            {
                context.Request.Headers[header.Key] = new StringValues(
                    header.Value.Select(SanitizeInput).ToArray()
                );
            }
        }

        // Sanitize JSON body
        if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
        {
            context.Request.EnableBuffering(); // يسمح بقراءة الـ Body أكثر من مرة
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                // استخدم Newtonsoft.Json أو System.Text.Json لتحليل الـ JSON
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(body);

                if (dict != null)
                {
                    var sanitized = new Dictionary<string, object>();
                    foreach (var kv in dict)
                    {
                        if (kv.Value is string strVal)
                            sanitized[kv.Key] = SanitizeInput(strVal);
                        else
                            sanitized[kv.Key] = kv.Value;
                    }

                    var sanitizedJson = System.Text.Json.JsonSerializer.Serialize(sanitized);
                    var bytes = Encoding.UTF8.GetBytes(sanitizedJson);
                    context.Request.Body = new MemoryStream(bytes);
                    context.Request.Body.Position = 0;
                }
            }
        }

        await _next(context);
    }

    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Remove <script> tags and content
        input = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Remove all HTML tags
        input = Regex.Replace(input, @"<[^>]+>", "");

        // Remove inline event handlers like onclick="..."
        input = Regex.Replace(input, @"on\w+=""[^""]*""", "", RegexOptions.IgnoreCase);
        input = Regex.Replace(input, @"on\w+='[^']*'", "", RegexOptions.IgnoreCase);

        // Remove javascript:, vbscript:, data: protocols
        input = Regex.Replace(input, @"(javascript|vbscript|data):", "", RegexOptions.IgnoreCase);

        // Encode dangerous characters
        input = input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                     .Replace("\"", "&quot;").Replace("'", "&#x27;").Replace("/", "&#x2F;");

        return input;
    }
}