namespace ContentHub.Infrastructure.Search.OpenSearch;

public static class OpenSearchDocumentMappings
{
    public static string Posts()
    {
        return """
               {
                 "settings": {
                   "index": {
                     "number_of_shards": 1,
                     "number_of_replicas": 0
                   }
                 },
                 "mappings": {
                   "properties": {
                     "id": { "type": "keyword" },
                     "title": { "type": "text", "fields": { "keyword": { "type": "keyword" } } },
                     "slug": { "type": "keyword" },
                     "summary": { "type": "text" },
                     "content": { "type": "text" },
                     "status": { "type": "keyword" },
                     "isFeatured": { "type": "boolean" },
                     "publishedAtUtc": { "type": "date" },
                     "createdAtUtc": { "type": "date" },
                     "coverAssetId": { "type": "keyword" },
                     "categoryIds": { "type": "keyword" },
                     "authorIds": { "type": "keyword" },
                     "tags": { "type": "keyword" },
                     "url": { "type": "keyword" }
                   }
                 }
               }
               """;
    }

    public static string Documents()
    {
        return """
               {
                 "settings": {
                   "index": {
                     "number_of_shards": 1,
                     "number_of_replicas": 0
                   }
                 },
                 "mappings": {
                   "properties": {
                     "id": { "type": "keyword" },
                     "fileName": { "type": "text", "fields": { "keyword": { "type": "keyword" } } },
                     "originalFileName": { "type": "text", "fields": { "keyword": { "type": "keyword" } } },
                     "contentType": { "type": "keyword" },
                     "size": { "type": "long" },
                     "storagePath": { "type": "text" },
                     "url": { "type": "keyword" },
                     "type": { "type": "keyword" },
                     "visibility": { "type": "keyword" },
                     "createdAtUtc": { "type": "date" }
                   }
                 }
               }
               """;
    }
}
