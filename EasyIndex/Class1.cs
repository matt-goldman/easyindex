namespace EasyIndex;

/// <summary>
/// Represents the type of path being processed
/// </summary>
public enum PathType
{
    File,
    Table
}

/// <summary>
/// Represents a path with its type
/// </summary>
public class IndexPath
{
    public string Path { get; set; } = string.Empty;
    public PathType Type { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a document in the search index
/// </summary>
public class IndexedDocument
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public PathType SourceType { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a search result
/// </summary>
public class SearchResult
{
    public IndexedDocument Document { get; set; } = new();
    public double Score { get; set; }
    public List<string> MatchedTerms { get; set; } = new();
}

/// <summary>
/// Interface for processing different types of paths to extract text content
/// </summary>
public interface IPathProcessor
{
    PathType SupportedType { get; }
    Task<IEnumerable<IndexedDocument>> ProcessAsync(IndexPath path, CancellationToken cancellationToken = default);
}
