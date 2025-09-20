# EasyIndex

A simple .NET algorithm for creating full-text indexes across any provided text data.

## Features

- **Extensible Architecture**: Plugin-based design using interfaces for different data sources
- **Multiple Data Sources**: Built-in support for files and tables (mock implementation)
- **Full-Text Search**: TF-IDF based scoring with inverted index for fast searching
- **In-Memory Storage**: Fast, lightweight indexing for immediate use
- **Rich Metadata**: Automatic extraction and preservation of document metadata
- **Async Support**: Non-blocking operations for large datasets

## Quick Start

### Basic Usage

```csharp
using EasyIndex;

// Create search engine
var searchEngine = new SearchIndexEngine();

// Define paths to index
var paths = new[]
{
    new IndexPath { Path = "/path/to/documents", Type = PathType.File },
    new IndexPath { Path = "server.database.table", Type = PathType.Table }
};

// Index the content
await searchEngine.IndexAsync(paths);

// Search the index
var results = searchEngine.Search("machine learning", maxResults: 10);

foreach (var result in results)
{
    Console.WriteLine($"Score: {result.Score:F2}");
    Console.WriteLine($"Source: {result.Document.SourcePath}");
    Console.WriteLine($"Content: {result.Document.Content.Substring(0, 100)}...");
}
```

### File Processing

The library supports processing individual files or entire directories:

```csharp
// Single file
new IndexPath { Path = "/path/to/document.txt", Type = PathType.File }

// Directory (recursive)
new IndexPath { Path = "/path/to/documents/", Type = PathType.File }
```

Supported file types: `.txt`, `.md`, `.csv`, `.json`, `.xml`, `.log`

### Table Processing

Tables can be referenced using various path formats:

```csharp
// Standard database path
new IndexPath { Path = "server.database.schema.table", Type = PathType.Table }

// Connection string format
new IndexPath { Path = "connection_string|table_name", Type = PathType.Table }
```

*Note: The current table processor is a mock implementation. In production, this would connect to actual databases.*

### Custom Metadata

Add custom metadata to paths that will be preserved in indexed documents:

```csharp
var path = new IndexPath 
{ 
    Path = "/documents", 
    Type = PathType.File,
    Metadata = new Dictionary<string, object>
    {
        ["Department"] = "Engineering",
        ["Priority"] = "High",
        ["IndexedDate"] = DateTime.UtcNow
    }
};
```

## Architecture

### Core Components

- **`SearchIndexEngine`**: Main orchestrator for indexing and searching
- **`IPathProcessor`**: Interface for processing different data source types
- **`FilePathProcessor`**: Implementation for file system sources
- **`TablePathProcessor`**: Mock implementation for database tables
- **`IndexedDocument`**: Represents a document in the search index
- **`SearchResult`**: Represents a search result with scoring

### Extensibility

Create custom processors by implementing `IPathProcessor`:

```csharp
public class CustomPathProcessor : IPathProcessor
{
    public PathType SupportedType => PathType.Custom; // Add new enum value

    public async Task<IEnumerable<IndexedDocument>> ProcessAsync(
        IndexPath path, 
        CancellationToken cancellationToken = default)
    {
        // Your custom implementation
        return documents;
    }
}

// Register with engine
searchEngine.RegisterProcessor(new CustomPathProcessor());
```

## Search Algorithm

The library uses a simplified TF-IDF (Term Frequency-Inverse Document Frequency) algorithm:

1. **Tokenization**: Text is split into terms using whitespace and punctuation
2. **Inverted Index**: Maps terms to documents containing them
3. **Scoring**: Combines term frequency in document with inverse document frequency
4. **Ranking**: Results sorted by relevance score

## API Reference

### Classes

#### `SearchIndexEngine`
- `IndexAsync(IEnumerable<IndexPath> paths)`: Index content from specified paths
- `Search(string query, int maxResults = 10)`: Search indexed content
- `GetAllDocuments()`: Retrieve all indexed documents
- `ClearIndex()`: Remove all indexed content
- `RegisterProcessor(IPathProcessor processor)`: Add custom processor

#### `IndexPath`
- `Path`: Source path or identifier
- `Type`: Type of path (File, Table, etc.)
- `Metadata`: Additional metadata to attach

#### `IndexedDocument`
- `Id`: Unique document identifier
- `Content`: Full text content
- `SourcePath`: Original source location
- `SourceType`: Type of source
- `Metadata`: Document metadata

#### `SearchResult`
- `Document`: The matched document
- `Score`: Relevance score
- `MatchedTerms`: Terms that matched the query

## Building and Testing

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run demo
cd EasyIndex.Demo
dotnet run
```

## Requirements

- .NET 8.0 or later
- No external dependencies for core functionality

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.