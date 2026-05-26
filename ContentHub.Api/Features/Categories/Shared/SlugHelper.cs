using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ContentHub.Api.Features.Categories.Shared;

public static partial class SlugHelper
{
    public static string GenerateSlug(string value)
    {
        var normalized = value
            .Trim()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);

            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var slug = builder
            .ToString()
            .Normalize(NormalizationForm.FormC);

        slug = InvalidCharactersRegex().Replace(slug, "-");
        slug = MultipleDashesRegex().Replace(slug, "-");

        return slug.Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex InvalidCharactersRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleDashesRegex();
}