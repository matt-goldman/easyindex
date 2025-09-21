using System.Text;

namespace EasyIndex;

/// <summary>
/// Processes file paths to extract text content for indexing
/// </summary>
public class FilePathProcessor : IPathProcessor
{
    public PathType SupportedType => PathType.File;

    public async Task<IEnumerable<IndexedDocument>> ProcessAsync(IndexPath path, CancellationToken cancellationToken = default)
    {
        if (path.Type != SupportedType)
        {
            throw new ArgumentException($"Unsupported path type: {path.Type}");
        }

        var documents = new List<IndexedDocument>();

        if (Directory.Exists(path.Path))
        {
            // Process directory
            await ProcessDirectoryAsync(path.Path, documents, cancellationToken);
        }
        else if (File.Exists(path.Path))
        {
            // Process single file
            var document = await ProcessFileAsync(path.Path, cancellationToken);
            if (document != null)
            {
                // Merge metadata from IndexPath
                foreach (var metadata in path.Metadata)
                {
                    document.Metadata[metadata.Key] = metadata.Value;
                }
                documents.Add(document);
            }
        }
        else
        {
            throw new FileNotFoundException($"Path not found: {path.Path}");
        }

        return documents;
    }

    private async Task ProcessDirectoryAsync(string directoryPath, List<IndexedDocument> documents, CancellationToken cancellationToken)
    {
        var supportedExtensions = new[] { ".txt", ".md", ".csv", ".json", ".xml", ".log" };

        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
            .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = await ProcessFileAsync(file, cancellationToken);
            if (document != null)
            {
                documents.Add(document);
            }
        }
    }

    private async Task<IndexedDocument?> ProcessFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);

            return new IndexedDocument
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                SourcePath = filePath,
                SourceType = PathType.File,
                Metadata = new Dictionary<string, object>
                {
                    ["FileName"] = Path.GetFileName(filePath),
                    ["FileExtension"] = Path.GetExtension(filePath),
                    ["FileSize"] = new FileInfo(filePath).Length,
                    ["LastModified"] = File.GetLastWriteTime(filePath)
                }
            };
        }
        catch (Exception ex)
        {
            // Log error but don't fail the entire indexing process
            Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            return null;
        }
    }
}