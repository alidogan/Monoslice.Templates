using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Shared.Persistence;

/// <summary>Converts concurrency tokens to HTTP ETags and applies the version a client expects.</summary>
public static class ConcurrencyToken
{
    public static string ToETag(IHasConcurrencyToken entity) => Format(entity.Version);

    /// <summary>
    /// Returns <c>true</c> when <paramref name="eTag"/> matches the entity's current version, and makes EF Core
    /// verify that version when saving (a concurrent update then fails with a <see cref="DbUpdateConcurrencyException"/>).
    /// </summary>
    public static bool TryApplyExpectedVersion<TEntity>(this DbContext dbContext, TEntity entity, string? eTag)
        where TEntity : class, IHasConcurrencyToken
    {
        if (!TryParse(eTag, out var expected) || !Matches(entity.Version, expected))
        {
            return false;
        }

        dbContext.Entry(entity).Property(nameof(IHasConcurrencyToken.Version)).OriginalValue = expected;
        return true;
    }

    private static string Normalize(string eTag)
    {
        var value = eTag.Trim();
        if (value.StartsWith("W/", StringComparison.Ordinal))
        {
            value = value[2..];
        }

        return value.Trim('"');
    }
#if (UsePostgreSQL)

    private static string Format(uint version) =>
        string.Create(CultureInfo.InvariantCulture, $"\"{version}\"");

    private static bool TryParse(string? eTag, out uint version)
    {
        version = 0;
        return !string.IsNullOrWhiteSpace(eTag)
            && uint.TryParse(Normalize(eTag), NumberStyles.None, CultureInfo.InvariantCulture, out version);
    }

    private static bool Matches(uint current, uint expected) => current == expected;
#else

    private static string Format(byte[] version) =>
        string.Create(CultureInfo.InvariantCulture, $"\"{Convert.ToBase64String(version)}\"");

    private static bool TryParse(string? eTag, out byte[] version)
    {
        version = [];
        if (string.IsNullOrWhiteSpace(eTag))
        {
            return false;
        }

        var normalized = Normalize(eTag);
        var buffer = new byte[normalized.Length];
        if (!Convert.TryFromBase64String(normalized, buffer, out var written))
        {
            return false;
        }

        version = buffer[..written];
        return true;
    }

    private static bool Matches(byte[] current, byte[] expected) => current.AsSpan().SequenceEqual(expected);
#endif
}
