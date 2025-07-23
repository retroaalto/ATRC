#include <iostream>
#include <ATRC.h>

using namespace atrc;

int main() {
    std::cout << "Starting simple C++ ATRC test...\n";
    
    try {
        // Test 1: Default constructor
        std::cout << "Creating ATRC_FD with default constructor...\n";
        ATRC_FD fd;
        std::cout << "SUCCESS: Default constructor completed\n";
        
        // Test 2: Constructor with file
        std::cout << "Creating ATRC_FD with file constructor...\n";
        ATRC_FD fd2("test.atrc", ATRC_READ_ONLY);
        std::cout << "SUCCESS: File constructor completed\n";
        
        // Test 3: Check status
        std::cout << "Checking file status...\n";
        bool status = fd2.CheckStatus();
        if (status) {
            std::cout << "SUCCESS: File loaded successfully\n";
        } else {
            std::cout << "INFO: File not loaded (may be expected)\n";
        }
        
        // Test 4: Add variable
        std::cout << "Adding a variable...\n";
        bool add_result = fd.AddVariable("TestVar", "TestValue");
        if (add_result) {
            std::cout << "SUCCESS: AddVariable succeeded\n";
        } else {
            std::cout << "INFO: AddVariable failed (may be expected)\n";
        }
        
        std::cout << "Simple C++ test completed\n";
        
    } catch (const std::exception& e) {
        std::cout << "EXCEPTION: " << e.what() << "\n";
        return 1;
    } catch (...) {
        std::cout << "UNKNOWN EXCEPTION occurred\n";
        return 1;
    }
    
    return 0;
}