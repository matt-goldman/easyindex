namespace EasyIndex;

/// <summary>
/// Processes table paths to extract text content for indexing
/// This is a mock implementation - in a real scenario, this would connect to a database
/// </summary>
public class TablePathProcessor : IPathProcessor
{
    public PathType SupportedType => PathType.Table;

    public async Task<IEnumerable<IndexedDocument>> ProcessAsync(IndexPath path, CancellationToken cancellationToken = default)
    {
        if (path.Type != SupportedType)
        {
            throw new ArgumentException($"Unsupported path type: {path.Type}");
        }

        var documents = new List<IndexedDocument>();

        // Mock implementation - in reality this would connect to a database
        // and extract text content from specified tables/columns
        await Task.Delay(100, cancellationToken); // Simulate async database operation

        // Parse table path format: "server.database.schema.table" or "connection_string|table_name"
        var tablePath = path.Path;
        var tableName = ExtractTableName(tablePath);

        // Create mock data for demonstration
        var mockRows = GenerateMockTableData(tableName);

        foreach (var row in mockRows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = new IndexedDocument
            {
                Id = Guid.NewGuid().ToString(),
                Content = row["content"].ToString() ?? string.Empty,
                SourcePath = path.Path,
                SourceType = PathType.Table,
                Metadata = new Dictionary<string, object>
                {
                    ["TableName"] = tableName,
                    ["RowId"] = row["id"],
                    ["ProcessedAt"] = DateTime.UtcNow
                }
            };

            // Merge metadata from IndexPath
            foreach (var metadata in path.Metadata)
            {
                document.Metadata[metadata.Key] = metadata.Value;
            }

            documents.Add(document);
        }

        return documents;
    }

    private string ExtractTableName(string tablePath)
    {
        // Simple extraction - in real implementation this would be more sophisticated
        if (tablePath.Contains('.'))
        {
            return tablePath.Split('.').Last();
        }
        if (tablePath.Contains('|'))
        {
            return tablePath.Split('|').Last();
        }
        return tablePath;
    }

    private List<Dictionary<string, object>> GenerateMockTableData(string tableName)
    {
        // Mock data for demonstration
        return new List<Dictionary<string, object>>
        {
            new()
            {
                ["id"] = 1,
                ["content"] = $"Sample content from {tableName} table row 1. This contains searchable text about products and services."
            },
            new()
            {
                ["id"] = 2,
                ["content"] = $"Another entry in {tableName} with different information about customer data and analytics."
            },
            new()
            {
                ["id"] = 3,
                ["content"] = $"Third record in {tableName} containing business intelligence and reporting content."
            }
        };
    }
}