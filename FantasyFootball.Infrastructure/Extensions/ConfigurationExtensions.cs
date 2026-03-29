using Microsoft.Extensions.Configuration;

namespace FantasyFootball.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static Uri GetRequiredBaseUrl(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Configuration key '{key}' is missing or empty.");

        value = value.Trim();

        if (!value.EndsWith('/'))
            value += "/";

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw new InvalidOperationException($"Configuration key '{key}' is not a valid absolute URI: '{value}'");

        return uri;
    }
}
