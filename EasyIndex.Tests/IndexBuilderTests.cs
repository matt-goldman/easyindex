namespace EasyIndex.Tests;

public class IndexBuilderTests : IDisposable
{
    private readonly string _tempDirectory;

    public IndexBuilderTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void Create_ShouldReturnNewIndexBuilderInstance()
    {
        // Act
        var builder = IndexBuilder.Create();

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public async Task BuildAsync_WithNoPaths_ShouldReturnEmptyEngine()
    {
        // Arrange
        var builder = IndexBuilder.Create();

        // Act
        var engine = await builder.BuildAsync();

        // Assert
        Assert.NotNull(engine);
        Assert.Empty(engine.GetAllDocuments());
    }

    [Fact]
    public async Task AddPath_AndBuildAsync_ShouldIndexContent()
    {
        // Arrange
        var testFile = Path.Combine(_tempDirectory, "test.txt");
        await File.WriteAllTextAsync(testFile, "Test content for indexing");

        var builder = IndexBuilder.Create();

        // Act
        var engine = await builder
            .AddPath(testFile, PathType.File)
            .BuildAsync();

        // Assert
        var documents = engine.GetAllDocuments().ToList();
        Assert.Single(documents);
        Assert.Equal("Test content for indexing", documents[0].Content);
        Assert.Equal(testFile, documents[0].SourcePath);
    }

    [Fact]
    public async Task AddPaths_ShouldIndexMultiplePaths()
    {
        // Arrange
        var file1 = Path.Combine(_tempDirectory, "file1.txt");
        var file2 = Path.Combine(_tempDirectory, "file2.txt");
        
        await File.WriteAllTextAsync(file1, "First file content");
        await File.WriteAllTextAsync(file2, "Second file content");

        var paths = new[]
        {
            new IndexPath { Path = file1, Type = PathType.File },
            new IndexPath { Path = file2, Type = PathType.File }
        };

        var builder = IndexBuilder.Create();

        // Act
        var engine = await builder
            .AddPaths(paths)
            .BuildAsync();

        // Assert
        var documents = engine.GetAllDocuments().ToList();
        Assert.Equal(2, documents.Count);
        Assert.Contains(documents, d => d.Content == "First file content");
        Assert.Contains(documents, d => d.Content == "Second file content");
    }

    [Fact]
    public async Task WithMetadata_ShouldAddMetadataToLastPath()
    {
        // Arrange
        var testFile = Path.Combine(_tempDirectory, "test.txt");
        await File.WriteAllTextAsync(testFile, "Test content");

        var metadata = new Dictionary<string, object>
        {
            ["Department"] = "Engineering",
            ["Priority"] = "High"
        };

        var builder = IndexBuilder.Create();

        // Act
        var engine = await builder
            .AddPath(testFile, PathType.File)
            .WithMetadata(metadata)
            .BuildAsync();

        // Assert
        var document = engine.GetAllDocuments().Single();
        Assert.Equal("Engineering", document.Metadata["Department"]);
        Assert.Equal("High", document.Metadata["Priority"]);
    }

    [Fact]
    public async Task WithMetadata_KeyValue_ShouldAddSingleMetadata()
    {
        // Arrange
        var testFile = Path.Combine(_tempDirectory, "test.txt");
        await File.WriteAllTextAsync(testFile, "Test content");

        var builder = IndexBuilder.Create();

        // Act
        var engine = await builder
            .AddPath(testFile, PathType.File)
            .WithMetadata("Source", "TestData")
            .BuildAsync();

        // Assert
        var document = engine.GetAllDocuments().Single();
        Assert.Equal("TestData", document.Metadata["Source"]);
    }

    [Fact]
    public void WithMetadata_WithoutAddingPath_ShouldThrowException()
    {
        // Arrange
        var builder = IndexBuilder.Create();
        var metadata = new Dictionary<string, object> { ["Key"] = "Value" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.WithMetadata(metadata));
    }

    [Fact]
    public async Task SaveToFileAsync_AndLoadFromFileAsync_ShouldPreserveIndex()
    {
        // Arrange
        var testFile = Path.Combine(_tempDirectory, "test.txt");
        var indexFile = Path.Combine(_tempDirectory, "index.json");
        
        await File.WriteAllTextAsync(testFile, "Test content for serialization");

        // Act - Build and Save
        var builder = IndexBuilder.Create();
        var originalEngine = await builder
            .AddPath(testFile, PathType.File)
            .WithMetadata("TestKey", "TestValue")
            .BuildAsync();
        
        await builder.SaveToFileAsync(indexFile);

        // Act - Load
        var loadedEngine = await IndexBuilder.LoadFromFileAsync(indexFile);

        // Assert
        var originalDocs = originalEngine.GetAllDocuments().ToList();
        var loadedDocs = loadedEngine.GetAllDocuments().ToList();

        Assert.Equal(originalDocs.Count, loadedDocs.Count);
        
        var originalDoc = originalDocs.Single();
        var loadedDoc = loadedDocs.Single();
        
        Assert.Equal(originalDoc.Content, loadedDoc.Content);
        Assert.Equal(originalDoc.SourcePath, loadedDoc.SourcePath);
        Assert.Equal(originalDoc.SourceType, loadedDoc.SourceType);
        
        // Handle JsonElement conversion for metadata comparison
        var originalTestKey = originalDoc.Metadata["TestKey"];
        var loadedTestKey = loadedDoc.Metadata["TestKey"];
        
        if (loadedTestKey is System.Text.Json.JsonElement jsonElement)
        {
            Assert.Equal(originalTestKey.ToString(), jsonElement.GetString());
        }
        else
        {
            Assert.Equal(originalTestKey, loadedTestKey);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(_tempDirectory, "nonexistent.json");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            IndexBuilder.LoadFromFileAsync(nonExistentFile));
    }

    [Fact]
    public async Task LoadFromFileAsync_WithInvalidJson_ShouldThrowJsonException()
    {
        // Arrange
        var invalidJsonFile = Path.Combine(_tempDirectory, "invalid.json");
        await File.WriteAllTextAsync(invalidJsonFile, "{ invalid json }");

        // Act & Assert
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => 
            IndexBuilder.LoadFromFileAsync(invalidJsonFile));
    }

    [Fact]
    public async Task SaveToFileAsync_WithEmptyIndex_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = IndexBuilder.Create();
        var indexFile = Path.Combine(_tempDirectory, "empty.json");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            builder.SaveToFileAsync(indexFile));
    }

    [Fact]
    public async Task RegisterProcessor_ShouldAllowCustomProcessors()
    {
        // Arrange
        var customProcessor = new MockCustomProcessor();
        var builder = IndexBuilder.Create();

        // Act
        var engine = await builder
            .RegisterProcessor(customProcessor)
            .AddPath("custom://test", (PathType)999) // Using a custom path type
            .BuildAsync();

        // Assert
        var documents = engine.GetAllDocuments().ToList();
        Assert.Single(documents);
        Assert.Equal("Custom processed content", documents[0].Content);
    }

    [Fact]
    public async Task FluentChaining_ShouldWorkWithComplexScenario()
    {
        // Arrange
        var file1 = Path.Combine(_tempDirectory, "doc1.txt");
        var file2 = Path.Combine(_tempDirectory, "doc2.txt");
        
        await File.WriteAllTextAsync(file1, "Document one content");
        await File.WriteAllTextAsync(file2, "Document two content");

        var additionalPaths = new[]
        {
            new IndexPath { Path = "table.test", Type = PathType.Table }
        };

        // Act
        var engine = await IndexBuilder.Create()
            .AddPath(file1, PathType.File)
            .WithMetadata("Category", "Important")
            .AddPath(file2, PathType.File)
            .WithMetadata("Category", "Archive")
            .WithMetadata("Author", "TestUser")
            .AddPaths(additionalPaths)
            .BuildAsync();

        // Assert
        var documents = engine.GetAllDocuments().ToList();
        Assert.Equal(5, documents.Count); // 2 files + 3 table docs (mock data) = 5 total

        var doc1 = documents.First(d => d.Content == "Document one content");
        Assert.Equal("Important", doc1.Metadata["Category"]);

        var doc2 = documents.First(d => d.Content == "Document two content");
        Assert.Equal("Archive", doc2.Metadata["Category"]);
        Assert.Equal("TestUser", doc2.Metadata["Author"]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    // Mock processor for testing custom processors
    private class MockCustomProcessor : IPathProcessor
    {
        public PathType SupportedType => (PathType)999;

        public Task<IEnumerable<IndexedDocument>> ProcessAsync(IndexPath path, CancellationToken cancellationToken = default)
        {
            var document = new IndexedDocument
            {
                Id = Guid.NewGuid().ToString(),
                Content = "Custom processed content",
                SourcePath = path.Path,
                SourceType = SupportedType
            };

            return Task.FromResult<IEnumerable<IndexedDocument>>(new[] { document });
        }
    }
}