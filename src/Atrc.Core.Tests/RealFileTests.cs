using System.IO;
using System.Threading.Tasks;
using Atrc.Core.Models;
using Xunit;

namespace Atrc.Core.Tests
{
    public class RealFileTests
    {
        [Fact]
        public async Task LoadAsync_ShouldParseRealAtrcFile()
        {
            // Arrange
            var testFilePath = Path.Combine("..", "..", "..", "..", "..", "tests", "phase2a", "test_data", "basic_test.atrc");
            var absolutePath = Path.GetFullPath(testFilePath);
            
            // Skip test if file doesn't exist (running in different environment)
            if (!File.Exists(absolutePath))
            {
                // Try alternative path
                testFilePath = Path.Combine("..", "..", "..", "..", "..", "test.atrc");
                absolutePath = Path.GetFullPath(testFilePath);
                
                if (!File.Exists(absolutePath))
                {
                    return; // Skip test
                }
            }

            // Act
            using var fileData = await AtrcFileData.LoadAsync(absolutePath, ReadMode.ReadOnly);

            // Assert
            Assert.NotNull(fileData);
            Assert.NotEmpty(fileData.Variables);
            Assert.Equal(absolutePath, fileData.Filename);
        }

        [Fact]
        public async Task CreateAndReadMode_ShouldWorkWithNewFile()
        {
            // Arrange
            var tempFile = Path.GetTempPath() + "test_atrc_" + System.Guid.NewGuid().ToString() + ".atrc";

            try
            {
                // Act - Create a new file and add some content
                using var fileData = await AtrcFileData.LoadAsync(tempFile, ReadMode.CreateAndRead);
                
                fileData.AddVariable("TestVar", "TestValue");
                fileData.AddBlock("TestBlock");
                fileData.AddKey("TestBlock", "TestKey", "TestKeyValue");

                // Assert
                Assert.Equal("TestValue", fileData["TestVar"]);
                Assert.Equal("TestKeyValue", fileData["TestBlock.TestKey"]);
                Assert.True(fileData.DoesExistBlock("TestBlock"));
                Assert.True(fileData.DoesExistVariable("TestVar"));
                Assert.True(fileData.DoesExistKey("TestBlock", "TestKey"));
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}