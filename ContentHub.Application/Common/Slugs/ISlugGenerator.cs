namespace ContentHub.Application.Common.Slugs;

public interface ISlugGenerator
{
    string Generate(string value);
}