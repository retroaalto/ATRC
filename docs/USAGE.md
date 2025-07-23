# ATRC Configuration File Usage Guide

## Table of Contents

1. [Introduction](#introduction)
2. [File Format Basics](#file-format-basics)
3. [Variables](#variables)
4. [Blocks and Keys](#blocks-and-keys)
5. [Preprocessor Directives](#preprocessor-directives)
6. [Platform Detection](#platform-detection)
7. [Architecture Support](#architecture-support)
8. [Dynamic Value Injection](#dynamic-value-injection)
9. [Advanced Features](#advanced-features)
10. [Practical Examples](#practical-examples)
11. [Best Practices](#best-practices)
12. [Error Handling](#error-handling)

## Introduction

ATRC (A Text Resource Configuration) is a powerful configuration file format designed for C/C++ applications. It provides a simple yet flexible syntax for managing application settings, supporting variable substitution, conditional compilation, platform detection, and dynamic value injection.

ATRC files are processed by the ATRC library, which offers both C and C++ APIs for reading, writing, and manipulating configuration data.

## File Format Basics

### File Signature

Every ATRC file must begin with the signature:
```
#!ATRC
```

This signature identifies the file as an ATRC configuration file and is required for proper parsing.

### Syntax Rules

- **Comments**: Lines starting with `#` (except preprocessor directives) are treated as comments
- **Case Sensitivity**: All identifiers (variable names, block names, keys) are case-sensitive
- **Whitespace**: Leading and trailing whitespace is generally ignored
- **Line Endings**: Both Unix (LF) and Windows (CRLF) line endings are supported

### Basic Structure

```ini
#!ATRC
# This is a comment
%variable_name%=value
[BlockName]
key=value
```

## Variables

Variables in ATRC are global and can be defined anywhere in the file. They use percent signs as delimiters.

### Variable Definition

```ini
%variable_name%=value
```

### Variable Substitution

Variables can reference other variables:

```ini
%username%=admin
%domain%=example.com
%email%=%username%@%domain%
# Result: email = "admin@example.com"
```

### Variable Scope

- Variables are global and can be accessed from anywhere in the file
- Variables can be marked as public or private (affects C API visibility)
- Variable substitution is recursive and happens at parse time

### Special Variables

#### Time Injection (%*%)
The special variable `%*%` is replaced with dynamic content when used with the injection system:

```ini
%message%=Loading progress: %*%
# When injected with a value, becomes: "Loading progress: 50%"
```

#### Advanced Injection (%*N%)
Indexed injection allows referencing specific arguments:

```ini
%template%=User %*1% has %*2% permissions
# When injected with ["john", "admin"], becomes: "User john has admin permissions"
```

## Blocks and Keys

Blocks provide organization and namespacing for configuration data.

### Block Definition

```ini
[BlockName]
key1=value1
key2=value2
```

### Key-Value Pairs

Within blocks, you define key-value pairs:

```ini
[DatabaseConfig]
host=localhost
port=5432
database=myapp
user=%db_username%
password=%db_password%
```

### Accessing Data

Using the C++ API:
```cpp
ATRC_FD config("config.atrc");
std::string host = config.ReadKey("DatabaseConfig", "host");
// or using operator overloading:
std::string host = config["DatabaseConfig"]["host"];
```

## Preprocessor Directives

ATRC supports conditional compilation through preprocessor directives.

### Conditional Blocks

```ini
#.IF WINDOWS
WindowsSpecificKey=value
#.ELIF LINUX
LinuxSpecificKey=value
#.ELSE
DefaultKey=value
#.ENDIF
```

### Supported Directives

- `#.IF condition` - Start conditional block
- `#.ELIF condition` - Alternative condition
- `#.ELSE` - Default case
- `#.ENDIF` - End conditional block
- `#.ERROR message` - Log error and potentially halt processing

### Logical Operations

Conditions support boolean logic:

```ini
#.IF LINUX OR UNIX
UnixLikeSystem=true
#.ENDIF

#.IF X64 AND WINDOWS
Windows64Config=enabled
#.ENDIF

#.IF NOT WINDOWS
NonWindowsConfig=true
#.ENDIF
```

## Platform Detection

ATRC automatically detects the target platform and makes these tags available:

### Operating System Tags

- `WINDOWS` - Microsoft Windows
- `LINUX` - Linux distributions
- `UNIX` - Unix-like systems (includes Linux)
- `MACOS` - macOS (Apple)

### Usage Example

```ini
[PlatformSpecific]
#.IF WINDOWS
LibraryPath=./lib/windows/
FileExtension=.dll
#.ELIF LINUX
LibraryPath=./lib/linux/
FileExtension=.so
#.ELIF MACOS
LibraryPath=./lib/macos/
FileExtension=.dylib
#.ELSE
#.ERROR Unsupported platform
#.ENDIF
```

## Architecture Support

ATRC provides architecture detection for platform-specific configurations:

### Architecture Tags

- `X86` - 32-bit x86 architecture
- `X64` - 64-bit x86-64 architecture
- `ARM` - ARM architecture (32-bit)
- `ARM64` - ARM64 architecture (64-bit)

### Architecture-Specific Configuration

```ini
[PerformanceSettings]
#.IF X64
ThreadPoolSize=16
MaxMemoryMB=8192
#.ELIF X86
ThreadPoolSize=8
MaxMemoryMB=2048
#.ENDIF
```

## Dynamic Value Injection

The injection system allows runtime substitution of values into configuration strings.

### Basic Injection

```ini
%template%=Hello %*%, welcome to %*%!
```

In C++:
```cpp
std::string result;
std::vector<std::string> args = {"John", "ATRC"};
ATRC_INJECT(config, config.ReadVariable("template"), "John", "ATRC");
// Result: "Hello John, welcome to ATRC!"
```

### Indexed Injection

For more control over argument positioning:

```ini
%greeting%=Welcome %*1%, you have %*2% messages and %*3% notifications
```

### Time-Based Injection

The special `%*%` can represent time or percentage values:

```ini
WelcomeMessage=Hi %username%, loading at: %*%\%&
# Could become: "Hi admin, loading at: 75% complete"
```

## Advanced Features

### Complex Conditional Logic

```ini
#.IF (WINDOWS AND X64) OR (LINUX AND ARM64)
OptimizedBinary=fast_path
#.ELSE
OptimizedBinary=standard_path
#.ENDIF
```

### Variable Visibility

Variables can be marked as public or private:

```ini
%public_var%=visible_to_c_api
%private_var%=hidden_from_c_api
```

### Error Handling

Use `#.ERROR` for validation:

```ini
#.IF NOT (WINDOWS OR LINUX OR MACOS)
#.ERROR Unsupported operating system detected
#.ENDIF
```

## Practical Examples

### Application Configuration

```ini
#!ATRC
# Application Configuration Example

# Environment variables
%app_name%=MyApplication
%version%=2.1.0
%debug_mode%=false

[Database]
#.IF DEBUG
host=localhost
port=5432
database=%app_name%_dev
#.ELSE
host=prod-db.example.com
port=5432  
database=%app_name%_prod
#.ENDIF
username=%db_user%
password=%db_pass%

[Logging]
#.IF DEBUG
level=debug
file=./logs/%app_name%_debug.log
#.ELSE
level=info
file=/var/log/%app_name%.log
#.ENDIF

[Platform]
#.IF WINDOWS
data_dir=%APPDATA%\%app_name%
temp_dir=%TEMP%
#.ELIF LINUX OR UNIX
data_dir=~/.%app_name%
temp_dir=/tmp
#.ENDIF

[Performance]
#.IF X64
max_threads=16
buffer_size=8192
#.ELSE
max_threads=8
buffer_size=4096
#.ENDIF
```

### Game Configuration

```ini
#!ATRC
# Game Settings Example

%game_title%=Epic Adventure
%player_name%=Player1

[Graphics]
#.IF WINDOWS AND X64
renderer=DirectX12
texture_quality=Ultra
#.ELIF LINUX
renderer=Vulkan
texture_quality=High
#.ELSE
renderer=OpenGL
texture_quality=Medium
#.ENDIF

resolution=1920x1080
fullscreen=true

[Audio]
master_volume=80
music_volume=70
sfx_volume=90

[Controls]
#.IF WINDOWS
use_xinput=true
#.ELSE
use_xinput=false
#.ENDIF

[SaveData]
save_path=%player_name%_save.dat
backup_path=%player_name%_backup.dat
autosave_interval=300

[Messages]
welcome=Welcome to %game_title%, %player_name%!
loading=Loading %*%... Please wait.
progress=Progress: %*% complete (%*%/%*% items)
```

## Best Practices

### Organization

1. **Group related settings** into logical blocks
2. **Use descriptive names** for variables and keys
3. **Place global variables** at the top of the file
4. **Document complex logic** with comments

### Variable Management

1. **Define variables before use** when possible
2. **Use consistent naming conventions** (e.g., snake_case)
3. **Avoid circular references** in variable substitution
4. **Keep variable scope** as narrow as practical

### Conditional Logic

1. **Test all conditional paths** during development
2. **Provide fallbacks** with `#.ELSE` when appropriate
3. **Use `#.ERROR`** to catch unsupported configurations
4. **Keep conditions simple** and readable

### Performance

1. **Minimize deep variable nesting** to avoid excessive recursion
2. **Cache frequently accessed values** in your application
3. **Use appropriate ReadMode** settings based on your use case

## Error Handling

### Common Issues

#### Missing File Signature
```
Error: File must start with #!ATRC
```
**Solution**: Ensure the first line of your file is exactly `#!ATRC`

#### Circular Variable References
```ini
# WRONG:
%var1%=%var2%
%var2%=%var1%
```
**Solution**: Avoid variables that reference each other

#### Unclosed Conditional Blocks
```ini
# WRONG:
#.IF WINDOWS
SomeKey=value
# Missing #.ENDIF
```
**Solution**: Every `#.IF` must have a corresponding `#.ENDIF`

#### Invalid Platform Tags
```ini
# WRONG:
#.IF INVALID_PLATFORM
```
**Solution**: Use only supported platform tags (WINDOWS, LINUX, UNIX, MACOS)

### Debugging Tips

1. **Use `#.ERROR`** to validate assumptions
2. **Test with different platforms** if using conditionals
3. **Check variable substitution** by reading variables after loading
4. **Validate file structure** with a simple parser first

### Error Recovery

The ATRC library provides status checking:

```cpp
ATRC_FD config("config.atrc");
if (!config.CheckStatus()) {
    std::cerr << "Failed to load configuration" << std::endl;
    // Handle error appropriately
}
```

---

This documentation covers the comprehensive usage of ATRC configuration files. For library API documentation and integration examples, refer to the main project documentation and header files.