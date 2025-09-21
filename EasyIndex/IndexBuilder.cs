using System.Text.Json;

namespace EasyIndex;

/// <summary>
/// Fluent API for building search indexes with paths and serialization support
/// </summary>
public class IndexBuilder
{
    private readonly List<IndexPath> _paths;
    private readonly SearchIndexEngine _engine;

    private IndexBuilder()
    {
        _paths = new List<IndexPath>();
        _engine = new SearchIndexEngine();
    }

    /// <summary>
    /// Creates a new IndexBuilder instance
    /// </summary>
    /// <returns>A new IndexBuilder instance</returns>
    public static IndexBuilder Create()
    {
        return new IndexBuilder();
    }

    /// <summary>
    /// Loads an index from a JSON file
    /// </summary>
    /// <param name="filePath">Path to the JSON file containing the serialized index</param>
    /// <returns>A SearchIndexEngine with the loaded index</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
    /// <exception cref="JsonException">Thrown when the file contains invalid JSON</exception>
    public static async Task<SearchIndexEngine> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Index file not found: {filePath}");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var indexData = JsonSerializer.Deserialize<IndexData>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        if (indexData?.Documents == null)
        {
            throw new JsonException("Invalid index file format");
        }

        var engine = new SearchIndexEngine();
        
        // Use reflection to access private fields and rebuild the index
        var documentsField = typeof(SearchIndexEngine)
            .GetField("_documents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var buildMethod = typeof(SearchIndexEngine)
            .GetMethod("BuildInvertedIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (documentsField?.GetValue(engine) is List<IndexedDocument> documents)
        {
            documents.Clear(); // Ensure we start with empty documents
            documents.AddRange(indexData.Documents);
            buildMethod?.Invoke(engine, new object[] { indexData.Documents });
        }

        return engine;
    }

    /// <summary>
    /// Adds a single path to be indexed
    /// </summary>
    /// <param name="path">The path to index (file or directory)</param>
    /// <param name="type">The type of path (File or Table)</param>
    /// <returns>The IndexBuilder instance for fluent chaining</returns>
    public IndexBuilder AddPath(string path, PathType type)
    {
        _paths.Add(new IndexPath { Path = path, Type = type });
        return this;
    }

    /// <summary>
    /// Adds multiple paths to be indexed
    /// </summary>
    /// <param name="paths">Collection of IndexPath objects to add</param>
    /// <returns>The IndexBuilder instance for fluent chaining</returns>
    public IndexBuilder AddPaths(IEnumerable<IndexPath> paths)
    {
        _paths.AddRange(paths);
        return this;
    }

    /// <summary>
    /// Adds metadata to the last added path
    /// </summary>
    /// <param name="metadata">Metadata dictionary to add</param>
    /// <returns>The IndexBuilder instance for fluent chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when no paths have been added yet</exception>
    public IndexBuilder WithMetadata(Dictionary<string, object> metadata)
    {
        if (_paths.Count == 0)
        {
            throw new InvalidOperationException("Cannot add metadata without adding a path first");
        }

        var lastPath = _paths[_paths.Count - 1];
        foreach (var kvp in metadata)
        {
            lastPath.Metadata[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    /// Adds a single metadata key-value pair to the last added path
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <param name="value">Metadata value</param>
    /// <returns>The IndexBuilder instance for fluent chaining</returns>
    public IndexBuilder WithMetadata(string key, object value)
    {
        return WithMetadata(new Dictionary<string, object> { [key] = value });
    }

    /// <summary>
    /// Registers a custom path processor with the underlying search engine
    /// </summary>
    /// <param name="processor">The custom processor to register</param>
    /// <returns>The IndexBuilder instance for fluent chaining</returns>
    public IndexBuilder RegisterProcessor(IPathProcessor processor)
    {
        _engine.RegisterProcessor(processor);
        return this;
    }

    /// <summary>
    /// Builds the search index by processing all added paths
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>A SearchIndexEngine with the indexed content</returns>
    public async Task<SearchIndexEngine> BuildAsync(CancellationToken cancellationToken = default)
    {
        if (_paths.Count > 0)
        {
            await _engine.IndexAsync(_paths, cancellationToken);
        }

        return _engine;
    }

    /// <summary>
    /// Saves the built index to a JSON file
    /// </summary>
    /// <param name="filePath">Path where to save the index file</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to save before building</exception>
    public async Task SaveToFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var documents = _engine.GetAllDocuments().ToList();
        if (documents.Count == 0)
        {
            throw new InvalidOperationException("Cannot save an empty index. Call BuildAsync() first or add some paths.");
        }

        var indexData = new IndexData { Documents = documents };
        var json = JsonSerializer.Serialize(indexData, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }
}

/// <summary>
/// Data structure for serializing/deserializing index data
/// </summary>
internal class IndexData
{
    public List<IndexedDocument> Documents { get; set; } = new();
}