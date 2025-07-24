# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ATRC is a high-performance .NET library for ATRC configuration files, providing 100% feature parity with the original C++ implementation. This is a C# conversion/rewrite project currently on branch `convert-to-csharp`. The library processes ATRC files which are configuration files with preprocessing directives, variable substitution, and platform-specific conditionals.

## Architecture

The project follows a modern .NET library structure with multi-targeting support:

### Core Structure
- **`src/Atrc.Core/`**: Main library targeting .NET Standard 2.0, .NET 6.0, and .NET 8.0
  - `Models/`: Core data structures (AtrcFileData, AtrcBlock, AtrcVariable, AtrcKey)
  - `Parsing/`: ATRC file parser implementation
  - `Preprocessing/`: Preprocessor system for conditionals and platform detection
  - `StandardLibrary/`: Type conversion utilities (AtrcConversions)
  - `Exceptions/`: Custom exception hierarchy

- **`src/Atrc.Core.Tests/`**: Comprehensive test suite using xUnit
  - `BasicAtrcTests.cs`: Core functionality tests
  - `PreprocessorTests.cs`: Preprocessor system tests
  - `VariableSubstitutionTests.cs`: Variable substitution tests
  - `StandardLibraryTests.cs`: Type conversion tests
  - `RealFileTests.cs`: Integration tests with real ATRC files

- **`samples/ConsoleExample/`**: Example console application demonstrating library usage

### Key Components
1. **AtrcFileData Class**: Main C# interface for reading/writing ATRC files (equivalent to ATRC_FD)
2. **AtrcParser**: Handles parsing of ATRC file format
3. **AtrcPreprocessor**: Processes conditionals, variables, and platform detection
4. **Type Conversion System**: Provides safe conversions between string values and .NET types

## Common Development Commands

### Building the Library

```bash
# Navigate to core library
cd src/Atrc.Core

# Build for all target frameworks
dotnet build

# Build in Release mode
dotnet build -c Release

# Build for specific framework
dotnet build --framework net8.0
```

### Running Tests

```bash
# Navigate to test project
cd src/Atrc.Core.Tests

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~BasicAtrcTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Sample Application

```bash
# Navigate to sample project
cd samples/ConsoleExample

# Build and run the example
dotnet run

# Or build first, then run
dotnet build
dotnet run
```

### Package Creation

```bash
# Navigate to core library
cd src/Atrc.Core

# Create NuGet package
dotnet pack -c Release

# Create package with specific version
dotnet pack -c Release -p:PackageVersion=1.0.0
```

## ATRC File Format

ATRC files use a custom configuration format with:
- Header: `#!ATRC` (optional)
- Variables: `%variable%=value`
- Private variables: `[PRIVATE]` section
- Blocks: `[BlockName]`
- Keys: `KeyName=Value`
- Comments: `# Comment text`
- Preprocessor directives: `#.IF`, `#.ELIF`, `#.ELSE`, `#.ENDIF`, `#.ERROR`
- Platform detection: `WINDOWS`, `LINUX`, `UNIX`
- Time injection: `%*%` (outputs current time percentage)

## Development Notes

- Multi-targeting: .NET Standard 2.0, .NET 6.0, .NET 8.0 for maximum compatibility
- Async-first design with proper cancellation token support
- Thread-safe concurrent collections for multi-threaded scenarios
- Memory-efficient with proper disposal patterns
- Comprehensive exception handling with custom exception hierarchy
- XML documentation generation enabled for API documentation

## API Usage Examples

### Basic File Operations
```csharp
// Load existing file
using var fileData = await AtrcFileData.LoadAsync("config.atrc", ReadMode.ReadOnly);

// Create new file
using var newFile = await AtrcFileData.LoadAsync("new.atrc", ReadMode.CreateAndRead);

// Create empty in-memory instance
using var empty = AtrcFileData.CreateEmpty();
```

### Reading Data
```csharp
// Read variables and keys
string value = fileData.ReadVariable("variableName");
string keyValue = fileData.ReadKey("blockName", "keyName");

// Using indexer access
string var = fileData["variableName"];
string key = fileData["blockName.keyName"];
string keyValue2 = fileData["blockName", "keyName"];
```

### Type Conversions
```csharp
// Safe type conversions using extension methods
bool isDebug = fileData["debugMode"]?.ToBool() ?? false;
int maxConn = fileData["maxConnections"]?.ToInt() ?? 0;
double timeout = fileData["timeoutSeconds"]?.ToDouble() ?? 0.0;
string[] formats = fileData["supportedFormats"]?.ToStringArray() ?? new string[0];
```

## File Locations

- Main library: `src/Atrc.Core/`
- Tests: `src/Atrc.Core.Tests/`
- Examples: `samples/ConsoleExample/`
- Sample ATRC files: `test.atrc`, `samples/ConsoleExample/sample.atrc`
- Documentation: `docs/index.html`
- Version info: `project/VERSION`

## Current Status

**Phase Status**: Phase 2C Complete (Core C# Implementation)
- ✅ Complete C# library implementation
- ✅ 100% feature parity with C++ version
- ✅ Comprehensive test suite (12+ tests)
- ✅ Multi-framework targeting
- ✅ Example applications
- ✅ NuGet package ready
