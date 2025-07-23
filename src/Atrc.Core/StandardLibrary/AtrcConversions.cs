using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Atrc.Exceptions;

namespace Atrc.Core.StandardLibrary
{
    /// <summary>
    /// Provides extension methods for converting string values from ATRC variables and keys
    /// into various data types, with robust error handling and support for common formats.
    /// </summary>
    public static class AtrcConversions
    {
        /// <summary>
        /// Converts the string representation of a logical value to its <see cref="bool"/> equivalent.
        /// Supports "true", "false", "1", "0", "yes", "no", "on", "off" (case-insensitive).
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>A <see cref="bool"/> equivalent to the value contained in <paramref name="value"/>.</returns>
        /// <exception cref="AtrcInvalidTypeConversionException">
        /// Thrown when <paramref name="value"/> is not a valid boolean representation.
        /// </exception>
        public static bool ToBool(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new AtrcInvalidTypeConversionException($"Cannot convert empty or whitespace string to boolean.");
            }

            string lowerValue = value.Trim().ToLowerInvariant();

            switch (lowerValue)
            {
                case "true":
                case "1":
                case "yes":
                case "on":
                    return true;
                case "false":
                case "0":
                case "no":
                case "off":
                    return false;
                default:
                    throw new AtrcInvalidTypeConversionException($"'{value}' is not a valid boolean representation.");
            }
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="ulong"/> (UInt64) equivalent.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>A <see cref="ulong"/> equivalent to the number contained in <paramref name="value"/>.</returns>
        /// <exception cref="AtrcInvalidTypeConversionException">
        /// Thrown when <paramref name="value"/> is not a valid UInt64 representation.
        /// </exception>
        public static ulong ToUInt64(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new AtrcInvalidTypeConversionException($"Cannot convert empty or whitespace string to UInt64.");
            }

            if (ulong.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out ulong result))
            {
                return result;
            }
            throw new AtrcInvalidTypeConversionException($"'{value}' is not a valid UInt64 representation.");
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="double"/> equivalent.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>A <see cref="double"/> equivalent to the number contained in <paramref name="value"/>.</returns>
        /// <exception cref="AtrcInvalidTypeConversionException">
        /// Thrown when <paramref name="value"/> is not a valid double representation.
        /// </exception>
        public static double ToDouble(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new AtrcInvalidTypeConversionException($"Cannot convert empty or whitespace string to double.");
            }

            if (double.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            throw new AtrcInvalidTypeConversionException($"'{value}' is not a valid double representation.");
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="int"/> equivalent.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>An <see cref="int"/> equivalent to the number contained in <paramref name="value"/>.</returns>
        /// <exception cref="AtrcInvalidTypeConversionException">
        /// Thrown when <paramref name="value"/> is not a valid integer representation.
        /// </exception>
        public static int ToInt(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new AtrcInvalidTypeConversionException($"Cannot convert empty or whitespace string to Int32.");
            }

            if (int.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }
            throw new AtrcInvalidTypeConversionException($"'{value}' is not a valid Int32 representation.");
        }

        /// <summary>
        /// Converts a comma-separated string of numbers to an array of <see cref="double"/> values.
        /// </summary>
        /// <param name="value">The comma-separated string to convert.</param>
        /// <returns>An array of <see cref="double"/> values.</returns>
        /// <exception cref="AtrcInvalidTypeConversionException">
        /// Thrown if any part of the string cannot be converted to a double.
        /// </exception>
        public static double[] ToDoubleArray(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<double>();
            }

            return value.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s =>
                        {
                            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                            {
                                return result;
                            }
                            throw new AtrcInvalidTypeConversionException($"'{s}' is not a valid double representation in array '{value}'.");
                        })
                        .ToArray();
        }

        /// <summary>
        /// Converts a comma-separated string of numbers to an array of <see cref="int"/> values.
        /// </summary>
        /// <param name="value">The comma-separated string to convert.</param>
        /// <returns>An array of <see cref="int"/> values.</returns>
        /// <exception cref="AtrcInvalidTypeConversionException">
        /// Thrown if any part of the string cannot be converted to an int.
        /// </exception>
        public static int[] ToIntArray(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<int>();
            }

            return value.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s =>
                        {
                            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
                            {
                                return result;
                            }
                            throw new AtrcInvalidTypeConversionException($"'{s}' is not a valid Int32 representation in array '{value}'.");
                        })
                        .ToArray();
        }

        /// <summary>
        /// Converts a comma-separated string of logical values to an array of <see cref="bool"/> values.
        /// </summary>
        /// <param name="value">The comma-separated string to convert.</param>
        /// <returns>An array of <see cref="bool"/> values.</returns>
        /// <exception cref="AtrcInvalidTypeConversionException">
        /// Thrown if any part of the string cannot be converted to a boolean.
        /// </exception>
        public static bool[] ToBoolArray(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<bool>();
            }

            return value.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => s.ToBool()) // Re-use existing ToBool for robust parsing
                        .ToArray();
        }

        /// <summary>
        /// Converts a comma-separated string of values to an array of <see cref="string"/> values.
        /// </summary>
        /// <param name="value">The comma-separated string to convert.</param>
        /// <returns>An array of <see cref="string"/> values.</returns>
        public static string[] ToStringArray(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            return value.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();
        }
    }
}