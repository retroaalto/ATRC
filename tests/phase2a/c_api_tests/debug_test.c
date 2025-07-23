#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <ATRC.h>

int main() {
    // Print current working directory
    char cwd[1024];
    if (getcwd(cwd, sizeof(cwd)) != NULL) {
        printf("Current working directory: %s\n", cwd);
    }
    
    // Check if test file exists
    const char* test_file = "./tests/phase2a/test_data/basic_test.atrc";
    FILE* f = fopen(test_file, "r");
    if (f) {
        printf("Test file exists: %s\n", test_file);
        fclose(f);
    } else {
        printf("Test file does not exist: %s\n", test_file);
        return 1;
    }
    
    // Try to create ATRC_FD with the file
    printf("Attempting to create ATRC_FD with file...\n");
    C_PATRC_FD fd = Create_ATRC_FD((char*)test_file, ATRC_READ_ONLY);
    if (fd) {
        printf("SUCCESS: Create_ATRC_FD succeeded\n");
        Destroy_ATRC_FD(fd);
    } else {
        printf("FAIL: Create_ATRC_FD failed\n");
        return 1;
    }
    
    return 0;
}