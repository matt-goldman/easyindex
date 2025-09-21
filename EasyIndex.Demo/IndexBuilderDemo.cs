using EasyIndex;

Console.WriteLine("EasyIndex Demo - Fluent IndexBuilder API");
Console.WriteLine("========================================");

// Create temporary test files
var tempDir = Path.Combine(Path.GetTempPath(), "easyindex-indexbuilder-demo");
Directory.CreateDirectory(tempDir);

try
{
    // Create sample files
    var file1 = Path.Combine(tempDir, "document1.txt");
    var file2 = Path.Combine(tempDir, "document2.md");
    var file3 = Path.Combine(tempDir, "notes.txt");
    var indexFile = Path.Combine(tempDir, "saved_index.json");

    await File.WriteAllTextAsync(file1, "This document contains information about machine learning algorithms and artificial intelligence techniques.");
    await File.WriteAllTextAsync(file2, "# Software Development\n\nThis markdown file discusses software engineering best practices, coding standards, and development methodologies.");
    await File.WriteAllTextAsync(file3, "Personal notes about data science, statistics, and machine learning research papers.");

    Console.WriteLine("Created sample files for indexing...");

    // Demo 1: Basic fluent API usage
    Console.WriteLine("\n=== Demo 1: Basic Fluent API ===");
    
    var searchEngine = await IndexBuilder.Create()
        .AddPath(file1, PathType.File)
        .WithMetadata("Category", "AI/ML")
        .WithMetadata("Priority", "High")
        .AddPath(file2, PathType.File)
        .WithMetadata("Category", "Development")
        .AddPath(file3, PathType.File)
        .WithMetadata("Category", "Research")
        .BuildAsync();

    Console.WriteLine($"Indexed {searchEngine.GetAllDocuments().Count()} documents using fluent API");

    // Demo search
    var results = searchEngine.Search("machine learning", 5);
    Console.WriteLine("\nSearch results for 'machine learning':");
    foreach (var result in results)
    {
        var doc = result.Document;
        Console.WriteLine($"  Score: {result.Score:F2} | Source: {Path.GetFileName(doc.SourcePath)} | Category: {doc.Metadata.GetValueOrDefault("Category", "N/A")}");
    }

    // Demo 2: Save and Load
    Console.WriteLine("\n=== Demo 2: Save and Load Index ===");
    
    var builderForSaving = IndexBuilder.Create();
    var engineForSaving = await builderForSaving
        .AddPath(tempDir, PathType.File) // Index entire directory
        .WithMetadata("Source", "DemoFiles")
        .WithMetadata("IndexedAt", DateTime.UtcNow)
        .AddPath("demo.database.articles", PathType.Table)
        .WithMetadata("Source", "MockDatabase")
        .BuildAsync();

    Console.WriteLine($"Built index with {engineForSaving.GetAllDocuments().Count()} documents");

    // Save to file
    await builderForSaving.SaveToFileAsync(indexFile);
    Console.WriteLine($"Saved index to: {indexFile}");

    // Load from file
    var loadedEngine = await IndexBuilder.LoadFromFileAsync(indexFile);
    Console.WriteLine($"Loaded index with {loadedEngine.GetAllDocuments().Count()} documents");

    // Verify loaded index works
    var loadedResults = loadedEngine.Search("software development", 3);
    Console.WriteLine("\nSearch results from loaded index for 'software development':");
    foreach (var result in loadedResults)
    {
        var doc = result.Document;
        Console.WriteLine($"  Score: {result.Score:F2} | Source: {Path.GetFileName(doc.SourcePath)}");
        Console.WriteLine($"    Content preview: {doc.Content.Substring(0, Math.Min(100, doc.Content.Length))}...");
    }

    // Demo 3: Complex fluent chaining with multiple data sources
    Console.WriteLine("\n=== Demo 3: Complex Fluent Chaining ===");

    var complexEngine = await IndexBuilder.Create()
        .AddPaths(new[]
        {
            new IndexPath { Path = file1, Type = PathType.File, Metadata = new() { ["Department"] = "Research" } },
            new IndexPath { Path = file2, Type = PathType.File, Metadata = new() { ["Department"] = "Engineering" } }
        })
        .AddPath("analytics.warehouse.reports", PathType.Table)
        .WithMetadata("Department", "Analytics")
        .WithMetadata("AccessLevel", "Restricted")
        .BuildAsync();

    Console.WriteLine($"Complex index created with {complexEngine.GetAllDocuments().Count()} documents");

    // Show metadata from different sources
    Console.WriteLine("\nDocument metadata examples:");
    foreach (var doc in complexEngine.GetAllDocuments().Take(3))
    {
        Console.WriteLine($"\nDocument: {Path.GetFileName(doc.SourcePath)} ({doc.SourceType})");
        foreach (var meta in doc.Metadata.Take(3))
        {
            Console.WriteLine($"  {meta.Key}: {meta.Value}");
        }
    }

    // Demo 4: Error handling
    Console.WriteLine("\n=== Demo 4: Error Handling ===");
    
    try
    {
        // Try to load non-existent file
        await IndexBuilder.LoadFromFileAsync("nonexistent.json");
    }
    catch (FileNotFoundException ex)
    {
        Console.WriteLine($"✓ Properly caught FileNotFoundException: {ex.Message}");
    }

    try
    {
        // Try to save empty index
        await IndexBuilder.Create().SaveToFileAsync("empty.json");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"✓ Properly caught InvalidOperationException: {ex.Message}");
    }

    try
    {
        // Try to add metadata without paths
        IndexBuilder.Create().WithMetadata("Key", "Value");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"✓ Properly caught InvalidOperationException: {ex.Message}");
    }

    Console.WriteLine("\n=== IndexBuilder API Demo Complete ===");
    Console.WriteLine("\nKey Features Demonstrated:");
    Console.WriteLine("✓ Fluent API with method chaining");
    Console.WriteLine("✓ Adding individual paths and collections");
    Console.WriteLine("✓ Metadata support for paths and documents");
    Console.WriteLine("✓ Save/Load functionality with JSON serialization");
    Console.WriteLine("✓ Integration with existing SearchIndexEngine");
    Console.WriteLine("✓ Proper error handling and validation");
}
finally
{
    // Cleanup
    if (Directory.Exists(tempDir))
    {
        Directory.Delete(tempDir, true);
    }
}