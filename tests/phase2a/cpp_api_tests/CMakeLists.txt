cmake_minimum_required(VERSION 3.10)

# Add the simple C++ test executable
add_executable(simple_cpp_test simple_cpp_test.cpp)

# Include directories
target_include_directories(simple_cpp_test PRIVATE
    ${CMAKE_SOURCE_DIR}/ATRC/include
)

# Link against the ATRC library
target_link_libraries(simple_cpp_test ATRC)

# Set C++ standard
set_property(TARGET simple_cpp_test PROPERTY CXX_STANDARD 17)

# Add the C++ API comprehensive test executable
add_executable(cpp_api_comprehensive_test cpp_api_comprehensive_test.cpp)

# Include directories
target_include_directories(cpp_api_comprehensive_test PRIVATE
    ${CMAKE_SOURCE_DIR}/ATRC/include
)

# Link against the ATRC library
target_link_libraries(cpp_api_comprehensive_test ATRC)

# Set C++ standard
set_property(TARGET cpp_api_comprehensive_test PROPERTY CXX_STANDARD 17)

# Add test to CTest if available
if(BUILD_TESTING)
    add_test(NAME simple_cpp_test COMMAND simple_cpp_test)
    add_test(NAME cpp_api_comprehensive_test COMMAND cpp_api_comprehensive_test)
endif()