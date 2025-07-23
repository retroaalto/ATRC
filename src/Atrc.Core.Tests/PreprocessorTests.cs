using System;
using Xunit;
using Atrc.Core.Preprocessing;
using Atrc.Exceptions;

namespace Atrc.Core.Tests
{
    /// <summary>
    /// Tests for the ATRC preprocessor functionality.
    /// </summary>
    public class PreprocessorTests
    {
        [Fact]
        public void Preprocess_SimpleIfCondition_ShouldProcessCorrectly()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.IF %LINUX% == 1
LinuxSpecific=true
#.ENDIF
AlwaysPresent=value";

            // Act
            var result = preprocessor.Preprocess(content);

            // Assert
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Assert.Contains("LinuxSpecific=true", result);
            }
            else
            {
                Assert.DoesNotContain("LinuxSpecific=true", result);
            }
            Assert.Contains("AlwaysPresent=value", result);
        }

        [Fact]
        public void Preprocess_IfElifElse_ShouldProcessCorrectly()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.IF %WINDOWS% == 1
WindowsKey=Windows Value
#.ELIF %LINUX% == 1
LinuxKey=Linux Value
#.ELSE
OtherKey=Other Value
#.ENDIF";

            // Act
            var result = preprocessor.Preprocess(content);

            // Assert - Should contain exactly one of the platform-specific keys
            var windowsPresent = result.Contains("WindowsKey=Windows Value");
            var linuxPresent = result.Contains("LinuxKey=Linux Value");
            var otherPresent = result.Contains("OtherKey=Other Value");

            // Exactly one should be true
            Assert.True((windowsPresent ? 1 : 0) + (linuxPresent ? 1 : 0) + (otherPresent ? 1 : 0) == 1);
        }

        [Fact]
        public void Preprocess_NestedConditionals_ShouldProcessCorrectly()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.IF %LINUX% == 1
OuterLinux=true
#.IF %UNIX% == 1
InnerUnix=true  
#.ENDIF
#.ENDIF";

            // Act
            var result = preprocessor.Preprocess(content);

            // Assert
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Assert.Contains("OuterLinux=true", result);
                Assert.Contains("InnerUnix=true", result); // Linux is also Unix
            }
            else
            {
                Assert.DoesNotContain("OuterLinux=true", result);
                Assert.DoesNotContain("InnerUnix=true", result);
            }
        }

        [Fact]
        public void Preprocess_ErrorDirective_ShouldThrowException()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.ERROR This is an error message
SomeKey=value";

            // Act & Assert
            var exception = Assert.Throws<AtrcInvalidDirectiveException>(() => preprocessor.Preprocess(content));
            Assert.Contains("Preprocessor error: This is an error message", exception.Message);
        }

        [Fact]
        public void Preprocess_ErrorDirectiveInSkippedBlock_ShouldNotThrow()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.IF %WINDOWS% == 1
#.ERROR This should not be processed on non-Windows
#.ENDIF
SomeKey=value";

            // Act - Should not throw if we're not on Windows
            var result = preprocessor.Preprocess(content);

            // Assert
            Assert.Contains("SomeKey=value", result);
        }

        [Fact]
        public void Preprocess_UnmatchedEndif_ShouldThrowException()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.ENDIF
SomeKey=value";

            // Act & Assert
            Assert.Throws<AtrcInvalidDirectiveException>(() => preprocessor.Preprocess(content));
        }

        [Fact]
        public void Preprocess_UnclosedIf_ShouldThrowException()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.IF %LINUX% == 1
SomeKey=value";

            // Act & Assert
            var exception = Assert.Throws<AtrcInvalidDirectiveException>(() => preprocessor.Preprocess(content));
            Assert.Contains("Unclosed preprocessor conditional block", exception.Message);
        }

        [Fact]
        public void Preprocess_UnknownVariable_ShouldThrowException()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.IF %UNKNOWN_VAR% == 1
SomeKey=value
#.ENDIF";

            // Act & Assert
            var exception = Assert.Throws<AtrcInvalidDirectiveException>(() => preprocessor.Preprocess(content));
            Assert.Contains("Unknown preprocessor variable: '%UNKNOWN_VAR%'", exception.Message);
        }

        [Fact]
        public void Preprocess_BooleanLiterals_ShouldProcessCorrectly()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.IF 1
TrueBlock=true
#.ENDIF
#.IF 0
FalseBlock=false
#.ENDIF";

            // Act
            var result = preprocessor.Preprocess(content);

            // Assert
            Assert.Contains("TrueBlock=true", result);
            Assert.DoesNotContain("FalseBlock=false", result);
        }

        [Fact]
        public void Preprocess_EqualityOperators_ShouldProcessCorrectly()
        {
            // Arrange
            var preprocessor = new AtrcPreprocessor();
            var content = @"
#.IF 1 == 1
Equal=true
#.ENDIF
#.IF 1 != 0
NotEqual=true
#.ENDIF
#.IF 1 == 0
ShouldNotAppear=false
#.ENDIF";

            // Act
            var result = preprocessor.Preprocess(content);

            // Assert
            Assert.Contains("Equal=true", result);
            Assert.Contains("NotEqual=true", result);
            Assert.DoesNotContain("ShouldNotAppear=false", result);
        }
    }
}