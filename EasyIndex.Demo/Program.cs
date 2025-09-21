using EasyIndex;

Console.WriteLine("EasyIndex Demo - Full-Text Search Library");
Console.WriteLine("==========================================");

// Create search engine
var searchEngine = new SearchIndexEngine();

// Create temporary test files
var tempDir = Path.Combine(Path.GetTempPath(), "easyindex-demo");
Directory.CreateDirectory(tempDir);

try
{
    // Create sample files
    var file1 = Path.Combine(tempDir, "document1.txt");
    var file2 = Path.Combine(tempDir, "document2.md");
    var file3 = Path.Combine(tempDir, "notes.txt");

    await File.WriteAllTextAsync(file1, "This document contains information about machine learning algorithms and artificial intelligence techniques.");
    await File.WriteAllTextAsync(file2, "# Software Development\n\nThis markdown file discusses software engineering best practices, coding standards, and development methodologies.");
    await File.WriteAllTextAsync(file3, "Personal notes about data science, statistics, and machine learning research papers.");

    Console.WriteLine("Created sample files for indexing...");

    // Create index paths
    var paths = new[]
    {
        new IndexPath
        {
            Path = tempDir,
            Type = PathType.File,
            Metadata = new Dictionary<string, object> { ["Source"] = "Demo Files" }
        },
        new IndexPath
        {
            Path = "demo.database.schema.articles",
            Type = PathType.Table,
            Metadata = new Dictionary<string, object> { ["Source"] = "Demo Database" }
        }
    };

    // Index the content
    Console.WriteLine("Indexing content...");
    await searchEngine.IndexAsync(paths);

    var allDocs = searchEngine.GetAllDocuments().ToList();
    Console.WriteLine($"Indexed {allDocs.Count} documents");

    // Demonstrate search functionality
    Console.WriteLine("\n--- Search Examples ---");

    // Search 1: Machine learning
    Console.WriteLine("\nSearching for 'machine learning':");
    var results1 = searchEngine.Search("machine learning", 5);
    DisplayResults(results1);

    // Search 2: Software development
    Console.WriteLine("\nSearching for 'software development':");
    var results2 = searchEngine.Search("software development", 5);
    DisplayResults(results2);

    // Search 3: Data science
    Console.WriteLine("\nSearching for 'data science':");
    var results3 = searchEngine.Search("data science", 5);
    DisplayResults(results3);

    // Search 4: Non-existent term
    Console.WriteLine("\nSearching for 'quantum physics':");
    var results4 = searchEngine.Search("quantum physics", 5);
    DisplayResults(results4);

    Console.WriteLine("\n--- Document Metadata ---");
    foreach (var doc in allDocs.Take(3))
    {
        Console.WriteLine($"\nDocument ID: {doc.Id}");
        Console.WriteLine($"Source: {doc.SourcePath} ({doc.SourceType})");
        Console.WriteLine($"Content preview: {doc.Content.Substring(0, Math.Min(100, doc.Content.Length))}...");
        Console.WriteLine("Metadata:");
        foreach (var meta in doc.Metadata)
        {
            Console.WriteLine($"  {meta.Key}: {meta.Value}");
        }
    }
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
