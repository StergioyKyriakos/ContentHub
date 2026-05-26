using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ContentHub.Application.Common.Slugs;

public sealed partial class SlugGenerator : ISlugGenerator
{
    public string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

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