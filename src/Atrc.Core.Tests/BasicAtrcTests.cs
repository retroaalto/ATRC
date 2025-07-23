using System;
using System.IO;
using System.Threading.Tasks;
using Atrc.Core.Models;
using Atrc.Core.Parsing;
using Xunit;

namespace Atrc.Core.Tests
{
    public class BasicAtrcTests
    {
        [Fact]
        public void CreateEmpty_ShouldCreateEmptyAtrcFileData()
        {
            // Arrange & Act
            using var fileData = AtrcFileData.CreateEmpty();

            // Assert
            Assert.NotNull(fileData);
            Assert.Empty(fileData.Variables);
            Assert.Empty(fileData.Blocks);
            Assert.Null(fileData.Filename);
            Assert.False(fileData.AutoSave);
            Assert.True(fileData.WriteCheck);
        }

        [Fact]
        public void AddVariable_ShouldAddVariableSuccessfully()
        {
            // Arrange
            using var fileData = AtrcFileData.CreateEmpty();

            // Act
            var result = fileData.AddVariable("TestVar", "TestValue", true);

            // Assert
            Assert.True(result);
            Assert.Single(fileData.Variables);
            Assert.Equal("TestValue", fileData.ReadVariable("TestVar"));
            Assert.True(fileData.IsPublic("TestVar"));
        }

        [Fact]
        public void AddBlock_ShouldAddBlockSuccessfully()
        {
            // Arrange
            using var fileData = AtrcFileData.CreateEmpty();

            // Act
            var result = fileData.AddBlock("TestBlock");

            // Assert
            Assert.True(result);
            Assert.Single(fileData.Blocks);
            Assert.True(fileData.DoesExistBlock("TestBlock"));
        }

        [Fact]
        public void AddKey_ShouldAddKeySuccessfully()
        {
            // Arrange
            using var fileData = AtrcFileData.CreateEmpty();
            fileData.AddBlock("TestBlock");

            // Act
            var result = fileData.AddKey("TestBlock", "TestKey", "TestValue");

            // Assert
            Assert.True(result);
            Assert.Equal("TestValue", fileData.ReadKey("TestBlock", "TestKey"));
            Assert.True(fileData.DoesExistKey("TestBlock", "TestKey"));
        }

        [Fact]
        public void IndexerAccess_ShouldWorkForVariables()
        {
            // Arrange
            using var fileData = AtrcFileData.CreateEmpty();
            fileData.AddVariable("TestVar", "OriginalValue");

            // Act & Assert
            Assert.Equal("OriginalValue", fileData["TestVar"]);
            
            fileData["TestVar"] = "NewValue";
            Assert.Equal("NewValue", fileData["TestVar"]);
        }

        [Fact]
        public void IndexerAccess_ShouldWorkForKeys()
        {
            // Arrange
            using var fileData = AtrcFileData.CreateEmpty();
            fileData.AddBlock("TestBlock");
            fileData.AddKey("TestBlock", "TestKey", "OriginalValue");

            // Act & Assert
            Assert.Equal("OriginalValue", fileData["TestBlock.TestKey"]);
            Assert.Equal("OriginalValue", fileData["TestBlock", "TestKey"]);
            
            fileData["TestBlock", "TestKey"] = "NewValue";
            Assert.Equal("NewValue", fileData["TestBlock", "TestKey"]);
        }

        [Fact]
        public void ParseContent_ShouldParseBasicAtrcContent()
        {
            // Arrange
            const string content = @"
#!ATRC
# Test comment
%GlobalVar%=GlobalValue
%NumericVar%=12345

[TestBlock]
SimpleKey=SimpleValue
NumericKey=42
";

            using var fileData = AtrcFileData.CreateEmpty();
            var parser = new AtrcParser();

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            Assert.Equal(2, fileData.Variables.Count);
            Assert.Single(fileData.Blocks);
            
            Assert.Equal("GlobalValue", fileData.ReadVariable("GlobalVar"));
            Assert.Equal("12345", fileData.ReadVariable("NumericVar"));
            
            Assert.True(fileData.DoesExistBlock("TestBlock"));
            Assert.Equal("SimpleValue", fileData.ReadKey("TestBlock", "SimpleKey"));
            Assert.Equal("42", fileData.ReadKey("TestBlock", "NumericKey"));
        }

        [Fact]
        public void ParseContent_ShouldHandleVariableSubstitution()
        {
            // Arrange
            const string content = @"
%BaseVar%=BaseValue
[TestBlock]
SubstitutedKey=%BaseVar%_suffix
";

            using var fileData = AtrcFileData.CreateEmpty();
            var parser = new AtrcParser();

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            Assert.Equal("BaseValue", fileData.ReadVariable("BaseVar"));
            Assert.Equal("BaseValue_suffix", fileData.ReadKey("TestBlock", "SubstitutedKey"));
        }

        [Fact]
        public void ParseContent_ShouldHandlePrivateVariables()
        {
            // Arrange
            const string content = @"%PublicVar%=PublicValue

[PRIVATE]
%PrivateVar%=PrivateValue

[RegularBlock]
TestKey=TestValue
";

            using var fileData = AtrcFileData.CreateEmpty();
            var parser = new AtrcParser();

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            Assert.True(fileData.IsPublic("PublicVar"));
            Assert.False(fileData.IsPublic("PrivateVar"));
            Assert.Equal("PrivateValue", fileData.ReadVariable("PrivateVar"));
        }

        [Fact]
        public async Task LoadAsync_ShouldCreateFileInCreateAndReadMode()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            
            try
            {
                // Act
                using var fileData = await AtrcFileData.LoadAsync(tempFile, ReadMode.CreateAndRead);

                // Assert
                Assert.NotNull(fileData);
                Assert.Equal(tempFile, fileData.Filename);
                Assert.True(File.Exists(tempFile));
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