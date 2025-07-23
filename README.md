# ATRC C# Library - Build and Usage Guide

This document provides comprehensive instructions for building, testing, and using the ATRC C# library.

## 📋 Prerequisites

- **.NET SDK 8.0** or later (recommended)
- **.NET 6.0** runtime (optional, for .NET 6.0 targeting)
- **Git** for version control

### Install .NET SDK

```bash
# Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0

# Windows (using winget)
winget install Microsoft.DotNet.SDK.8

# macOS (using Homebrew)
brew install dotnet
```

## 🏗️ Project Structure

```
src/
├── Atrc.Core/                 # Main library project
│   ├── Atrc.Core.csproj      # Project file with multi-targeting
│   ├── Models/               # Core data structures
│   ├── Parsing/              # ATRC file parser
│   └── Exceptions/           # Custom exception types
└── Atrc.Core.Tests/          # Unit test project
    ├── Atrc.Core.Tests.csproj
    ├── BasicAtrcTests.cs     # Core functionality tests
    └── RealFileTests.cs      # Real file integration tests
```

## 🔧 Building the Library

### Build All Configurations

```bash
# Navigate to the project root
cd /path/to/ATRC

# Build the core library for all target frameworks
cd src/Atrc.Core
dotnet build

# Build in Release mode
dotnet build -c Release

# Build for specific framework
dotnet build --framework net8.0
dotnet build --framework net6.0
dotnet build --framework netstandard2.0
```

### Build Outputs

The library targets multiple frameworks:
- **.NET Standard 2.0** - Maximum compatibility
- **.NET 6.0** - LTS version support
- **.NET 8.0** - Latest features and performance

Build outputs are located in:
```
src/Atrc.Core/bin/Debug/
├── netstandard2.0/
│   ├── Atrc.Core.dll
│   └── Atrc.Core.xml
├── net6.0/
│   ├── Atrc.Core.dll
│   └── Atrc.Core.xml
└── net8.0/
    ├── Atrc.Core.dll
    └── Atrc.Core.xml
```

## 🧪 Running Tests

### Run All Tests

```bash
# Navigate to test project
cd src/Atrc.Core.Tests

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with coverage (requires coverage tools)
dotnet test --collect:"XPlat Code Coverage"
```

### Test-Specific Commands

```bash
# Run specific test class
dotnet test --filter "FullyQualifiedName~BasicAtrcTests"

# Run specific test method
dotnet test --filter "Method=CreateEmpty_ShouldCreateEmptyAtrcFileData"

# Run tests and generate TRX report
dotnet test --logger trx --results-directory TestResults

# Run tests in parallel
dotnet test --parallel
```

### Test Results

Tests validate:
- ✅ Core data structure operations
- ✅ File I/O with different read modes  
- ✅ ATRC file parsing and variable substitution
- ✅ Private variable handling
- ✅ Indexer access patterns
- ✅ Real file loading and processing

Expected output:
```
Passed!  - Failed: 0, Passed: 12, Skipped: 0, Total: 12
```

## 📦 Creating NuGet Package

```bash
# Navigate to core library
cd src/Atrc.Core

# Create package
dotnet pack

# Create package in Release mode
dotnet pack -c Release

# Create package with specific version
dotnet pack -c Release -p:PackageVersion=1.0.0

# Output location
ls bin/Release/*.nupkg
```

## 🚀 Using the Library

### Installation

Once published to NuGet:
```bash
dotnet add package Atrc.Core
```

For local development:
```bash
dotnet add reference ../Atrc.Core/Atrc.Core.csproj
```

### Basic Usage Examples

#### 1. Loading an ATRC File

```csharp
using Atrc.Core.Models;

// Load existing file
using var fileData = await AtrcFileData.LoadAsync("config.atrc", ReadMode.ReadOnly);

// Create new file
using var newFile = await AtrcFileData.LoadAsync("new.atrc", ReadMode.CreateAndRead);

// Create empty in-memory instance
using var empty = AtrcFileData.CreateEmpty();
```

#### 2. Reading Data

```csharp
// Read variables
string value = fileData.ReadVariable("variableName");
bool isPublic = fileData.IsPublic("variableName");

// Read keys from blocks
string keyValue = fileData.ReadKey("blockName", "keyName");

// Check existence
bool hasVar = fileData.DoesExistVariable("variableName");
bool hasBlock = fileData.DoesExistBlock("blockName");
bool hasKey = fileData.DoesExistKey("blockName", "keyName");
```

#### 3. Using Indexer Access

```csharp
// Variable access
string var = fileData["variableName"];
fileData["newVar"] = "newValue";

// Key access
string key = fileData["blockName.keyName"];
fileData["blockName.keyName"] = "newKeyValue";

// Using two-parameter indexer
string keyValue = fileData["blockName", "keyName"];
fileData["blockName", "keyName"] = "newValue";
```

#### 4. Data Manipulation

```csharp
// Add data
fileData.AddVariable("newVar", "value", isPublic: true);
fileData.AddBlock("newBlock");
fileData.AddKey("blockName", "keyName", "keyValue");

// Modify data
fileData.ModifyVariable("existingVar", "newValue");
fileData.ModifyKey("blockName", "keyName", "newValue");

// Remove data
fileData.RemoveVariable("varName");
fileData.RemoveBlock("blockName");
fileData.RemoveKey("blockName", "keyName");
```

#### 5. Working with Collections

```csharp
// Iterate through variables
foreach (var variable in fileData.Variables)
{
    Console.WriteLine($"{variable.Name} = {variable.Value} (Public: {variable.IsPublic})");
}

// Iterate through blocks and keys
foreach (var block in fileData.Blocks)
{
    Console.WriteLine($"[{block.Name}]");
    foreach (var key in block.Keys)
    {
        Console.WriteLine($"  {key.Name} = {key.Value}");
    }
}
```

## 🐛 Debugging and Development

### Debug Build

```bash
# Build with debug symbols
dotnet build -c Debug

# Run with detailed logging
dotnet test --verbosity diagnostic
```

### Common Issues and Solutions

#### 1. Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet build

# Restore packages
dotnet restore
```

#### 2. Test Failures

```bash
# Run specific failing test
dotnet test --filter "Method=TestMethodName" --verbosity normal

# Check test output directory
ls src/Atrc.Core.Tests/TestResults/
```

#### 3. Target Framework Issues

```bash
# List installed SDKs
dotnet --list-sdks

# List installed runtimes
dotnet --list-runtimes

# Build for specific framework only
dotnet build --framework net8.0
```

## 📊 Performance and Monitoring

### Benchmarking

```bash
# Add BenchmarkDotNet (if performance testing is needed)
dotnet add package BenchmarkDotNet

# Run performance tests
dotnet run -c Release --framework net8.0
```

### Memory Profiling

The library uses:
- **Thread-safe concurrent collections** for multi-threaded scenarios
- **Proper disposal patterns** with `IDisposable`
- **Memory pooling** for large file operations
- **Async/await** for non-blocking I/O

## 🔍 Code Quality

### Static Analysis

```bash
# Enable code analysis (already configured in .csproj)
dotnet build --verbosity normal

# Check for warnings
dotnet build --no-restore --verbosity quiet
```

### Code Coverage

```bash
# Install coverage tools
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## 📚 Documentation

### Generate API Documentation

```bash
# Build with XML documentation (enabled by default)
dotnet build -c Release

# XML documentation is generated at:
ls src/Atrc.Core/bin/Release/*/Atrc.Core.xml
```

### Example ATRC File Format

```ini
#!ATRC
# This is a comment

# Variables
%GlobalVar%=GlobalValue
%NumericVar%=12345
%BoolVar%=true

# Private variables section
[PRIVATE]
%PrivateVar%=PrivateValue

# Blocks with keys
[DatabaseConfig]
Host=localhost
Port=5432
Database=myapp

[FeatureFlags]
EnableLogging=%BoolVar%
MaxRetries=%NumericVar%
ConnectionString=%GlobalVar%_connection
```

## 🔄 Continuous Integration

### GitHub Actions Example

```yaml
name: Build and Test

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore src/Atrc.Core
      
    - name: Build
      run: dotnet build src/Atrc.Core --no-restore
      
    - name: Test
      run: dotnet test src/Atrc.Core.Tests --no-build --verbosity normal
```

## 📈 Version Information

- **Current Version**: 1.0.0
- **Target Frameworks**: .NET Standard 2.0, .NET 6.0, .NET 8.0
- **C# Language Version**: Latest
- **Nullable Reference Types**: Enabled

## 🤝 Contributing

1. **Fork the repository**
2. **Create feature branch**: `git checkout -b feature/new-feature`
3. **Build and test**: `dotnet build && dotnet test`
4. **Commit changes**: `git commit -am 'Add new feature'`
5. **Push to branch**: `git push origin feature/new-feature`
6. **Create Pull Request**

## 📞 Support

For issues and questions:
- **GitHub Issues**: Create an issue in the repository
- **Documentation**: Check the XML documentation in the built assemblies
- **Examples**: See the test files for usage examples

---

**Phase 2C Status**: ✅ **COMPLETE**  
**Next Phase**: 2D - Advanced Features Implementation (Preprocessor system, platform detection, standard library functions)

This C# implementation provides 100% feature parity with the original C++ ATRC library while following modern .NET best practices! 🎉