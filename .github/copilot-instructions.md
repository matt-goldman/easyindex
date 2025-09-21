# EasyIndex - Full-Text Search Library

EasyIndex is a .NET 8.0 library providing full-text search indexing capabilities across files and database tables. It uses TF-IDF scoring with an inverted index for fast searching and supports an extensible plugin architecture.

**Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Build, Test, and Run the Repository:
- `dotnet build` -- Takes ~35 seconds. NEVER CANCEL. Set timeout to 90+ seconds.
- `dotnet test` -- Takes ~8 seconds. NEVER CANCEL. Set timeout to 30+ seconds.
- `dotnet format` -- Fixes whitespace and formatting issues. Always run before committing.
- `dotnet format --verify-no-changes` -- Validates formatting compliance.

### Run the Demo Application:
- `cd EasyIndex.Demo && dotnet run` -- Takes ~2 seconds. Shows complete functionality with file indexing, table mocking, and search examples.

### Manual Testing After Changes:
- Always run the demo application to verify end-to-end functionality works.
- Test file indexing by placing test files in `/tmp/easyindex-validation/` and indexing them.
- Verify search results return expected scores and matched terms.
- Check that both file and table (mock) indexing work correctly.

### Common Development Workflow:
1. `dotnet build` -- Ensure compilation succeeds
2. `dotnet test` -- Ensure all 15 tests pass
3. `cd EasyIndex.Demo && dotnet run` -- Verify functionality works end-to-end
4. `dotnet format` -- Fix formatting issues
5. Test your specific changes manually

## Validation Scenarios

### Always Test These User Scenarios After Making Changes:
1. **File Indexing Test**: Create test files with known content, index them, search for terms, verify correct results and scores.
2. **Search Accuracy Test**: Search for terms that should match, verify TF-IDF scoring is reasonable and matched terms are highlighted.
3. **Demo Application Test**: Run the full demo and verify all search examples work (machine learning, software development, data science).
4. **Extensibility Test**: Verify that custom path processors can be registered and work correctly.

### Expected Demo Output Validation:
The demo should index 6 documents (3 files + 3 mock table entries) and return search results like:
- "machine learning" → 2 results with scores ~2.20
- "software development" → 1 result with score ~7.17  
- "data science" → 2 results with scores ~2.89 and ~1.10
- "quantum physics" → No results

## Project Structure

### Core Projects:
- **EasyIndex/**: Main library with SearchIndexEngine, path processors, and core classes
- **EasyIndex.Demo/**: Console application demonstrating full functionality
- **EasyIndex.Tests/**: xUnit test suite (15 tests covering all major functionality)

### Key Classes and Files:
- `SearchIndexEngine.cs`: Main orchestrator for indexing and searching
- `FilePathProcessor.cs`: Handles file system indexing (.txt, .md, .csv, .json, .xml, .log)
- `TablePathProcessor.cs`: Mock implementation for database table indexing
- `Class1.cs`: Core data models (IndexPath, IndexedDocument, SearchResult, IPathProcessor)

### Test Coverage:
- `UnitTest1.cs`: SearchIndexEngine tests
- `FilePathProcessorTests.cs`: File processing tests  
- `TablePathProcessorTests.cs`: Table processing tests

## Build Times and Timeouts

**CRITICAL TIMING INFORMATION:**
- **Build**: ~35 seconds. NEVER CANCEL. Use timeout of 90+ seconds minimum.
- **Tests**: ~8 seconds. NEVER CANCEL. Use timeout of 30+ seconds minimum.  
- **Demo Run**: ~2 seconds. Use timeout of 30+ seconds minimum.
- **Full Clean/Build/Test/Demo**: ~22 seconds total. Use timeout of 60+ seconds minimum.

## Requirements and Dependencies

### System Requirements:
- .NET 8.0 SDK (verified compatible)
- No external dependencies for core functionality
- Linux/Windows/macOS compatible

### Supported File Types:
- Text files: `.txt`, `.md`, `.csv`, `.json`, `.xml`, `.log`
- Processes files recursively when given directory paths

## Linting and Code Quality

### Always Run Before Committing:
- `dotnet format` -- Fixes all whitespace and formatting issues automatically
- `dotnet format --verify-no-changes` -- Validates code meets formatting standards  
- `dotnet build` -- Ensures no compilation errors
- `dotnet test` -- Ensures all functionality works

### Formatting Standards:
The project uses standard .NET formatting conventions. There are no custom linting rules or additional analyzers configured.

## Architecture Overview

### Core Components:
- **SearchIndexEngine**: Main entry point, coordinates path processors
- **IPathProcessor**: Interface for extensible data source support
- **FilePathProcessor**: Built-in file system support
- **TablePathProcessor**: Built-in mock database support
- **Inverted Index**: In-memory TF-IDF based search index

### Extension Points:
To add custom data sources, implement `IPathProcessor`:
```csharp
public class CustomProcessor : IPathProcessor
{
    public PathType SupportedType => PathType.Custom; // Add enum value
    public async Task<IEnumerable<IndexedDocument>> ProcessAsync(IndexPath path, CancellationToken cancellationToken = default)
    {
        // Custom implementation
    }
}
```

## Common Tasks Reference

### Repository Root Contents:
```
.
├── EasyIndex/                    # Main library
├── EasyIndex.Demo/              # Demo console application  
├── EasyIndex.Tests/             # Unit tests (xUnit)
├── EasyIndex.sln                # Solution file
├── README.md                    # Documentation
├── LICENSE                      # MIT License
└── .gitignore                   # Standard .NET gitignore
```

### Most Frequently Modified Files:
- `SearchIndexEngine.cs` -- Core search and indexing logic
- `FilePathProcessor.cs` -- File handling and metadata extraction  
- Test files when adding new functionality
- Demo program when testing new features

### Target Framework:
All projects target .NET 8.0 with nullable reference types enabled and implicit usings.