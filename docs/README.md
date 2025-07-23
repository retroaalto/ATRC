# ATRC Library - Build Instructions

This document provides comprehensive build instructions for the ATRC configuration file processing library and its C# conversion project.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Building C++ Library](#building-c-library)
- [Running Tests](#running-tests)
- [C# Conversion Project](#c-conversion-project)
- [Development Environment](#development-environment)
- [Troubleshooting](#troubleshooting)

## Overview

ATRC is a cross-platform configuration file processing library that supports:
- Variable substitution (`%variable%`)
- Block-based configuration (`[BlockName]`)
- Preprocessor directives (`#.IF`, `#.ELIF`, `#.ELSE`, `#.ENDIF`)
- Platform detection (`WINDOWS`, `LINUX`, `UNIX`)
- C and C++ APIs

## Prerequisites

### For C++ Development

#### Linux (Current Platform)
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install build-essential cmake git

# Required packages
sudo apt install gcc g++ cmake make

# Optional: for better development experience
sudo apt install gdb valgrind clang-format
```

#### Windows
- Visual Studio 2019 or later with C++ workload
- CMake 3.16 or later
- Git for Windows

#### macOS
```bash
# Install Xcode command line tools
xcode-select --install

# Install CMake (using Homebrew)
brew install cmake
```

### For C# Development
- .NET SDK 6.0 or later
- .NET Standard 2.0 support for maximum compatibility
- IDE: Visual Studio, Visual Studio Code, or JetBrains Rider

## Building C++ Library

### Linux Build (Recommended for Development)

#### Quick Build
```bash
# Clone and navigate to project
cd /path/to/ATRC

# Use the build script (recommended)
./scripts/build_linux.sh

# Manual build (alternative)
cmake --preset linux-x64-debug -B./out/linux-x64-debug/build \
  -DCMAKE_BUILD_TYPE=Debug \
  -DCMAKE_TOOLCHAIN_FILE=./cmake/toolchain-linux-x64.cmake \
  -DATRC_BUILD_TESTS=ON

cmake --build ./out/linux-x64-debug/build --config Debug
```

#### Build Options
```bash
# Debug build (default)
cmake --preset linux-x64-debug -B./out/linux-x64-debug/build \
  -DCMAKE_BUILD_TYPE=Debug \
  -DATRC_BUILD_TESTS=ON

# Release build
cmake --preset linux-x64-release -B./out/linux-x64-release/build \
  -DCMAKE_BUILD_TYPE=Release \
  -DATRC_BUILD_TESTS=ON

# Build without tests
cmake --preset linux-x64-debug -B./out/linux-x64-debug/build \
  -DCMAKE_BUILD_TYPE=Debug \
  -DATRC_BUILD_TESTS=OFF
```

#### Available CMake Presets
- `linux-x64-debug` - Linux 64-bit Debug build
- `linux-x64-release` - Linux 64-bit Release build  
- `linux-x86-debug` - Linux 32-bit Debug build
- `linux-x86-release` - Linux 32-bit Release build
- `windows-x64-debug` - Windows 64-bit Debug build
- `windows-x64-release` - Windows 64-bit Release build

### Windows Build

#### Using Visual Studio
```cmd
# Run the Visual Studio setup script
.\vs_run.bat

# Or manually with CMake
cmake --preset windows-x64-debug -B.\out\windows-x64-debug\build ^
  -DCMAKE_BUILD_TYPE=Debug ^
  -DATRC_BUILD_TESTS=ON

cmake --build .\out\windows-x64-debug\build --config Debug
```

### Build Artifacts

After successful build, you'll find:
```
out/
├── linux-x64-debug/build/
│   ├── ATRC/
│   │   └── libATRC.so              # Main library
│   └── ATRC.Test/
│       └── ATRC.Test               # Test executable
└── [other platform builds...]
```

## Running Tests

### Linux
```bash
# Build first (if not already built)
./scripts/build_linux.sh

# Run the test executable
./out/linux-x64-debug/build/ATRC.Test/ATRC.Test

# Or use the test script
./scripts/run_tests.sh
```

### Windows
```cmd
# Build first
.\vs_run.bat

# Run tests
.\scripts\run_test.bat
```

### Test Files
The library includes test ATRC configuration files:
- `test.atrc` (root directory) - Main test configuration
- `ATRC.Test/test.atrc` - Additional test scenarios

## C# Conversion Project

### Phase 2 Development Plan

The C# conversion follows a structured approach outlined in `Phase2-TODO.md`:

1. **Phase 2A**: Comprehensive C++ Testing - Create validation baseline
2. **Phase 2B**: C# Architecture Design - Design .NET equivalent
3. **Phase 2C**: Core Implementation - Basic functionality
4. **Phase 2D**: Advanced Features - Preprocessor and advanced features
5. **Phase 2E**: NuGet Package - Package creation and distribution
6. **Phase 2F**: Documentation - Final documentation and guides

### C# Build Prerequisites
```bash
# Install .NET SDK (Linux)
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version latest

# Verify installation
dotnet --version
```

### C# Build Commands

The C# project is now available and ready to use:

```bash
# Navigate to C# source directory
cd src

# Restore dependencies
dotnet restore Atrc.Core.sln

# Build the library
dotnet build Atrc.Core.sln --configuration Release

# Run tests
dotnet test Atrc.Core.Tests/Atrc.Core.Tests.csproj

# Create NuGet package
dotnet pack Atrc.Core/Atrc.Core.csproj --configuration Release
```

## C# NuGet Package

The ATRC C# library is available as a NuGet package with 100% feature parity to the C++ implementation.

### Installation

```bash
# Using .NET CLI
dotnet add package Atrc.Core

# Using Package Manager Console (Visual Studio)
Install-Package Atrc.Core
```

### Quick Start

```csharp
using Atrc.Core;
using Atrc.Core.Models;
using Atrc.Core.StandardLibrary;

// Load configuration file
using var config = new AtrcFileData("config.atrc", ReadMode.ReadOnly);

// Read values with automatic variable substitution
string dbConnection = config["Database", "ConnectionString"];
bool debugMode = AtrcConversions.ToBool(config["Application", "Debug"]);
long maxConnections = AtrcConversions.ToInt64(config["Database", "MaxConnections"]);

// Access variables directly
var appName = config.GetVariable("app_name")?.Value;

// Iterate through configuration
foreach (var block in config.Blocks)
{
    Console.WriteLine($"[{block.Name}]");
    foreach (var key in block.Keys)
    {
        Console.WriteLine($"  {key.Name} = {key.Value}");
    }
}
```

### Key Features

- **Variable Substitution**: `%variable%` syntax with nested references and circular reference detection
- **Preprocessor Directives**: `#.IF`, `#.ELIF`, `#.ELSE`, `#.ENDIF` with platform detection (`WINDOWS`, `LINUX`, `UNIX`)
- **Type Conversions**: Built-in converters for bool, int, double, arrays with robust error handling
- **Memory Management**: Automatic disposal with `using` statements, no manual cleanup required
- **Exception Handling**: Structured exceptions for different error types with detailed messages
- **Thread Safety**: Safe for concurrent read operations, synchronized writes
- **LINQ Integration**: Query configuration data with LINQ expressions
- **Time Injection**: `%*%` syntax for dynamic timestamp insertion

### Sample ATRC File

```ini
# Variables with substitution
%app_name%=MyApplication
%version%=1.0.0
%debug_mode%=true

# Platform-specific paths
#.IF WINDOWS
%data_path%=C:\ProgramData\%app_name%
#.ELIF LINUX
%data_path%=/usr/local/share/%app_name%
#.ELSE
%data_path%=/tmp/%app_name%
#.ENDIF

[Application]
Name=%app_name%
Version=%version%
Debug=%debug_mode%
DataPath=%data_path%
BuildTime=%*%

[Database]
ConnectionString=Server=localhost;Database=%app_name%
MaxConnections=10
EnableSSL=true
TimeoutSeconds=30

[Features]
EnableMetrics=true
EnableCaching=false
MaxCacheSize=1000

# List-style values
[SupportedFormats]
Formats=json,xml,csv,txt
```

## Development Environment

### Recommended IDE Setup

#### Visual Studio Code (Linux/Windows/macOS)
```bash
# Install recommended extensions
code --install-extension ms-vscode.cpptools
code --install-extension ms-vscode.cmake-tools
code --install-extension ms-dotnettools.csharp
```

#### CMake Integration
The project uses CMake presets for consistent builds across platforms:
```json
// .vscode/settings.json
{
    "cmake.configurePreset": "linux-x64-debug",
    "cmake.buildPreset": "linux-x64-debug"
}
```

### Environment Variables
```bash
# Optional: Set custom install prefix
export CMAKE_INSTALL_PREFIX=/usr/local

# Optional: Enable verbose build output
export VERBOSE=1
```

## Troubleshooting

### Common Build Issues

#### Linux Issues
```bash
# CMake not found
sudo apt install cmake

# Compiler not found
sudo apt install build-essential

# Permission issues
chmod +x scripts/build_linux.sh

# Clean build (start fresh)
rm -rf out/
./scripts/build_linux.sh
```

#### Library Path Issues
```bash
# Add library to LD_LIBRARY_PATH
export LD_LIBRARY_PATH="$PWD/out/linux-x64-debug/build/ATRC:$LD_LIBRARY_PATH"

# Or run with explicit path
LD_LIBRARY_PATH="./out/linux-x64-debug/build/ATRC" ./out/linux-x64-debug/build/ATRC.Test/ATRC.Test
```

#### Windows Issues
- Ensure Visual Studio has C++ workload installed
- Check CMake is in PATH
- Use Developer Command Prompt for Visual Studio

### Build Verification
```bash
# Verify library was built
ls -la out/linux-x64-debug/build/ATRC/libATRC.so

# Check library dependencies
ldd out/linux-x64-debug/build/ATRC/libATRC.so

# Verify test executable
file out/linux-x64-debug/build/ATRC.Test/ATRC.Test
```

### Performance Optimization
```bash
# Release build for performance testing
cmake --preset linux-x64-release -B./out/linux-x64-release/build \
  -DCMAKE_BUILD_TYPE=Release

# Profile with perf (Linux)
perf record ./out/linux-x64-release/build/ATRC.Test/ATRC.Test
perf report
```

## Documentation

### C# API Reference

The C# library provides extensive API documentation with IntelliSense support:

#### Core Classes

- **`AtrcFileData`**: Main class for loading and manipulating ATRC files
  ```csharp
  // Constructors
  new AtrcFileData(string filePath, ReadMode mode)
  new AtrcFileData() // Empty file data
  
  // Properties
  IReadOnlyList<AtrcVariable> Variables { get; }
  IReadOnlyList<AtrcBlock> Blocks { get; }
  
  // Indexers
  string this[string key] { get; } // Get key from any block
  string this[string blockName, string keyName] { get; set; }
  ```

- **`AtrcVariable`**: Represents a variable with public/private scoping
  ```csharp
  public class AtrcVariable
  {
      public string Name { get; }
      public string Value { get; }
      public bool IsPublic { get; }
  }
  ```

- **`AtrcBlock`**: Configuration block containing keys
  ```csharp
  public class AtrcBlock
  {
      public string Name { get; }
      public IReadOnlyList<AtrcKey> Keys { get; }
  }
  ```

- **`AtrcKey`**: Key-value pair within a block
  ```csharp
  public class AtrcKey
  {
      public string Name { get; }
      public string Value { get; }
  }
  ```

#### Key Methods

```csharp
// File operations
var config = new AtrcFileData("file.atrc", ReadMode.ReadOnly);
await var config = AtrcFileData.LoadAsync("file.atrc", ReadMode.ReadOnly);

// Data access
string value = config["block", "key"];           // Get key value
string value = config["key"];                    // Get key from any block
AtrcVariable var = config.GetVariable("name");   // Get variable

// Data modification (requires ReadWrite mode)
config.AddBlock("NewBlock");
config.WriteKey("Block", "Key", "Value");
config.AddVariable("var_name", "value", isPublic: true);
config.RemoveBlock("BlockName");
config.SaveToFile("output.atrc");

// Type conversions
bool result = AtrcConversions.ToBool("true");
long number = AtrcConversions.ToInt64("42");
double floating = AtrcConversions.ToDouble("3.14");
string[] array = AtrcConversions.ToStringArray("item1,item2,item3");
```

#### Exception Types

- **`AtrcException`**: Base exception for all ATRC errors
- **`AtrcFileNotFoundException`**: File not found or inaccessible
- **`AtrcParseException`**: Parse error in ATRC file
- **`AtrcPreprocessorException`**: Error in preprocessor directives
- **`AtrcVariableException`**: Variable substitution error (circular references, etc.)

#### Advanced Features

```csharp
// LINQ queries
var databaseKeys = config.Blocks
    .Where(b => b.Name == "Database")
    .SelectMany(b => b.Keys)
    .ToDictionary(k => k.Name, k => k.Value);

// Variable substitution with circular reference detection
var substituted = config.SubstituteVariables("%app_name%/data/%version%");

// Platform-specific configuration
var platformPath = config["Paths", "DataDirectory"]; // Automatically resolved

// Time injection
var buildTime = config["Build", "Timestamp"]; // %*% replaced with current time
```

### Migration Guide

For developers migrating from the C++ version:

- **[`docs/MIGRATION_GUIDE.md`](MIGRATION_GUIDE.md)**: Comprehensive migration guide
  - API mapping between C++ and C#
  - Code examples showing before/after patterns
  - Key differences in memory management and error handling
  - Step-by-step migration checklist

### Sample Projects

The `samples/` directory contains example projects demonstrating various usage patterns:

#### Console Example (`samples/ConsoleExample/`)
- Basic console application showing core features
- File loading, variable substitution, type conversions
- Includes sample ATRC file with preprocessor directives

To run the sample:
```bash
cd samples/ConsoleExample
dotnet run
```

### Additional Resources

- **`USAGE.md`**: Library usage examples and patterns
- **`Phase2-TODO.md`**: Detailed C# conversion project plan
- **`CLAUDE.md`**: Development guidelines for Claude Code
- **`index.html`**: Web-based documentation (if available)

### C/C++ API Documentation

The original C++ API is defined in:
- **`ATRC/include/ATRC.h`**: Complete C/C++ API reference
- Comments in source files provide implementation details

### Build Documentation

This document (`docs/README.md`) provides comprehensive build instructions for both C++ and C# components.

## Contributing

### Development Workflow
1. Build and test C++ library to ensure baseline functionality
2. Follow Phase 2 plan for C# conversion development
3. Maintain test coverage and documentation
4. Use Gemini CLI as primary development tool (fallback to Sonnet if needed)

### Code Quality
- Follow C++17 standards for C++ code
- Follow .NET guidelines for C# code
- Maintain comprehensive test coverage
- Document all public APIs

---

For questions or issues, refer to the project documentation or create an issue in the project repository.