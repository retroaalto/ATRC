using System;
using Xunit;
using Atrc.Core.StandardLibrary;
using Atrc.Exceptions;

namespace Atrc.Core.Tests
{
    /// <summary>
    /// Tests for the ATRC standard library conversion functions.
    /// </summary>
    public class StandardLibraryTests
    {
        #region Boolean Conversion Tests

        [Theory]
        [InlineData("true", true)]
        [InlineData("TRUE", true)]
        [InlineData("True", true)]
        [InlineData("1", true)]
        [InlineData("yes", true)]
        [InlineData("YES", true)]
        [InlineData("on", true)]
        [InlineData("ON", true)]
        [InlineData("false", false)]
        [InlineData("FALSE", false)]
        [InlineData("False", false)]
        [InlineData("0", false)]
        [InlineData("no", false)]
        [InlineData("NO", false)]
        [InlineData("off", false)]
        [InlineData("OFF", false)]
        public void ToBool_ValidValues_ShouldConvertCorrectly(string input, bool expected)
        {
            // Act
            var result = input.ToBool();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("maybe")]
        [InlineData("2")]
        [InlineData("invalid")]
        public void ToBool_InvalidValues_ShouldThrowException(string input)
        {
            // Act & Assert
            Assert.Throws<AtrcInvalidTypeConversionException>(() => input.ToBool());
        }

        [Theory]
        [InlineData("true,false,1,0", new[] { true, false, true, false })]
        [InlineData("yes,no,on,off", new[] { true, false, true, false })]
        [InlineData(" TRUE , FALSE , YES , NO ", new[] { true, false, true, false })]
        public void ToBoolArray_ValidValues_ShouldConvertCorrectly(string input, bool[] expected)
        {
            // Act
            var result = input.ToBoolArray();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToBoolArray_EmptyString_ShouldReturnEmptyArray()
        {
            // Act
            var result = "".ToBoolArray();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Integer Conversion Tests

        [Theory]
        [InlineData("42", 42)]
        [InlineData("-42", -42)]
        [InlineData("0", 0)]
        [InlineData("2147483647", 2147483647)]
        [InlineData("-2147483648", -2147483648)]
        [InlineData("  123  ", 123)]
        public void ToInt_ValidValues_ShouldConvertCorrectly(string input, int expected)
        {
            // Act
            var result = input.ToInt();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("12.34")]
        [InlineData("2147483648")] // Overflow
        public void ToInt_InvalidValues_ShouldThrowException(string input)
        {
            // Act & Assert
            Assert.Throws<AtrcInvalidTypeConversionException>(() => input.ToInt());
        }

        [Theory]
        [InlineData("1,2,3,4", new[] { 1, 2, 3, 4 })]
        [InlineData("10, 20, 30", new[] { 10, 20, 30 })]
        [InlineData("-1,0,1", new[] { -1, 0, 1 })]
        public void ToIntArray_ValidValues_ShouldConvertCorrectly(string input, int[] expected)
        {
            // Act
            var result = input.ToIntArray();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region UInt64 Conversion Tests

        [Theory]
        [InlineData("42", 42UL)]
        [InlineData("0", 0UL)]
        [InlineData("18446744073709551615", 18446744073709551615UL)] // Max UInt64
        [InlineData("  123  ", 123UL)]
        public void ToUInt64_ValidValues_ShouldConvertCorrectly(string input, ulong expected)
        {
            // Act
            var result = input.ToUInt64();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("-1")] // Negative
        [InlineData("12.34")]
        public void ToUInt64_InvalidValues_ShouldThrowException(string input)
        {
            // Act & Assert
            Assert.Throws<AtrcInvalidTypeConversionException>(() => input.ToUInt64());
        }

        #endregion

        #region Double Conversion Tests

        [Theory]
        [InlineData("42.5", 42.5)]
        [InlineData("-42.5", -42.5)]
        [InlineData("0", 0.0)]
        [InlineData("0.0", 0.0)]
        [InlineData("3.14159", 3.14159)]
        [InlineData("1e3", 1000.0)]
        [InlineData("  123.45  ", 123.45)]
        public void ToDouble_ValidValues_ShouldConvertCorrectly(string input, double expected)
        {
            // Act
            var result = input.ToDouble();

            // Assert
            Assert.Equal(expected, result, 6); // 6 decimal places precision
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("not_a_number")]
        public void ToDouble_InvalidValues_ShouldThrowException(string input)
        {
            // Act & Assert
            Assert.Throws<AtrcInvalidTypeConversionException>(() => input.ToDouble());
        }

        [Theory]
        [InlineData("1.1,2.2,3.3", new[] { 1.1, 2.2, 3.3 })]
        [InlineData("0.5, 1.5, 2.5", new[] { 0.5, 1.5, 2.5 })]
        [InlineData("-1.0,0.0,1.0", new[] { -1.0, 0.0, 1.0 })]
        public void ToDoubleArray_ValidValues_ShouldConvertCorrectly(string input, double[] expected)
        {
            // Act
            var result = input.ToDoubleArray();

            // Assert
            Assert.Equal(expected.Length, result.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], result[i], 6);
            }
        }

        #endregion

        #region String Array Tests

        [Theory]
        [InlineData("apple,banana,cherry", new[] { "apple", "banana", "cherry" })]
        [InlineData("one, two, three", new[] { "one", "two", "three" })]
        [InlineData("  first  ,  second  ", new[] { "first", "second" })]
        public void ToStringArray_ValidValues_ShouldConvertCorrectly(string input, string[] expected)
        {
            // Act
            var result = input.ToStringArray();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToStringArray_EmptyString_ShouldReturnEmptyArray()
        {
            // Act
            var result = "".ToStringArray();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ToStringArray_WhitespaceOnly_ShouldReturnEmptyArray()
        {
            // Act
            var result = "   ".ToStringArray();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ConversionMethods_WithAtrcFileData_ShouldWorkCorrectly()
        {
            // Arrange
            var parser = new Atrc.Core.Parsing.AtrcParser();
            var fileData = Atrc.Core.Models.AtrcFileData.CreateEmpty();
            var content = @"
%debugMode%=true
%port%=8080
%version%=1.23
%servers%=web01,web02,web03
%flags%=true,false,yes,no

[Database]
MaxConnections=100
Timeout=30.5
EnableLogging=on";

            // Act
            parser.ParseContent(content, fileData);

            // Assert - Test various conversions
            Assert.True(fileData.ReadVariable("debugMode").ToBool());
            Assert.Equal(8080, fileData.ReadVariable("port").ToInt());
            Assert.Equal(1.23, fileData.ReadVariable("version").ToDouble(), 2);
            Assert.Equal(new[] { "web01", "web02", "web03" }, fileData.ReadVariable("servers").ToStringArray());
            Assert.Equal(new[] { true, false, true, false }, fileData.ReadVariable("flags").ToBoolArray());

            Assert.Equal(100, fileData.ReadKey("Database", "MaxConnections").ToInt());
            Assert.Equal(30.5, fileData.ReadKey("Database", "Timeout").ToDouble(), 1);
            Assert.True(fileData.ReadKey("Database", "EnableLogging").ToBool());
        }

        #endregion
    }
}