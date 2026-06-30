using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Data.Entities.Assets;
using ContentHub.Data.Entities.Posts;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.Search.OpenSearch;

public sealed class OpenSearchIndex
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ContentHubDbContext _db;
    private readonly IFileUrlResolver _fileUrlResolver;
    private readonly IOptionsMonitor<OpenSearchOptions> _options;
    private readonly ILogger<OpenSearchIndex> _logger;

    public OpenSearchIndex(
        ContentHubDbContext db,
        IFileUrlResolver fileUrlResolver,
        IOptionsMonitor<OpenSearchOptions> options,
        ILogger<OpenSearchIndex> logger)
    {
        _db = db;
        _fileUrlResolver = fileUrlResolver;
        _options = options;
        _logger = logger;
    }

    public bool IsOpenSearchEnabled()
    {
        return _options.CurrentValue.UseOpenSearch();
    }

    public async Task<OpenSearchReindexResult> ReindexAsync(CancellationToken ct = default)
    {
        var options = _options.CurrentValue;

        if (!options.UseOpenSearch())
        {
            return new OpenSearchReindexResult
            {
                Provider = options.Provider,
                OpenSearchEnabled = false,
                Message = "Search provider is not OpenSearch, so reindexing was skipped."
            };
        }

        await EnsureIndexesAsync(ct);

        var posts = await GetPostDocumentsAsync(ct);
        var documents = await GetDocumentDocumentsAsync(ct);

        await BulkIndexAsync(options.PostsIndexName(), posts, ct);
        await BulkIndexAsync(options.DocumentsIndexName(), documents, ct);

        return new OpenSearchReindexResult
        {
            Provider = options.Provider,
            OpenSearchEnabled = true,
            PostsIndexed = posts.Count,
            DocumentsIndexed = documents.Count,
            Message = "OpenSearch indexes were rebuilt successfully."
        };
    }

    public async Task<OpenSearchSearchResult<OpenSearchPostDocument>> SearchPostsAsync(
        OpenSearchPostSearchRequest request,
        CancellationToken ct = default)
    {
        var options = _options.CurrentValue;
        var body = BuildPostSearchBody(request);
        var json = await PostJsonAsync($"{options.PostsIndexName()}/_search", body, ct);

        return ReadSearchResult(
            json,
            source => JsonSerializer.Deserialize<OpenSearchPostDocument>(source.GetRawText(), JsonOptions)!);
    }

    public async Task<OpenSearchSearchResult<OpenSearchDocumentDocument>> SearchDocumentsAsync(
        OpenSearchDocumentSearchRequest request,
        CancellationToken ct = default)
    {
        var options = _options.CurrentValue;
        var body = BuildDocumentSearchBody(request);
        var json = await PostJsonAsync($"{options.DocumentsIndexName()}/_search", body, ct);

        return ReadSearchResult(
            json,
            source => JsonSerializer.Deserialize<OpenSearchDocumentDocument>(source.GetRawText(), JsonOptions)!);
    }

    private async Task EnsureIndexesAsync(CancellationToken ct)
    {
        var options = _options.CurrentValue;

        await EnsureIndexAsync(
            options.PostsIndexName(),
            OpenSearchDocumentMappings.Posts(),
            ct);

        await EnsureIndexAsync(
            options.DocumentsIndexName(),
            OpenSearchDocumentMappings.Documents(),
            ct);
    }

    private async Task EnsureIndexAsync(
        string indexName,
        string mappingJson,
        CancellationToken ct)
    {
        using var client = CreateClient();
        using var headRequest = new HttpRequestMessage(
            HttpMethod.Head,
            CreateUri(indexName));

        using var headResponse = await client.SendAsync(headRequest, ct);

        if (headResponse.IsSuccessStatusCode)
        {
            return;
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            CreateUri(indexName))
        {
            Content = new StringContent(mappingJson, Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(request, ct);
        await EnsureSuccessAsync(response, $"OpenSearch index '{indexName}' could not be created.");
    }

    private async Task<List<OpenSearchPostDocument>> GetPostDocumentsAsync(CancellationToken ct)
    {
        var posts = await _db.Posts
            .AsNoTracking()
            .Where(post => post.Status == PostStatus.Published)
            .Select(post => new
            {
                post.Id,
                post.Title,
                post.Slug,
                post.Summary,
                post.Content,
                post.Status,
                post.IsFeatured,
                post.PublishedAtUtc,
                post.CreatedAtUtc,
                post.CoverAssetId,
                CategoryIds = post.Categories.Select(category => category.CategoryId).ToList(),
                AuthorIds = post.Authors.Select(author => author.AuthorId).ToList(),
                Tags = post.Tags.Select(tag => tag.Name).ToList()
            })
            .ToListAsync(ct);

        return posts
            .Select(post => new OpenSearchPostDocument
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                Content = post.Content,
                Status = post.Status,
                IsFeatured = post.IsFeatured,
                PublishedAtUtc = post.PublishedAtUtc,
                CreatedAtUtc = post.CreatedAtUtc,
                CoverAssetId = post.CoverAssetId,
                CategoryIds = post.CategoryIds,
                AuthorIds = post.AuthorIds,
                Tags = post.Tags,
                Url = $"/api/public/posts/{post.Slug}"
            })
            .ToList();
    }

    private async Task<List<OpenSearchDocumentDocument>> GetDocumentDocumentsAsync(CancellationToken ct)
    {
        var assets = await _db.Assets
            .AsNoTracking()
            .Where(asset => asset.Type == AssetType.Document)
            .Select(asset => new
            {
                asset.Id,
                asset.FileName,
                asset.OriginalFileName,
                asset.ContentType,
                asset.Size,
                asset.StoragePath,
                asset.Provider,
                asset.Type,
                asset.Visibility,
                asset.CreatedAtUtc
            })
            .ToListAsync(ct);

        return assets
            .Select(asset => new OpenSearchDocumentDocument
            {
                Id = asset.Id,
                FileName = asset.FileName,
                OriginalFileName = asset.OriginalFileName,
                ContentType = asset.ContentType,
                Size = asset.Size,
                StoragePath = asset.StoragePath,
                Url = _fileUrlResolver.ResolveUrl(asset.StoragePath, asset.Provider),
                Type = asset.Type,
                Visibility = asset.Visibility,
                CreatedAtUtc = asset.CreatedAtUtc
            })
            .ToList();
    }

    private async Task BulkIndexAsync<T>(
        string indexName,
        IReadOnlyCollection<T> documents,
        CancellationToken ct)
        where T : class
    {
        if (documents.Count == 0)
        {
            return;
        }

        var builder = new StringBuilder();

        foreach (var document in documents)
        {
            var id = document switch
            {
                OpenSearchPostDocument post => post.Id,
                OpenSearchDocumentDocument asset => asset.Id,
                _ => throw new InvalidOperationException("OpenSearch document type is not supported.")
            };

            builder.Append(JsonSerializer.Serialize(new
            {
                index = new
                {
                    _index = indexName,
                    _id = id.ToString()
                }
            }, JsonOptions));
            builder.Append('\n');
            builder.Append(JsonSerializer.Serialize(document, JsonOptions));
            builder.Append('\n');
        }

        var responseJson = await PostRawAsync("_bulk", builder.ToString(), "application/x-ndjson", ct);

        using var documentJson = JsonDocument.Parse(responseJson);
        if (documentJson.RootElement.TryGetProperty("errors", out var errors) && errors.GetBoolean())
        {
            _logger.LogWarning("OpenSearch bulk index returned errors: {Response}", responseJson);
            throw new InvalidOperationException("OpenSearch bulk indexing failed.");
        }
    }

    private static Dictionary<string, object?> BuildPostSearchBody(OpenSearchPostSearchRequest request)
    {
        var filters = new List<object>();

        if (!request.IncludeUnpublished)
        {
            filters.Add(Term("status", PostStatus.Published.ToString()));
        }
        else if (request.Status.HasValue)
        {
            filters.Add(Term("status", request.Status.Value.ToString()));
        }

        if (request.CategoryId.HasValue)
        {
            filters.Add(Term("categoryIds", request.CategoryId.Value.ToString()));
        }

        if (request.AuthorId.HasValue)
        {
            filters.Add(Term("authorIds", request.AuthorId.Value.ToString()));
        }

        if (request.IsFeatured.HasValue)
        {
            filters.Add(Term("isFeatured", request.IsFeatured.Value));
        }

        if (request.PublishedFrom.HasValue || request.PublishedTo.HasValue)
        {
            var range = new Dictionary<string, object>();

            if (request.PublishedFrom.HasValue)
            {
                range["gte"] = request.PublishedFrom.Value;
            }

            if (request.PublishedTo.HasValue)
            {
                range["lte"] = request.PublishedTo.Value;
            }

            filters.Add(new
            {
                range = new Dictionary<string, object>
                {
                    ["publishedAtUtc"] = range
                }
            });
        }

        var body = BuildBaseSearchBody(
            request.Query,
            request.Page,
            request.PageSize,
            ["title^4", "summary^2", "content", "tags"],
            ["title", "summary", "content"],
            filters);

        AddPostSort(body, request);

        return body;
    }

    private static Dictionary<string, object?> BuildDocumentSearchBody(OpenSearchDocumentSearchRequest request)
    {
        var filters = new List<object>
        {
            Term("type", AssetType.Document.ToString())
        };

        if (request.Visibility.HasValue)
        {
            filters.Add(Term("visibility", request.Visibility.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(request.ContentType))
        {
            filters.Add(new
            {
                wildcard = new Dictionary<string, object>
                {
                    ["contentType"] = new
                    {
                        value = $"*{request.ContentType.Trim()}*",
                        case_insensitive = true
                    }
                }
            });
        }

        var body = BuildBaseSearchBody(
            request.Query,
            request.Page,
            request.PageSize,
            ["originalFileName^3", "fileName^2", "storagePath"],
            ["originalFileName", "fileName", "storagePath"],
            filters);

        AddDocumentSort(body, request);

        return body;
    }

    private static Dictionary<string, object?> BuildBaseSearchBody(
        string? query,
        int page,
        int pageSize,
        string[] searchFields,
        string[] highlightFields,
        IReadOnlyCollection<object> filters)
    {
        var must = new List<object>();
        var searchText = query?.Trim();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            must.Add(new
            {
                match_all = new { }
            });
        }
        else
        {
            must.Add(new
            {
                multi_match = new
                {
                    query = searchText,
                    fields = searchFields,
                    fuzziness = "AUTO"
                }
            });
        }

        return new Dictionary<string, object?>
        {
            ["from"] = (Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100),
            ["size"] = Math.Clamp(pageSize, 1, 100),
            ["query"] = new
            {
                @bool = new
                {
                    must,
                    filter = filters
                }
            },
            ["highlight"] = new
            {
                pre_tags = new[] { "<mark>" },
                post_tags = new[] { "</mark>" },
                fields = highlightFields.ToDictionary(
                    field => field,
                    _ => new { } as object)
            }
        };
    }

    private static void AddPostSort(
        Dictionary<string, object?> body,
        OpenSearchPostSearchRequest request)
    {
        if (IsRelevanceSort(request.SortBy) && !string.IsNullOrWhiteSpace(request.Query))
        {
            return;
        }

        var field = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => "title.keyword",
            "createdat" => "createdAtUtc",
            "isfeatured" => "isFeatured",
            _ => "publishedAtUtc"
        };

        body["sort"] = Sort(field, request.SortDirection);
    }

    private static void AddDocumentSort(
        Dictionary<string, object?> body,
        OpenSearchDocumentSearchRequest request)
    {
        if (IsRelevanceSort(request.SortBy) && !string.IsNullOrWhiteSpace(request.Query))
        {
            return;
        }

        var field = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "filename" => "fileName.keyword",
            "originalfilename" => "originalFileName.keyword",
            "size" => "size",
            "type" => "type",
            _ => "createdAtUtc"
        };

        body["sort"] = Sort(field, request.SortDirection);
    }

    private static object[] Sort(
        string field,
        string? sortDirection)
    {
        var direction = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";

        return
        [
            new Dictionary<string, object>
            {
                [field] = new
                {
                    order = direction,
                    missing = "_last"
                }
            }
        ];
    }

    private static bool IsRelevanceSort(string? sortField)
    {
        return string.IsNullOrWhiteSpace(sortField) ||
               string.Equals(sortField, "relevance", StringComparison.OrdinalIgnoreCase);
    }

    private static object Term(string field, object value)
    {
        return new
        {
            term = new Dictionary<string, object>
            {
                [field] = value
            }
        };
    }

    private async Task<string> PostJsonAsync(
        string path,
        object body,
        CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);

        return await PostRawAsync(path, json, "application/json", ct);
    }

    private async Task<string> PostRawAsync(
        string path,
        string body,
        string contentType,
        CancellationToken ct)
    {
        using var client = CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            CreateUri(path))
        {
            Content = new StringContent(body, Encoding.UTF8, contentType)
        };

        using var response = await client.SendAsync(request, ct);
        await EnsureSuccessAsync(response, "OpenSearch request failed.");

        return await response.Content.ReadAsStringAsync(ct);
    }

    private HttpClient CreateClient()
    {
        var options = _options.CurrentValue;
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(options.RequestTimeoutSeconds, 1))
        };

        if (!string.IsNullOrWhiteSpace(options.Username))
        {
            var token = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{options.Username}:{options.Password}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        }

        return client;
    }

    private Uri CreateUri(string path)
    {
        var endpoint = _options.CurrentValue.Endpoint.TrimEnd('/') + "/";

        return new Uri(new Uri(endpoint), path.TrimStart('/'));
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string message)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();

        throw new InvalidOperationException($"{message} Status {(int)response.StatusCode}. {body}");
    }

    private static OpenSearchSearchResult<T> ReadSearchResult<T>(
        string json,
        Func<JsonElement, T> mapSource)
    {
        using var document = JsonDocument.Parse(json);
        var hits = document.RootElement
            .GetProperty("hits");

        var totalCount = ReadTotalCount(hits);
        var items = new List<OpenSearchSearchHit<T>>();

        foreach (var hit in hits.GetProperty("hits").EnumerateArray())
        {
            var source = hit.GetProperty("_source");

            items.Add(new OpenSearchSearchHit<T>
            {
                Document = mapSource(source),
                Highlights = ReadHighlights(hit)
            });
        }

        return new OpenSearchSearchResult<T>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private static int ReadTotalCount(JsonElement hits)
    {
        var total = hits.GetProperty("total");

        if (total.ValueKind == JsonValueKind.Number)
        {
            return total.GetInt32();
        }

        return total.GetProperty("value").GetInt32();
    }

    private static IReadOnlyCollection<string> ReadHighlights(JsonElement hit)
    {
        if (!hit.TryGetProperty("highlight", out var highlight))
        {
            return [];
        }

        var highlights = new List<string>();

        foreach (var field in highlight.EnumerateObject())
        {
            foreach (var value in field.Value.EnumerateArray())
            {
                highlights.Add(value.GetString() ?? string.Empty);
            }
        }

        return highlights;
    }
}
