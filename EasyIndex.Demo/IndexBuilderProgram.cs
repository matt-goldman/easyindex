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
    var indexFile = Path.Combine(tempDir, "saved_index.json");

    await File.WriteAllTextAsync(file1, "This document contains information about machine learning algorithms and artificial intelligence techniques.");
    await File.WriteAllTextAsync(file2, "# Software Development\n\nThis markdown file discusses software engineering best practices, coding standards, and development methodologies.");

    Console.WriteLine("Created sample files for indexing...");

    // Demo 1: Basic fluent API usage
    Console.WriteLine("\n=== Demo 1: Basic Fluent API ===");
    
    var searchEngine = await IndexBuilder.Create()
        .AddPath(file1, PathType.File)
        .WithMetadata("Category", "AI/ML")
        .AddPath(file2, PathType.File)
        .WithMetadata("Category", "Development")
        .BuildAsync();

    Console.WriteLine($"Indexed {searchEngine.GetAllDocuments().Count()} documents using fluent API");

    // Demo search
    var results = searchEngine.Search("machine learning", 5);
    Console.WriteLine("\nSearch results for 'machine learning':");
    DisplayResults(results);

    // Demo 2: Save and Load
    Console.WriteLine("\n=== Demo 2: Save and Load Index ===");
    
    var builderForSaving = IndexBuilder.Create();
    var engineForSaving = await builderForSaving
        .AddPath(tempDir, PathType.File) // Index entire directory
        .WithMetadata("Source", "DemoFiles")
        .BuildAsync();

    Console.WriteLine($"Built index with {engineForSaving.GetAllDocuments().Count()} documents");

    // Save to file
    await builderForSaving.SaveToFileAsync(indexFile);
    Console.WriteLine($"Saved index to: {Path.GetFileName(indexFile)}");

    // Load from file
    var loadedEngine = await IndexBuilder.LoadFromFileAsync(indexFile);
    Console.WriteLine($"Loaded index with {loadedEngine.GetAllDocuments().Count()} documents");

    // Verify loaded index works
    var loadedResults = loadedEngine.Search("software development", 3);
    Console.WriteLine("\nSearch results from loaded index:");
    DisplayResults(loadedResults);

    // Demo 3: Complex fluent chaining
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

    Console.WriteLine("\n✅ IndexBuilder API Demo Complete!");
    Console.WriteLine("\nKey Features Demonstrated:");
    Console.WriteLine("• Fluent API with method chaining");
    Console.WriteLine("• Adding individual paths and collections");
    Console.WriteLine("• Metadata support for paths and documents");
    Console.WriteLine("• Save/Load functionality with JSON serialization");
    Console.WriteLine("• Integration with existing SearchIndexEngine");
}
finally
{
    // Cleanup
    if (Directory.Exists(tempDir))
    {
        Directory.Delete(tempDir, true);
    }
}

static void DisplayResults(IEnumerable<SearchResult> results)
{
    var resultList = results.ToList();
    if (!resultList.Any())
    {
        Console.WriteLine("  No results found.");
        return;
    }

    foreach (var result in resultList)
    {
        Console.WriteLine($"  Score: {result.Score:F2} | Source: {Path.GetFileName(result.Document.SourcePath)} | Matched: [{string.Join(", ", result.MatchedTerms)}]");
        Console.WriteLine($"    Preview: {result.Document.Content.Substring(0, Math.Min(150, result.Document.Content.Length))}...");
    }
}