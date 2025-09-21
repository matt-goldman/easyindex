namespace EasyIndex.Tests;

public class TablePathProcessorTests
{
    private readonly TablePathProcessor _processor;

    public TablePathProcessorTests()
    {
        _processor = new TablePathProcessor();
    }

    [Fact]
    public void SupportedType_ShouldReturnTable()
    {
        Assert.Equal(PathType.Table, _processor.SupportedType);
    }

    [Fact]
    public async Task ProcessAsync_WithTablePath_ShouldReturnDocuments()
    {
        // Arrange
        var indexPath = new IndexPath { Path = "server.database.schema.table", Type = PathType.Table };

        // Act
        var documents = (await _processor.ProcessAsync(indexPath)).ToList();

        // Assert
        Assert.NotEmpty(documents);
        Assert.All(documents, doc => 
        {
            Assert.Equal(PathType.Table, doc.SourceType);
            Assert.Equal("server.database.schema.table", doc.SourcePath);
            Assert.Contains("TableName", doc.Metadata.Keys);
            Assert.Equal("table", doc.Metadata["TableName"]);
        });
    }

    [Fact]
    public async Task ProcessAsync_WithWrongPathType_ShouldThrowException()
    {
        // Arrange
        var indexPath = new IndexPath { Path = "test", Type = PathType.File };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _processor.ProcessAsync(indexPath));
    }

    [Fact]
    public async Task ProcessAsync_WithMetadata_ShouldMergeMetadata()
    {
        // Arrange
        var indexPath = new IndexPath 
        { 
            Path = "test.table", 
            Type = PathType.Table,
            Metadata = new Dictionary<string, object> { ["CustomKey"] = "CustomValue" }
        };

        // Act
        var documents = await _processor.ProcessAsync(indexPath);

        // Assert
        Assert.All(documents, doc =>
        {
            Assert.Contains("CustomKey", doc.Metadata.Keys);
            Assert.Equal("CustomValue", doc.Metadata["CustomKey"]);
        });
    }

    [Fact]
    public async Task ProcessAsync_WithPipeDelimitedPath_ShouldExtractTableName()
    {
        // Arrange
        var indexPath = new IndexPath { Path = "connection_string|my_table", Type = PathType.Table };

        // Act
        var documents = await _processor.ProcessAsync(indexPath);

        // Assert
        Assert.All(documents, doc =>
        {
            Assert.Contains("TableName", doc.Metadata.Keys);
            Assert.Equal("my_table", doc.Metadata["TableName"]);
        });
    }
}