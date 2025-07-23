using System;

namespace Atrc.Exceptions
{
    /// <summary>
    /// Base exception for all ATRC-related errors.
    /// </summary>
    public class AtrcException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtrcException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public AtrcException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtrcException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public AtrcException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown for file I/O related errors.
    /// </summary>
    public class AtrcIOException : AtrcException
    {
        public AtrcIOException(string message) : base(message) { }
        public AtrcIOException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when an ATRC file is not found.
    /// </summary>
    public class AtrcFileNotFoundException : AtrcIOException
    {
        public AtrcFileNotFoundException(string message) : base(message) { }
        public AtrcFileNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when access to an ATRC file is denied.
    /// </summary>
    public class AtrcFileAccessDeniedException : AtrcIOException
    {
        public AtrcFileAccessDeniedException(string message) : base(message) { }
        public AtrcFileAccessDeniedException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when an ATRC file has an invalid format.
    /// </summary>
    public class AtrcFileFormatException : AtrcIOException
    {
        public AtrcFileFormatException(string message) : base(message) { }
        public AtrcFileFormatException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown for parsing-related errors.
    /// </summary>
    public class AtrcParserException : AtrcException
    {
        /// <summary>
        /// Gets the line number where the error occurred.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the column where the error occurred.
        /// </summary>
        public int Column { get; }

        public AtrcParserException(string message, int lineNumber = 0, int column = 0) : base(message)
        {
            LineNumber = lineNumber;
            Column = column;
        }

        public AtrcParserException(string message, Exception innerException, int lineNumber = 0, int column = 0) 
            : base(message, innerException)
        {
            LineNumber = lineNumber;
            Column = column;
        }
    }

    /// <summary>
    /// Exception thrown for syntax errors in ATRC files.
    /// </summary>
    public class AtrcSyntaxException : AtrcParserException
    {
        public AtrcSyntaxException(string message, int lineNumber = 0, int column = 0) : base(message, lineNumber, column) { }
        public AtrcSyntaxException(string message, Exception innerException, int lineNumber = 0, int column = 0) 
            : base(message, innerException, lineNumber, column) { }
    }

    /// <summary>
    /// Exception thrown for invalid preprocessor directives.
    /// </summary>
    public class AtrcInvalidDirectiveException : AtrcParserException
    {
        public AtrcInvalidDirectiveException(string message, int lineNumber = 0, int column = 0) : base(message, lineNumber, column) { }
        public AtrcInvalidDirectiveException(string message, Exception innerException, int lineNumber = 0, int column = 0) 
            : base(message, innerException, lineNumber, column) { }
    }

    /// <summary>
    /// Exception thrown when circular variable references are detected.
    /// </summary>
    public class AtrcCircularReferenceException : AtrcParserException
    {
        public AtrcCircularReferenceException(string message, int lineNumber = 0, int column = 0) : base(message, lineNumber, column) { }
        public AtrcCircularReferenceException(string message, Exception innerException, int lineNumber = 0, int column = 0) 
            : base(message, innerException, lineNumber, column) { }
    }

    /// <summary>
    /// Exception thrown for data access errors.
    /// </summary>
    public class AtrcDataException : AtrcException
    {
        public AtrcDataException(string message) : base(message) { }
        public AtrcDataException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a requested key is not found.
    /// </summary>
    public class AtrcKeyNotFoundException : AtrcDataException
    {
        public AtrcKeyNotFoundException(string message) : base(message) { }
        public AtrcKeyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when type conversion fails.
    /// </summary>
    public class AtrcInvalidTypeConversionException : AtrcDataException
    {
        public AtrcInvalidTypeConversionException(string message) : base(message) { }
        public AtrcInvalidTypeConversionException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown for platform-specific errors.
    /// </summary>
    public class AtrcPlatformException : AtrcException
    {
        public AtrcPlatformException(string message) : base(message) { }
        public AtrcPlatformException(string message, Exception innerException) : base(message, innerException) { }
    }
}