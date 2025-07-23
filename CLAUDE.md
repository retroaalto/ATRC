# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ATRC is a C/C++ resource/configuration file library written in C++17 with a C wrapper. It processes ATRC files which are configuration files with preprocessing directives similar to C preprocessor but with variable substitution and platform-specific conditionals.

## Architecture

The project follows a standard CMake-based C/C++ library structure:

- **Core Library (`ATRC/`)**: Contains the main library implementation
  - `ATRC.h`: Main header with C/C++ API definitions
  - `core_dev.cpp`: Core C++ implementation
  - `c.c` and `c_cpp.cpp`: C wrapper implementation
  - `filehandler2.cpp`: File I/O operations
  - `ATRCFiledata.cpp`: Data structure management
  - `atrc_stdlib.cpp`: Standard library functions

- **Test Suite (`ATRC.Test/`)**: Contains test executables and test ATRC files
  - Tests are implemented in both C and C++ to verify both APIs
  - `test.atrc`: Sample configuration file for testing

- **Build System**: Multi-platform CMake configuration with presets for Windows, Linux, and macOS

## Key Components

1. **ATRC_FD Class**: Main C++ interface for reading/writing ATRC files
2. **C Wrapper Functions**: Provides C API for the library
3. **Preprocessor System**: Handles variables, conditionals, and platform detection
4. **Cross-platform Support**: Windows (Visual Studio), Linux (GCC), macOS builds

## Common Development Commands

### Building the Project

**Linux (current platform):**
```bash
# Configure and build using preset
cmake --preset linux-x64-debug -B./out/linux-x64-debug/build -DCMAKE_BUILD_TYPE=Debug -DCMAKE_TOOLCHAIN_FILE=./cmake/toolchain-linux-x64.cmake -DATRC_BUILD_TESTS=ON
cmake --build ./out/linux-x64-debug/build --config Debug

# Or use the build script
./scripts/build_linux.sh
```

**Windows:**
```cmd
.\vs_run.bat
```

### Running Tests

**Linux:**
```bash
# Build first, then run the test executable
./out/linux-x64-debug/build/ATRC.Test/ATRC.Test
```

**Windows:**
```cmd
.\scripts\run_test.bat
```

### CMake Build Options

- `ATRC_BUILD_TESTS=ON/OFF`: Enable/disable building tests (default: ON)
- Available presets: `linux-x64-debug`, `linux-x64-release`, `windows-x64-debug`, `windows-x64-release`, etc.

## ATRC File Format

ATRC files use a custom configuration format with:
- Variables: `%variable%=value`
- Blocks: `[BlockName]`
- Keys: `KeyName=Value`
- Preprocessor directives: `#.IF`, `#.ELIF`, `#.ELSE`, `#.ENDIF`, `#.ERROR`
- Platform detection: `WINDOWS`, `LINUX`, `UNIX`
- Time injection: `%*%` (outputs current time percentage)

## Development Notes

- The library is built as a shared library (DLL/SO)
- C++17 standard is required
- The project uses CMake presets for cross-platform builds
- Test files include both C and C++ implementations to verify both APIs work correctly
- The library handles file I/O, parsing, and provides both C and C++ interfaces

## File Locations

- Main header: `ATRC/include/ATRC.h`
- Test files: `ATRC.Test/test.atrc` and `test.atrc` (root)
- Build output: `out/` directory (gitignored)
- Version info: `project/VERSION`
- Documentation: `docs/index.html`