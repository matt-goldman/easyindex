using System.Text.RegularExpressions;

namespace EasyIndex;

/// <summary>
/// Main search index engine that coordinates processing and provides search functionality
/// </summary>
public class SearchIndexEngine
{
    private readonly Dictionary<PathType, IPathProcessor> _processors;
    private readonly List<IndexedDocument> _documents;
    private readonly Dictionary<string, HashSet<string>> _invertedIndex;

    public SearchIndexEngine()
    {
        _processors = new Dictionary<PathType, IPathProcessor>();
        _documents = new List<IndexedDocument>();
        _invertedIndex = new Dictionary<string, HashSet<string>>();

        // Register default processors
        RegisterProcessor(new FilePathProcessor());
        RegisterProcessor(new TablePathProcessor());
    }

    public void RegisterProcessor(IPathProcessor processor)
    {
        _processors[processor.SupportedType] = processor;
    }

    public async Task IndexAsync(IEnumerable<IndexPath> paths, CancellationToken cancellationToken = default)
    {
        var allDocuments = new List<IndexedDocument>();

        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_processors.TryGetValue(path.Type, out var processor))
            {
                throw new NotSupportedException($"No processor registered for path type: {path.Type}");
            }

            var documents = await processor.ProcessAsync(path, cancellationToken);
            allDocuments.AddRange(documents);
        }

        // Add to document collection
        _documents.AddRange(allDocuments);

        // Build inverted index
        BuildInvertedIndex(allDocuments);
    }

    public IEnumerable<SearchResult> Search(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<SearchResult>();
        }

        var queryTerms = TokenizeText(query.ToLowerInvariant());
        var documentScores = new Dictionary<string, (IndexedDocument doc, double score, HashSet<string> matchedTerms)>();

        foreach (var term in queryTerms)
        {
            if (_invertedIndex.TryGetValue(term, out var documentIds))
            {
                foreach (var docId in documentIds)
                {
                    var document = _documents.First(d => d.Id == docId);

                    if (!documentScores.ContainsKey(docId))
                    {
                        documentScores[docId] = (document, 0.0, new HashSet<string>());
                    }

                    var current = documentScores[docId];
                    // Simple TF-IDF like scoring
                    var termFrequency = CountTermOccurrences(document.Content.ToLowerInvariant(), term);
                    var inverseDocumentFrequency = Math.Log((double)_documents.Count / documentIds.Count);
                    var score = termFrequency * inverseDocumentFrequency;

                    current.matchedTerms.Add(term);
                    documentScores[docId] = (current.doc, current.score + score, current.matchedTerms);
                }
            }
        }

        return documentScores.Values
            .Select(item => new SearchResult
            {
                Document = item.doc,
                Score = item.score,
                MatchedTerms = item.matchedTerms.ToList()
            })
            .OrderByDescending(r => r.Score)
            .Take(maxResults);
    }

    public IEnumerable<IndexedDocument> GetAllDocuments()
    {
        return _documents.AsReadOnly();
    }

    public void ClearIndex()
    {
        _documents.Clear();
        _invertedIndex.Clear();
    }

    private void BuildInvertedIndex(IEnumerable<IndexedDocument> documents)
    {
        foreach (var document in documents)
        {
            var tokens = TokenizeText(document.Content.ToLowerInvariant());

            foreach (var token in tokens.Distinct())
            {
                if (!_invertedIndex.ContainsKey(token))
                {
                    _invertedIndex[token] = new HashSet<string>();
                }
                _invertedIndex[token].Add(document.Id);
            }
        }
    }

    private List<string> TokenizeText(string text)
    {
        // Simple tokenization - split on whitespace and punctuation
        var tokens = Regex.Split(text, @"\W+")
            .Where(token => !string.IsNullOrWhiteSpace(token) && token.Length > 2)
            .ToList();

        return tokens;
    }

    private int CountTermOccurrences(string text, string term)
    {
        var tokens = TokenizeText(text);
        return tokens.Count(t => t == term);
    }
}