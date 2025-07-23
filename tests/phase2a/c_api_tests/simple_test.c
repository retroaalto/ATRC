#include <stdio.h>
#include <stdlib.h>
#include <ATRC.h>

int main() {
    printf("Starting simple ATRC test...\n");
    
    // Test 1: Create empty ATRC_FD
    printf("Creating empty ATRC_FD...\n");
    C_PATRC_FD fd = Create_Empty_ATRC_FD();
    if (fd == NULL) {
        printf("FAIL: Create_Empty_ATRC_FD returned NULL\n");
        return 1;
    }
    printf("SUCCESS: Create_Empty_ATRC_FD returned non-NULL\n");
    
    // Test 2: Try to add a variable
    printf("Adding a test variable...\n");
    bool add_result = AddVariable(fd, "TestVar", "TestValue");
    if (add_result) {
        printf("SUCCESS: AddVariable succeeded\n");
    } else {
        printf("FAIL: AddVariable failed\n");
    }
    
    // Test 3: Try to read the variable
    printf("Reading the test variable...\n");
    const char* value = ReadVariable(fd, "TestVar");
    if (value != NULL) {
        printf("SUCCESS: ReadVariable returned '%s'\n", value);
    } else {
        printf("FAIL: ReadVariable returned NULL\n");
    }
    
    // Test 4: Clean up
    printf("Destroying ATRC_FD...\n");
    Destroy_ATRC_FD(fd);
    printf("SUCCESS: Test completed\n");
    
    return 0;
}