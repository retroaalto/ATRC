#include <iostream>
#include <string>
#include <vector>
#include <cassert>
#include <iomanip>
#include <ATRC.h>

using namespace atrc;

// Test result counters
static int tests_passed = 0;
static int tests_failed = 0;
static int total_tests = 0;

// Test macros
#define TEST_START(name) \
    std::cout << "\n--- Starting Test: " << name << " ---\n"; \
    total_tests++;

#define TEST_ASSERT(condition, message) \
    if (condition) { \
        std::cout << "[PASS] " << message << "\n"; \
        tests_passed++; \
    } else { \
        std::cout << "[FAIL] " << message << "\n"; \
        tests_failed++; \
    }

#define TEST_ASSERT_TRUE(condition, message) \
    TEST_ASSERT((condition) == true, message)

#define TEST_ASSERT_FALSE(condition, message) \
    TEST_ASSERT((condition) == false, message)

#define TEST_ASSERT_STR_EQ(expected, actual, message) \
    TEST_ASSERT((actual) == (expected), message)

#define TEST_ASSERT_NOT_EMPTY(str, message) \
    TEST_ASSERT(!(str).empty(), message)

// Test ATRC_FD constructors and destructor
void test_atrc_fd_constructors() {
    TEST_START("ATRC_FD Constructors and Destructor");
    
    // Test default constructor
    try {
        ATRC_FD fd1;
        std::cout << "[PASS] Default constructor completed without exception\n";
        tests_passed++;
    } catch (...) {
        std::cout << "[FAIL] Default constructor threw exception\n";
        tests_failed++;
    }
    
    // Test constructor with string path
    try {
        std::string test_path = "test.atrc";
        ATRC_FD fd2(test_path, ATRC_READ_ONLY);
        
        bool status = fd2.CheckStatus();
        TEST_ASSERT_TRUE(status, "ATRC_FD constructor with string path loads file successfully");
        
        std::string filename = fd2.GetFilename();
        TEST_ASSERT_STR_EQ(test_path, filename, "ATRC_FD stores correct filename");
        
    } catch (...) {
        std::cout << "[FAIL] String constructor threw exception\n";
        tests_failed++;
    }
    
    // Test constructor with const char* path
    try {
        ATRC_FD fd3("test.atrc", ATRC_READ_ONLY);
        
        bool status = fd3.CheckStatus();
        TEST_ASSERT_TRUE(status, "ATRC_FD constructor with const char* path loads file successfully");
        
    } catch (...) {
        std::cout << "[FAIL] const char* constructor threw exception\n";
        tests_failed++;
    }
    
    // Test different read modes
    try {
        ATRC_FD fd4("test.atrc", ATRC_CREATE_READ);
        bool status4 = fd4.CheckStatus();
        TEST_ASSERT_TRUE(status4, "ATRC_FD with ATRC_CREATE_READ mode works");
        
        ATRC_FD fd5("test.atrc", ATRC_FORCE_READ);
        bool status5 = fd5.CheckStatus();
        TEST_ASSERT_TRUE(status5, "ATRC_FD with ATRC_FORCE_READ mode works");
        
    } catch (...) {
        std::cout << "[FAIL] Different read modes threw exception\n";
        tests_failed++;
    }
}

// Test file operations
void test_file_operations() {
    TEST_START("File Operations");
    
    ATRC_FD fd;
    
    // Test reading a file
    std::string test_path = "test.atrc";
    bool read_result = fd.Read(test_path, ATRC_READ_ONLY);
    TEST_ASSERT_TRUE(read_result, "Read method loads file successfully");
    
    if (read_result) {
        bool status = fd.CheckStatus();
        TEST_ASSERT_TRUE(status, "CheckStatus returns true after successful read");
        
        std::string filename = fd.GetFilename();
        TEST_ASSERT_STR_EQ(test_path, filename, "GetFilename returns correct path after Read");
    }
    
    // Test ReadAgain
    bool read_again = fd.ReadAgain(ATRC_READ_ONLY);
    TEST_ASSERT_TRUE(read_again, "ReadAgain method works");
    
    // Test reading non-existent file
    std::string invalid_path = "nonexistent.atrc";
    bool read_invalid = fd.Read(invalid_path, ATRC_READ_ONLY);
    TEST_ASSERT_FALSE(read_invalid, "Reading non-existent file returns false");
}

// Test variable operations
void test_variable_operations() {
    TEST_START("Variable Operations");
    
    ATRC_FD fd("test.atrc", ATRC_READ_ONLY);
    
    if (!fd.CheckStatus()) {
        std::cout << "[FAIL] Could not load test file for variable operations\n";
        tests_failed++;
        return;
    }
    
    // Test reading existing variables
    std::string test_var = fd.ReadVariable("TestVariable");
    TEST_ASSERT_STR_EQ("TestValue", test_var, "ReadVariable returns correct value for TestVariable");
    
    std::string premade_var = fd.ReadVariable("PremadeVariable");
    TEST_ASSERT_STR_EQ("Constant value is", premade_var, "ReadVariable returns correct value for PremadeVariable");
    
    // Test variable existence
    bool exists_test = fd.DoesExistVariable("TestVariable");
    TEST_ASSERT_TRUE(exists_test, "DoesExistVariable returns true for existing variable");
    
    bool exists_nonexistent = fd.DoesExistVariable("NonExistentVariable");
    TEST_ASSERT_FALSE(exists_nonexistent, "DoesExistVariable returns false for non-existent variable");
    
    // Test IsPublic
    bool is_public = fd.IsPublic("TestVariable");
    TEST_ASSERT_TRUE(is_public, "IsPublic returns true for public variable");
    
    // Test adding variables
    bool add_result = fd.AddVariable("NewTestVar", "NewTestValue");
    TEST_ASSERT_TRUE(add_result, "AddVariable succeeds");
    
    if (add_result) {
        bool new_exists = fd.DoesExistVariable("NewTestVar");
        TEST_ASSERT_TRUE(new_exists, "Added variable exists");
        
        std::string new_value = fd.ReadVariable("NewTestVar");
        TEST_ASSERT_STR_EQ("NewTestValue", new_value, "Added variable has correct value");
    }
    
    // Test modifying variables
    bool modify_result = fd.ModifyVariable("NewTestVar", "ModifiedValue");
    TEST_ASSERT_TRUE(modify_result, "ModifyVariable succeeds");
    
    if (modify_result) {
        std::string modified_value = fd.ReadVariable("NewTestVar");
        TEST_ASSERT_STR_EQ("ModifiedValue", modified_value, "Modified variable has correct value");
    }
    
    // Test removing variables
    bool remove_result = fd.RemoveVariable("NewTestVar");
    TEST_ASSERT_TRUE(remove_result, "RemoveVariable succeeds");
    
    if (remove_result) {
        bool removed_exists = fd.DoesExistVariable("NewTestVar");
        TEST_ASSERT_FALSE(removed_exists, "Removed variable no longer exists");
    }
}

// Test block operations
void test_block_operations() {
    TEST_START("Block Operations");
    
    ATRC_FD fd("test.atrc", ATRC_READ_ONLY);
    
    if (!fd.CheckStatus()) {
        std::cout << "[FAIL] Could not load test file for block operations\n";
        tests_failed++;
        return;
    }
    
    // Test existing blocks
    bool exists_varsafety = fd.DoesExistBlock("VAR_SAFETY");
    TEST_ASSERT_TRUE(exists_varsafety, "DoesExistBlock returns true for VAR_SAFETY");
    
    bool exists_premade = fd.DoesExistBlock("PremadeBlock");
    TEST_ASSERT_TRUE(exists_premade, "DoesExistBlock returns true for PremadeBlock");
    
    bool exists_nonexistent = fd.DoesExistBlock("NonExistentBlock");
    TEST_ASSERT_FALSE(exists_nonexistent, "DoesExistBlock returns false for non-existent block");
    
    // Test adding blocks
    bool add_result = fd.AddBlock("TestBlock");
    TEST_ASSERT_TRUE(add_result, "AddBlock succeeds");
    
    if (add_result) {
        bool new_exists = fd.DoesExistBlock("TestBlock");
        TEST_ASSERT_TRUE(new_exists, "Added block exists");
    }
    
    // Test removing blocks
    bool remove_result = fd.RemoveBlock("TestBlock");
    TEST_ASSERT_TRUE(remove_result, "RemoveBlock succeeds");
    
    if (remove_result) {
        bool removed_exists = fd.DoesExistBlock("TestBlock");
        TEST_ASSERT_FALSE(removed_exists, "Removed block no longer exists");
    }
}

// Test key operations
void test_key_operations() {
    TEST_START("Key Operations");
    
    ATRC_FD fd("test.atrc", ATRC_READ_ONLY);
    
    if (!fd.CheckStatus()) {
        std::cout << "[FAIL] Could not load test file for key operations\n";
        tests_failed++;
        return;
    }
    
    // Test reading existing keys
    std::string premade_value = fd.ReadKey("PremadeBlock", "PremadeValue");
    TEST_ASSERT_NOT_EMPTY(premade_value, "ReadKey returns non-empty value for PremadeValue");
    std::cout << "PremadeValue: '" << premade_value << "'\n";
    
    // Test key existence
    bool exists_premade = fd.DoesExistKey("PremadeBlock", "PremadeValue");
    TEST_ASSERT_TRUE(exists_premade, "DoesExistKey returns true for existing key");
    
    bool exists_nonexistent = fd.DoesExistKey("PremadeBlock", "NonExistentKey");
    TEST_ASSERT_FALSE(exists_nonexistent, "DoesExistKey returns false for non-existent key");
    
    // Test adding keys (need to create a block first)
    fd.AddBlock("TestKeyBlock");
    
    bool add_key = fd.AddKey("TestKeyBlock", "TestKey", "TestValue");
    TEST_ASSERT_TRUE(add_key, "AddKey succeeds");
    
    if (add_key) {
        bool key_exists = fd.DoesExistKey("TestKeyBlock", "TestKey");
        TEST_ASSERT_TRUE(key_exists, "Added key exists");
        
        std::string key_value = fd.ReadKey("TestKeyBlock", "TestKey");
        TEST_ASSERT_STR_EQ("TestValue", key_value, "Added key has correct value");
    }
    
    // Test modifying keys
    bool modify_key = fd.ModifyKey("TestKeyBlock", "TestKey", "ModifiedKeyValue");
    TEST_ASSERT_TRUE(modify_key, "ModifyKey succeeds");
    
    if (modify_key) {
        std::string modified_key_value = fd.ReadKey("TestKeyBlock", "TestKey");
        TEST_ASSERT_STR_EQ("ModifiedKeyValue", modified_key_value, "Modified key has correct value");
    }
    
    // Test removing keys
    bool remove_key = fd.RemoveKey("TestKeyBlock", "TestKey");
    TEST_ASSERT_TRUE(remove_key, "RemoveKey succeeds");
    
    if (remove_key) {
        bool key_removed = fd.DoesExistKey("TestKeyBlock", "TestKey");
        TEST_ASSERT_FALSE(key_removed, "Removed key no longer exists");
    }
    
    // Clean up
    fd.RemoveBlock("TestKeyBlock");
}

// Test proxy pattern and operator overloads
void test_proxy_and_operators() {
    TEST_START("Proxy Pattern and Operator Overloads");
    
    ATRC_FD fd("test.atrc", ATRC_READ_ONLY);
    
    if (!fd.CheckStatus()) {
        std::cout << "[FAIL] Could not load test file for proxy tests\n";
        tests_failed++;
        return;
    }
    
    // Test operator[] for variables
    try {
        std::string var_value = fd["TestVariable"];
        TEST_ASSERT_STR_EQ("TestValue", var_value, "operator[] returns correct variable value");
    } catch (...) {
        std::cout << "[FAIL] operator[] for variables threw exception\n";
        tests_failed++;
    }
    
    // Test operator[] for keys (block]key format)
    try {
        std::string key_value = fd["PremadeBlock]PremadeValue"];
        TEST_ASSERT_NOT_EMPTY(key_value, "operator[] returns non-empty value for key");
        std::cout << "Block key via operator[]: '" << key_value << "'\n";
    } catch (...) {
        std::cout << "[FAIL] operator[] for keys threw exception\n";
        tests_failed++;
    }
    
    // Test assignment operator
    try {
        fd.AddVariable("AssignTestVar", "OriginalValue");
        fd["AssignTestVar"] = "AssignedValue";
        
        std::string assigned_value = fd.ReadVariable("AssignTestVar");
        TEST_ASSERT_STR_EQ("AssignedValue", assigned_value, "Assignment operator works for variables");
        
        fd.RemoveVariable("AssignTestVar");
    } catch (...) {
        std::cout << "[FAIL] Assignment operator threw exception\n";
        tests_failed++;
    }
}

// Test variable substitution and injection
void test_variable_substitution() {
    TEST_START("Variable Substitution and Injection");
    
    ATRC_FD fd("test.atrc", ATRC_READ_ONLY);
    
    if (!fd.CheckStatus()) {
        std::cout << "[FAIL] Could not load test file for substitution tests\n";
        tests_failed++;
        return;
    }
    
    // Test reading key with variable substitution
    std::string premade_value = fd.ReadKey("PremadeBlock", "PremadeValue");
    TEST_ASSERT_NOT_EMPTY(premade_value, "Key with variable substitution returns non-empty value");
    std::cout << "Variable substitution result: '" << premade_value << "'\n";
    
    // Test InsertVar_S method
    try {
        std::string template_str = "Hello %*%, value is %*%";
        std::vector<std::string> args = {"World", "42"};
        std::string result = fd.InsertVar_S(template_str, args);
        
        TEST_ASSERT_STR_EQ("Hello World, value is 42", result, "InsertVar_S produces correct result");
    } catch (...) {
        std::cout << "[FAIL] InsertVar_S threw exception\n";
        tests_failed++;
    }
    
    // Test InsertVar method (void version)
    try {
        std::string template_str = "Test %*% substitution %*%";
        std::vector<std::string> args = {"variable", "works"};
        fd.InsertVar(template_str, args);
        
        TEST_ASSERT_STR_EQ("Test variable substitution works", template_str, "InsertVar modifies string correctly");
    } catch (...) {
        std::cout << "[FAIL] InsertVar threw exception\n";
        tests_failed++;
    }
}

// Test properties and settings
void test_properties_and_settings() {
    TEST_START("Properties and Settings");
    
    ATRC_FD fd("test.atrc", ATRC_READ_ONLY);
    
    if (!fd.CheckStatus()) {
        std::cout << "[FAIL] Could not load test file for properties tests\n";
        tests_failed++;
        return;
    }
    
    // Test AutoSave property
    bool original_autosave = fd.GetAutoSave();
    fd.SetAutoSave(true);
    bool new_autosave = fd.GetAutoSave();
    TEST_ASSERT_TRUE(new_autosave, "SetAutoSave(true) sets AutoSave to true");
    
    fd.SetAutoSave(false);
    bool turned_off_autosave = fd.GetAutoSave();
    TEST_ASSERT_FALSE(turned_off_autosave, "SetAutoSave(false) sets AutoSave to false");
    
    // Restore original setting
    fd.SetAutoSave(original_autosave);
    
    // Test WriteCheck property
    bool original_writecheck = fd.GetWriteCheck();
    fd.SetWriteCheck(true);
    bool new_writecheck = fd.GetWriteCheck();
    TEST_ASSERT_TRUE(new_writecheck, "SetWriteCheck(true) sets WriteCheck to true");
    
    fd.SetWriteCheck(false);
    bool turned_off_writecheck = fd.GetWriteCheck();
    TEST_ASSERT_FALSE(turned_off_writecheck, "SetWriteCheck(false) sets WriteCheck to false");
    
    // Restore original setting
    fd.SetWriteCheck(original_writecheck);
}

// Test data access methods
void test_data_access() {
    TEST_START("Data Access Methods");
    
    ATRC_FD fd("test.atrc", ATRC_READ_ONLY);
    
    if (!fd.CheckStatus()) {
        std::cout << "[FAIL] Could not load test file for data access tests\n";
        tests_failed++;
        return;
    }
    
    // Test GetVariables
    try {
        std::vector<Variable> variables = fd.GetVariables();
        TEST_ASSERT(variables.size() > 0, "GetVariables returns non-empty vector");
        
        // Check if we can find our test variables
        bool found_test_var = false;
        for (const auto& var : variables) {
            if (var.Name == "TestVariable") {
                found_test_var = true;
                TEST_ASSERT_STR_EQ("TestValue", var.Value, "GetVariables returns correct variable data");
                TEST_ASSERT_TRUE(var.IsPublic, "GetVariables returns correct IsPublic flag");
                break;
            }
        }
        TEST_ASSERT_TRUE(found_test_var, "GetVariables contains expected test variable");
    } catch (...) {
        std::cout << "[FAIL] GetVariables threw exception\n";
        tests_failed++;
    }
    
    // Test GetBlocks
    try {
        std::vector<Block>* blocks = fd.GetBlocks();
        TEST_ASSERT(blocks != nullptr, "GetBlocks returns non-null pointer");
        
        if (blocks) {
            TEST_ASSERT(blocks->size() > 0, "GetBlocks returns non-empty vector");
            
            // Check if we can find our test blocks
            bool found_premade_block = false;
            for (const auto& block : *blocks) {
                if (block.Name == "PremadeBlock") {
                    found_premade_block = true;
                    TEST_ASSERT(block.Keys.size() > 0, "PremadeBlock contains keys");
                    break;
                }
            }
            TEST_ASSERT_TRUE(found_premade_block, "GetBlocks contains expected test block");
        }
    } catch (...) {
        std::cout << "[FAIL] GetBlocks threw exception\n";
        tests_failed++;
    }
    
    // Test GetFilename
    std::string filename = fd.GetFilename();
    TEST_ASSERT_STR_EQ("test.atrc", filename, "GetFilename returns correct filename");
}

// Test comment operations
void test_comment_operations() {
    TEST_START("Comment Operations");
    
    ATRC_FD fd("test.atrc", ATRC_READ_ONLY);
    
    if (!fd.CheckStatus()) {
        std::cout << "[FAIL] Could not load test file for comment tests\n";
        tests_failed++;
        return;
    }
    
    // Test writing comments
    bool write_top = fd.WriteCommentToTop("This is a top comment");
    TEST_ASSERT_TRUE(write_top, "WriteCommentToTop succeeds");
    
    bool write_bottom = fd.WriteCommentToBottom("This is a bottom comment");
    TEST_ASSERT_TRUE(write_bottom, "WriteCommentToBottom succeeds");
}

// Test C++ standard library integration
void test_stdlib_integration() {
    TEST_START("Standard Library Integration");
    
    ATRC_FD fd("test.atrc", ATRC_READ_ONLY);
    
    if (!fd.CheckStatus()) {
        std::cout << "[FAIL] Could not load test file for stdlib tests\n";
        tests_failed++;
        return;
    }
    
    // Test atrc_to_vector function
    try {
        std::vector<std::string> result = atrc_std::atrc_to_vector(',', "item1,item2,item3");
        TEST_ASSERT(result.size() == 3, "atrc_to_vector returns correct number of items");
        
        if (result.size() >= 3) {
            TEST_ASSERT_STR_EQ("item1", result[0], "atrc_to_vector first item correct");
            TEST_ASSERT_STR_EQ("item2", result[1], "atrc_to_vector second item correct");
            TEST_ASSERT_STR_EQ("item3", result[2], "atrc_to_vector third item correct");
        }
    } catch (...) {
        std::cout << "[FAIL] atrc_to_vector threw exception\n";
        tests_failed++;
    }
}

void print_test_summary() {
    std::cout << "\n\n=== TEST SUMMARY ===\n";
    std::cout << "Total tests run: " << total_tests << "\n";
    std::cout << "Tests passed: " << tests_passed << "\n";
    std::cout << "Tests failed: " << tests_failed << "\n";
    std::cout << "Success rate: " << std::fixed << std::setprecision(2) 
              << (total_tests > 0 ? (tests_passed * 100.0 / total_tests) : 0.0) << "%\n";
    
    if (tests_failed == 0) {
        std::cout << "🎉 ALL TESTS PASSED! 🎉\n";
    } else {
        std::cout << "❌ Some tests failed. Please review the output above.\n";
    }
}

int main() {
    std::cout << "=== ATRC C++ API Comprehensive Test Suite ===\n";
    
    // Run all test suites
    test_atrc_fd_constructors();
    test_file_operations();
    test_variable_operations();
    test_block_operations();
    test_key_operations();
    test_proxy_and_operators();
    test_variable_substitution();
    test_properties_and_settings();
    test_data_access();
    test_comment_operations();
    test_stdlib_integration();
    
    print_test_summary();
    
    return (tests_failed == 0) ? 0 : 1;
}