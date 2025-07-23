cmake_minimum_required(VERSION 3.10)

# Add the simple test executable
add_executable(simple_test simple_test.c)

# Include directories
target_include_directories(simple_test PRIVATE
    ${CMAKE_SOURCE_DIR}/ATRC/include
)

# Link against the ATRC library
target_link_libraries(simple_test ATRC)

# Set C standard
set_property(TARGET simple_test PROPERTY C_STANDARD 11)

# Add the C API comprehensive test executable
add_executable(c_api_comprehensive_test c_api_comprehensive_test.c)

# Include directories
target_include_directories(c_api_comprehensive_test PRIVATE
    ${CMAKE_SOURCE_DIR}/ATRC/include
)

# Link against the ATRC library
target_link_libraries(c_api_comprehensive_test ATRC)

# Set C standard
set_property(TARGET c_api_comprehensive_test PROPERTY C_STANDARD 11)

# Add the debug test executable
add_executable(debug_test debug_test.c)

# Include directories
target_include_directories(debug_test PRIVATE
    ${CMAKE_SOURCE_DIR}/ATRC/include
)

# Link against the ATRC library
target_link_libraries(debug_test ATRC)

# Set C standard
set_property(TARGET debug_test PROPERTY C_STANDARD 11)

# Add the working test executable
add_executable(working_test working_test.c)

# Include directories
target_include_directories(working_test PRIVATE
    ${CMAKE_SOURCE_DIR}/ATRC/include
)

# Link against the ATRC library
target_link_libraries(working_test ATRC)

# Set C standard
set_property(TARGET working_test PROPERTY C_STANDARD 11)

# Add test to CTest if available
if(BUILD_TESTING)
    add_test(NAME simple_test COMMAND simple_test)
    add_test(NAME debug_test COMMAND debug_test)
    add_test(NAME c_api_comprehensive_test COMMAND c_api_comprehensive_test)
endif()