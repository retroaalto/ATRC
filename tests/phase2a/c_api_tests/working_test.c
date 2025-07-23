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
    
    // Try with the working test.atrc file
    const char* test_file = "test.atrc";
    printf("Attempting to create ATRC_FD with file: %s\n", test_file);
    
    C_PATRC_FD fd = Create_ATRC_FD((char*)test_file, ATRC_READ_ONLY);
    if (fd) {
        printf("SUCCESS: Create_ATRC_FD succeeded\n");
        
        // Try to read a variable from the working file
        const char* test_var = ReadVariable(fd, "TestVariable");
        if (test_var) {
            printf("SUCCESS: ReadVariable returned '%s'\n", test_var);
        } else {
            printf("INFO: ReadVariable returned NULL (no TestVariable)\n");
        }
        
        // Try to read a variable that should exist
        const char* premade_var = ReadVariable(fd, "PremadeVariable");
        if (premade_var) {
            printf("SUCCESS: ReadVariable PremadeVariable returned '%s'\n", premade_var);
        } else {
            printf("INFO: ReadVariable PremadeVariable returned NULL\n");
        }
        
        Destroy_ATRC_FD(fd);
        printf("SUCCESS: Test completed\n");
    } else {
        printf("FAIL: Create_ATRC_FD failed\n");
        return 1;
    }
    
    return 0;
}