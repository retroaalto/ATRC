cmake_minimum_required(VERSION 3.10)

# Phase 2A test suites
project(ATRC_Phase2A_Tests)

# Enable testing
enable_testing()

# Add subdirectories for different test suites
add_subdirectory(c_api_tests)
add_subdirectory(cpp_api_tests)
# add_subdirectory(format_tests)
# add_subdirectory(preprocessor_tests)
# add_subdirectory(performance_tests)