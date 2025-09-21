namespace EasyIndex.Tests;

public class SearchIndexEngineTests
{
    private readonly SearchIndexEngine _engine;

    public SearchIndexEngineTests()
    {
        _engine = new SearchIndexEngine();
    }

    [Fact]
    public async Task IndexAsync_WithTablePath_ShouldIndexDocuments()
    {
        // Arrange
        var paths = new[]
        {
            new IndexPath { Path = "test.table", Type = PathType.Table }
        };

        // Act
        await _engine.IndexAsync(paths);
        var documents = _engine.GetAllDocuments().ToList();

        // Assert
        Assert.NotEmpty(documents);
        Assert.All(documents, doc => Assert.Equal(PathType.Table, doc.SourceType));
    }

    [Fact]
    public void Search_WithMatchingQuery_ShouldReturnResults()
    {
        // Arrange
        var documents = new[]
        {
            new IndexedDocument
            {
                Id = "1",
                Content = "This is a test document about machine learning",
                SourcePath = "test.txt",
                SourceType = PathType.File
            },
            new IndexedDocument
            {
                Id = "2",
                Content = "Another document discussing artificial intelligence",
                SourcePath = "test2.txt",
                SourceType = PathType.File
            }
        };

        // Manually add documents for testing
        foreach (var doc in documents)
        {
            typeof(SearchIndexEngine)
                .GetField("_documents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_engine)
                ?.GetType()
                .GetMethod("Add")?
                .Invoke(typeof(SearchIndexEngine)
                    .GetField("_documents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .GetValue(_engine), new[] { doc });
        }

        // Build inverted index manually for testing
        var buildMethod = typeof(SearchIndexEngine)
            .GetMethod("BuildInvertedIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        buildMethod?.Invoke(_engine, new object[] { documents });

        // Act
        var results = _engine.Search("machine learning").ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Document.Id == "1");
    }

    [Fact]
    public void Search_WithEmptyQuery_ShouldReturnEmptyResults()
    {
        // Act
        var results = _engine.Search("").ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ClearIndex_ShouldRemoveAllDocuments()
    {
        // Arrange - Add a document first
        var doc = new IndexedDocument
        {
            Id = "1",
            Content = "Test content",
            SourcePath = "test.txt",
            SourceType = PathType.File
        };

        typeof(SearchIndexEngine)
            .GetField("_documents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .GetValue(_engine)
            ?.GetType()
            .GetMethod("Add")?
            .Invoke(typeof(SearchIndexEngine)
                .GetField("_documents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_engine), new[] { doc });

        // Act
        _engine.ClearIndex();

        // Assert
        Assert.Empty(_engine.GetAllDocuments());
    }
}