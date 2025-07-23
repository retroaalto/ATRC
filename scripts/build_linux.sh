#!/bin/bash

# Define the root of your project - automatically detect from script location
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Change to project root to ensure cmake can find presets and other config files
cd "$PROJECT_ROOT"

VERSION=$(< "${PROJECT_ROOT}/project/VERSION")
OUT_DIR="${PROJECT_ROOT}/out"
ATRC_DIR="${PROJECT_ROOT}/ATRC_${VERSION}"

# Initialize error handling
ERROR_ENCOUNTERED=0
ATTEMPT_COUNT=2
counter=${1:-0}

# Array of presets for Linux
declare -a presets=(
    "linux-x64-debug" "linux-x64-release"
    "linux-x86-debug" "linux-x86-release"
)

# Create the ATRC directory structure
mkdir -p "${ATRC_DIR}/Linux/x64/Debug"
mkdir -p "${ATRC_DIR}/Linux/x64/Release"
mkdir -p "${ATRC_DIR}/Linux/x86/Debug"
mkdir -p "${ATRC_DIR}/Linux/x86/Release"

# Loop through presets
for preset in "${presets[@]}"; do
    echo "Building preset: $preset"

    # Determine build type and architecture
    build_type=$( [[ "$preset" == *"debug"* ]] && echo "Debug" || echo "Release" )
    atrc_arch=$( [[ "$preset" == *"x86"* ]] && echo "x86" || echo "x64" )
    toolchain_file="${PROJECT_ROOT}/cmake/toolchain-linux-${atrc_arch}.cmake"
    
    # Generate build files
    command="cmake --preset $preset -B${OUT_DIR}/${preset}/build -DCMAKE_BUILD_TYPE=$build_type -DCMAKE_TOOLCHAIN_FILE=$toolchain_file"
    echo "Running: $command"
    eval $command
    if [ $? -ne 0 ]; then
        echo "Error encountered with: $command"
        ERROR_ENCOUNTERED=1
        break
    fi

    # Build the project
    command="cmake --build ${OUT_DIR}/${preset}/build --config $build_type --parallel"
    echo "Running: $command"
    eval $command
    if [ $? -ne 0 ]; then
        echo "Error encountered with: $command"
        ERROR_ENCOUNTERED=1
        break
    fi

    # Copy built library files to the output directory
    echo "Copying .so files to ${ATRC_DIR}/Linux/${atrc_arch}/${build_type}"

    cp -r "${OUT_DIR}/${preset}/build/ATRC/libATRC.so" "${ATRC_DIR}/Linux/${atrc_arch}/${build_type}/libATRC.so"
done

# Handle build errors
if [ "$ERROR_ENCOUNTERED" -eq 1 ]; then
    echo "Build process failed. Attempt: $counter"
    counter=$((counter + 1))
    if [ "$counter" -lt $ATTEMPT_COUNT ]; then
        echo "Retrying the build process... Attempt: $counter"
        exec "$0" "$counter"
    fi
    echo "Build process failed after $counter attempts."
    exit 1
else
    echo "All builds completed successfully!"
fi
