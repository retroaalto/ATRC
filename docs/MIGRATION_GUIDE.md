# C++ to C# Migration Guide

This guide helps developers migrate from the ATRC C++ library to the new C# NuGet package.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [API Mapping](#api-mapping)
- [Code Examples](#code-examples)
- [Key Differences](#key-differences)
- [Migration Checklist](#migration-checklist)
- [FAQ](#faq)

## Overview

The ATRC C# library provides 100% feature parity with the C++ implementation while following .NET best practices. The core functionality remains identical, but the API has been adapted to feel natural in C#.

### Key Benefits of C# Version

- **Memory Management**: Automatic garbage collection, no manual memory management
- **Exception Handling**: Structured exception handling with custom exception types
- **Type Safety**: Strong typing with nullable reference types
- **Async Support**: Async/await patterns for file operations
- **LINQ Integration**: Query configuration data using LINQ
- **NuGet Distribution**: Easy installation and dependency management

## Installation

### Before (C++)
```cpp
// Build from source or link static library
#include "ATRC.h"
// Link: -lATRC
```

### After (C#)
```bash
# Install via NuGet Package Manager
dotnet add package Atrc.Core

# Or via Package Manager Console
Install-Package Atrc.Core
```

## API Mapping

### Core Classes

| C++ | C# | Notes |
|-----|-----|-------|
| `ATRC_FD` | `AtrcFileData` | Main file data class |
| `C_Variable` | `AtrcVariable` | Variable structure |
| `C_Key` | `AtrcKey` | Key-value pair |
| `C_Block` | `AtrcBlock` | Configuration block |

### File Operations

| C++ | C# | Notes |
|-----|-----|-------|
| `Create_ATRC_FD(filename, mode)` | `new AtrcFileData(filename, mode)` | Constructor pattern |
| `Create_Empty_ATRC_FD()` | `new AtrcFileData()` | Empty constructor |
| `Destroy_ATRC_FD(fd)` | `using` statement or `Dispose()` | Automatic disposal |

### Data Access

| C++ | C# | Notes |
|-----|-----|-------|
| `Read(fd, block, key)` | `fileData[block, key]` | Indexer syntax |
| `ReadVariable(fd, name)` | `fileData.GetVariable(name)` | Method call |
| `GetAllBlocks(fd)` | `fileData.Blocks` | Property access |
| `GetAllVariables(fd)` | `fileData.Variables` | Property access |

### Data Modification

| C++ | C# | Notes |
|-----|-----|-------|
| `AddBlock(fd, name)` | `fileData.AddBlock(name)` | Method call |
| `RemoveBlock(fd, name)` | `fileData.RemoveBlock(name)` | Method call |
| `AddVariable(fd, name, value)` | `fileData.AddVariable(name, value)` | Method call |
| `WriteKey(fd, block, key, value)` | `fileData.WriteKey(block, key, value)` | Method call |

### Type Conversions

| C++ | C# | Notes |
|-----|-----|-------|
| `atrc_to_bool(value)` | `AtrcConversions.ToBool(value)` | Static method |
| `atrc_to_int64_t(value)` | `AtrcConversions.ToInt64(value)` | Static method |
| `atrc_to_double(value)` | `AtrcConversions.ToDouble(value)` | Static method |
| `atrc_to_uint64_t(value)` | `AtrcConversions.ToUInt64(value)` | Static method |

## Code Examples

### Basic File Reading

#### C++ Version
```cpp
#include "ATRC.h"

int main() {
    C_ATRC_FD* fd = Create_ATRC_FD("config.atrc", ATRC_read_ONLY);
    if (!fd) {
        printf("Failed to load file\n");
        return 1;
    }
    
    const char* value = Read(fd, "Database", "ConnectionString");
    if (value) {
        printf("Connection: %s\n", value);
    }
    
    Destroy_ATRC_FD(fd);
    return 0;
}
```

#### C# Version
```csharp
using Atrc.Core;
using Atrc.Core.Models;

class Program 
{
    static void Main() 
    {
        try 
        {
            using var fileData = new AtrcFileData("config.atrc", ReadMode.ReadOnly);
            
            string value = fileData["Database", "ConnectionString"];
            Console.WriteLine($"Connection: {value}");
        }
        catch (AtrcException ex) 
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

### Variable Substitution

#### C++ Version
```cpp
C_ATRC_FD* fd = Create_ATRC_FD("config.atrc", ATRC_read_ONLY);
const char* appName = ReadVariable(fd, "app_name");
const char* dataPath = Read(fd, "Application", "DataPath");
// dataPath now contains substituted value
```

#### C# Version
```csharp
using var fileData = new AtrcFileData("config.atrc", ReadMode.ReadOnly);
var appName = fileData.GetVariable("app_name")?.Value;
var dataPath = fileData["Application", "DataPath"];
// dataPath now contains substituted value
```

### Type Conversions

#### C++ Version
```cpp
const char* debugStr = Read(fd, "Application", "Debug");
bool isDebug = atrc_to_bool(debugStr);

const char* portStr = Read(fd, "Server", "Port");
int64_t port = atrc_to_int64_t(portStr);
```

#### C# Version
```csharp
string debugStr = fileData["Application", "Debug"];
bool isDebug = AtrcConversions.ToBool(debugStr);

string portStr = fileData["Server", "Port"];
long port = AtrcConversions.ToInt64(portStr);
```

### Working with Collections

#### C++ Version
```cpp
C_Block* blocks = GetAllBlocks(fd);
for (int i = 0; blocks[i].name != NULL; i++) {
    printf("Block: %s\n", blocks[i].name);
    
    C_Key* keys = blocks[i].keys;
    for (int j = 0; keys[j].name != NULL; j++) {
        printf("  %s = %s\n", keys[j].name, keys[j].value);
    }
}
```

#### C# Version
```csharp
foreach (var block in fileData.Blocks) 
{
    Console.WriteLine($"Block: {block.Name}");
    
    foreach (var key in block.Keys) 
    {
        Console.WriteLine($"  {key.Name} = {key.Value}");
    }
}

// Or with LINQ
var databaseKeys = fileData.Blocks
    .Where(b => b.Name == "Database")
    .SelectMany(b => b.Keys)
    .ToDictionary(k => k.Name, k => k.Value);
```

## Key Differences

### Memory Management

**C++**: Manual memory management required
```cpp
C_ATRC_FD* fd = Create_ATRC_FD("file.atrc", ATRC_read_ONLY);
// ... use fd
Destroy_ATRC_FD(fd); // Must call explicitly
```

**C#**: Automatic memory management
```csharp
using var fileData = new AtrcFileData("file.atrc", ReadMode.ReadOnly);
// Automatically disposed at end of scope
```

### Error Handling

**C++**: Return codes and null pointers
```cpp
C_ATRC_FD* fd = Create_ATRC_FD("file.atrc", ATRC_read_ONLY);
if (!fd) {
    // Handle error
}
```

**C#**: Structured exception handling
```csharp
try 
{
    var fileData = new AtrcFileData("file.atrc", ReadMode.ReadOnly);
}
catch (AtrcFileNotFoundException ex) 
{
    // Handle file not found
}
catch (AtrcParseException ex) 
{
    // Handle parse error
}
```

### String Handling

**C++**: Manual string management
```cpp
const char* value = Read(fd, "block", "key");
// value lifetime tied to fd
```

**C#**: Automatic string management
```csharp
string value = fileData["block", "key"];
// value is a managed string
```

### Thread Safety

**C++**: Not thread-safe, manual synchronization required

**C#**: Thread-safe for read operations, explicit locking for writes
```csharp
// Multiple threads can safely read
var value1 = fileData["block1", "key1"];
var value2 = fileData["block2", "key2"];

// Writes are automatically synchronized
fileData.WriteKey("block", "key", "value");
```

## Migration Checklist

### Phase 1: Setup
- [ ] Install Atrc.Core NuGet package
- [ ] Remove C++ library references
- [ ] Update include statements to using statements

### Phase 2: Core Migration
- [ ] Replace `Create_ATRC_FD` with `new AtrcFileData`
- [ ] Replace `Destroy_ATRC_FD` with `using` statements
- [ ] Replace `Read()` calls with indexer syntax
- [ ] Replace manual memory management with automatic disposal

### Phase 3: API Updates
- [ ] Update variable access to use `GetVariable()`
- [ ] Replace collection iteration with foreach loops
- [ ] Update type conversion calls to `AtrcConversions`
- [ ] Replace error checking with try-catch blocks

### Phase 4: Optimization
- [ ] Add LINQ queries where appropriate
- [ ] Implement async patterns for file operations
- [ ] Use nullable reference types for better null safety
- [ ] Add XML documentation for IntelliSense

### Phase 5: Testing
- [ ] Port existing unit tests to C# test framework
- [ ] Test with existing ATRC files
- [ ] Verify preprocessor functionality
- [ ] Test platform-specific behavior

## FAQ

### Q: Is the C# version as fast as the C++ version?
A: Performance is comparable for most use cases. The C# version includes optimizations like string interning and lazy loading. For very large files, the C++ version may have a slight advantage.

### Q: Can I use both versions in the same project?
A: Yes, but it's not recommended. The C# version is designed to be a complete replacement.

### Q: Are all preprocessor features supported?
A: Yes, the C# version has 100% feature parity including `#.IF`/`#.ELIF`/`#.ELSE`/`#.ENDIF`, platform detection, and variable substitution.

### Q: How do I handle platform-specific code?
A: Use the same ATRC preprocessor directives. The C# version detects `WINDOWS`, `LINUX`, and `UNIX` just like the C++ version.

### Q: Can I extend the library with custom functions?
A: Yes, you can create extension methods or inherit from the base classes. The C# version is designed to be extensible.

### Q: What about binary compatibility with C++ generated files?
A: ATRC files are text-based, so files created by either version are fully compatible.

### Q: Is async/await supported?
A: Yes, the C# version includes async overloads for file operations:
```csharp
var fileData = await AtrcFileData.LoadAsync("config.atrc", ReadMode.ReadOnly);
```

## Support

For migration assistance or questions:
- Check the API documentation
- Review the sample projects in the `samples/` directory
- File issues on the project repository
- Consult the original C++ documentation for behavior clarification