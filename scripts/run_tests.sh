#!/bin/bash

# Define the root of your project - automatically detect from script location
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Change to project root
cd "$PROJECT_ROOT"

VERSION=$(< "${PROJECT_ROOT}/project/VERSION")
ERROR_ENCOUNTERED=0

echo "=== ATRC Test Runner v${VERSION} ==="
echo "Project root: $PROJECT_ROOT"
echo

# First, build the project if it hasn't been built
echo "Building project first..."
if ! ./scripts/build_linux.sh; then
    echo "❌ Build failed. Cannot run tests."
    exit 1
fi

echo
echo "=== Running Tests ==="

# Array of test configurations to run
declare -a test_configs=(
    "linux-x64-debug"
    "linux-x64-release"
)

# Run tests for each configuration
for config in "${test_configs[@]}"; do
    echo
    echo "--- Testing configuration: $config ---"
    
    TEST_EXECUTABLE="${PROJECT_ROOT}/out/${config}/build/ATRC.Test/ATRC.Test"
    
    if [ ! -f "$TEST_EXECUTABLE" ]; then
        echo "⚠️  Test executable not found: $TEST_EXECUTABLE"
        echo "   Skipping $config tests..."
        continue
    fi
    
    # Make sure the executable is executable
    chmod +x "$TEST_EXECUTABLE"
    
    echo "Running: $TEST_EXECUTABLE"
    
    # Run the test from the test directory (where test.atrc is located)
    TEST_DIR="$(dirname "$TEST_EXECUTABLE")"
    cd "$TEST_DIR"
    
    # Run the test and capture both output and exit code
    if "./$(basename "$TEST_EXECUTABLE")"; then
        echo "✅ Tests passed for $config"
    else
        echo "❌ Tests failed for $config"
        ERROR_ENCOUNTERED=1
    fi
    
    # Return to project root
    cd "$PROJECT_ROOT"
done

echo
echo "=== Test Summary ==="

if [ "$ERROR_ENCOUNTERED" -eq 1 ]; then
    echo "❌ Some tests failed. Check output above for details."
    exit 1
else
    echo "✅ All tests passed successfully!"
    exit 0
fi