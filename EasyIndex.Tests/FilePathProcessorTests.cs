namespace EasyIndex.Tests;

public class FilePathProcessorTests : IDisposable
{
    private readonly FilePathProcessor _processor;
    private readonly string _tempDirectory;

    public FilePathProcessorTests()
    {
        _processor = new FilePathProcessor();
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void SupportedType_ShouldReturnFile()
    {
        Assert.Equal(PathType.File, _processor.SupportedType);
    }

    [Fact]
    public async Task ProcessAsync_WithSingleFile_ShouldReturnDocument()
    {
        // Arrange
        var testFile = Path.Combine(_tempDirectory, "test.txt");
        var content = "This is test content for indexing";
        await File.WriteAllTextAsync(testFile, content);

        var indexPath = new IndexPath { Path = testFile, Type = PathType.File };

        // Act
        var documents = await _processor.ProcessAsync(indexPath);

        // Assert
        var document = Assert.Single(documents);
        Assert.Equal(content, document.Content);
        Assert.Equal(testFile, document.SourcePath);
        Assert.Equal(PathType.File, document.SourceType);
        Assert.Contains("FileName", document.Metadata.Keys);
        Assert.Equal("test.txt", document.Metadata["FileName"]);
    }

    [Fact]
    public async Task ProcessAsync_WithDirectory_ShouldReturnMultipleDocuments()
    {
        // Arrange
        var file1 = Path.Combine(_tempDirectory, "test1.txt");
        var file2 = Path.Combine(_tempDirectory, "test2.md");
        
        await File.WriteAllTextAsync(file1, "Content of file 1");
        await File.WriteAllTextAsync(file2, "Content of file 2");

        var indexPath = new IndexPath { Path = _tempDirectory, Type = PathType.File };

        // Act
        var documents = (await _processor.ProcessAsync(indexPath)).ToList();

        // Assert
        Assert.Equal(2, documents.Count);
        Assert.All(documents, doc => Assert.Equal(PathType.File, doc.SourceType));
    }

    [Fact]
    public async Task ProcessAsync_WithWrongPathType_ShouldThrowException()
    {
        // Arrange
        var indexPath = new IndexPath { Path = "test", Type = PathType.Table };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _processor.ProcessAsync(indexPath));
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentPath_ShouldThrowException()
    {
        // Arrange
        var indexPath = new IndexPath { Path = "non-existent-file.txt", Type = PathType.File };

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _processor.ProcessAsync(indexPath));
    }

    [Fact]
    public async Task ProcessAsync_WithMetadata_ShouldMergeMetadata()
    {
        // Arrange
        var testFile = Path.Combine(_tempDirectory, "test.txt");
        await File.WriteAllTextAsync(testFile, "Test content");

        var indexPath = new IndexPath 
        { 
            Path = testFile, 
            Type = PathType.File,
            Metadata = new Dictionary<string, object> { ["CustomKey"] = "CustomValue" }
        };

        // Act
        var documents = await _processor.ProcessAsync(indexPath);

        // Assert
        var document = Assert.Single(documents);
        Assert.Contains("CustomKey", document.Metadata.Keys);
        Assert.Equal("CustomValue", document.Metadata["CustomKey"]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}