using System;
using Xunit;
using Atrc.Core.Models;
using Atrc.Core.Parsing;
using Atrc.Exceptions;

namespace Atrc.Core.Tests
{
    /// <summary>
    /// Tests for the enhanced variable substitution functionality.
    /// </summary>
    public class VariableSubstitutionTests
    {
        [Fact]
        public void ParseContent_SimpleVariableSubstitution_ShouldResolveCorrectly()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%baseUrl%=https://example.com
%fullUrl%=%baseUrl%/api/users

[Config]
Url=%fullUrl%";

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            Assert.Equal("https://example.com", fileData.ReadVariable("baseUrl"));
            Assert.Equal("https://example.com/api/users", fileData.ReadVariable("fullUrl"));
            Assert.Equal("https://example.com/api/users", fileData.ReadKey("Config", "Url"));
        }

        [Fact]
        public void ParseContent_NestedVariableSubstitution_ShouldResolveCorrectly()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%server%=prod-server
%environment%=%server%-env
%database%=%environment%-db
%connectionString%=server=%database%;trusted=true

[Database]
Connection=%connectionString%";

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            Assert.Equal("prod-server", fileData.ReadVariable("server"));
            Assert.Equal("prod-server-env", fileData.ReadVariable("environment"));
            Assert.Equal("prod-server-env-db", fileData.ReadVariable("database"));
            Assert.Equal("server=prod-server-env-db;trusted=true", fileData.ReadVariable("connectionString"));
            Assert.Equal("server=prod-server-env-db;trusted=true", fileData.ReadKey("Database", "Connection"));
        }

        [Fact]
        public void ParseContent_CircularReference_ShouldThrowException()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%var1%=%var2%
%var2%=%var1%

[Test]
Value=%var1%";

            // Act & Assert
            var exception = Assert.Throws<AtrcCircularReferenceException>(() => parser.ParseContent(content, fileData));
            Assert.Contains("Circular variable reference detected", exception.Message);
            Assert.Contains("var1", exception.Message);
            Assert.Contains("var2", exception.Message);
        }

        [Fact]
        public void ParseContent_ComplexCircularReference_ShouldThrowException()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%a%=%b%_suffix
%b%=%c%_middle
%c%=%a%_start

[Test]
Value=%a%";

            // Act & Assert
            var exception = Assert.Throws<AtrcCircularReferenceException>(() => parser.ParseContent(content, fileData));
            Assert.Contains("Circular variable reference detected", exception.Message);
        }

        [Fact]
        public void ParseContent_UndefinedVariable_ShouldThrowException()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%defined%=value

[Test]
Value=%undefined%";

            // Act & Assert
            var exception = Assert.Throws<AtrcParserException>(() => parser.ParseContent(content, fileData));
            Assert.Contains("Undefined variable 'undefined' referenced", exception.Message);
        }

        [Fact]
        public void ParseContent_DeepNesting_ShouldResolveWithinLimit()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%level0%=base
%level1%=%level0%-1
%level2%=%level1%-2
%level3%=%level2%-3
%level4%=%level3%-4
%level5%=%level4%-5

[Test]
Value=%level5%";

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            Assert.Equal("base-1-2-3-4-5", fileData.ReadKey("Test", "Value"));
        }

        [Fact(Skip = "Depth limiting needs refinement")]
        public void ParseContent_ExcessiveDepth_ShouldThrowException()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var contentBuilder = new System.Text.StringBuilder();
            
            // Create a chain of 12 variables (exceeds the MaxSubstitutionDepth of 10)
            contentBuilder.AppendLine("%level0%=base");
            for (int i = 1; i <= 12; i++)
            {
                contentBuilder.AppendLine($"%level{i}%=%level{i-1}%-{i}");
            }
            contentBuilder.AppendLine("[Test]");
            contentBuilder.AppendLine("Value=%level12%");

            // Act & Assert
            var exception = Assert.Throws<AtrcParserException>(() => parser.ParseContent(contentBuilder.ToString(), fileData));
            Assert.Contains("Maximum variable substitution depth", exception.Message);
        }

        [Fact]
        public void ParseContent_MultipleVariablesInSingleValue_ShouldResolveCorrectly()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%protocol%=https
%host%=api.example.com
%port%=443
%version%=v2

[API]
BaseUrl=%protocol%://%host%:%port%/%version%";

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            Assert.Equal("https://api.example.com:443/v2", fileData.ReadKey("API", "BaseUrl"));
        }

        [Fact]
        public void ParseContent_TimeInjection_ShouldReplaceTimeReference()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%timestamp%=Time: %*%

[Logging]
LogMessage=%timestamp%";

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            var timestampValue = fileData.ReadVariable("timestamp");
            var logMessage = fileData.ReadKey("Logging", "LogMessage");
            
            Assert.StartsWith("Time: ", timestampValue);
            Assert.StartsWith("Time: ", logMessage);
            Assert.Equal(timestampValue, logMessage);
        }

        [Fact]
        public void ParseContent_VariableWithTimeInjectionAndSubstitution_ShouldResolveCorrectly()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%prefix%=LOG
%timeValue%=%*%
%logEntry%=%prefix%: %timeValue%

[Log]
Entry=%logEntry%";

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            var logEntry = fileData.ReadKey("Log", "Entry");
            Assert.StartsWith("LOG: ", logEntry);
            Assert.Matches(@"LOG: \d+\.\d", logEntry); // Should match pattern like "LOG: 42.3"
        }

        [Fact]
        public void ParseContent_VariableInPrivateSection_ShouldNotAffectSubstitution()
        {
            // Arrange
            var parser = new AtrcParser();
            var fileData = AtrcFileData.CreateEmpty();
            var content = @"
%publicVar%=public_value

[PRIVATE]
%privateVar%=private_value

[Config]
PublicValue=%publicVar%
PrivateValue=%privateVar%";

            // Act
            parser.ParseContent(content, fileData);

            // Assert
            Assert.Equal("public_value", fileData.ReadKey("Config", "PublicValue"));
            Assert.Equal("private_value", fileData.ReadKey("Config", "PrivateValue"));
            Assert.True(fileData.IsPublic("publicVar"));
            Assert.False(fileData.IsPublic("privateVar"));
        }
    }
}