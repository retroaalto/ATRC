using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Atrc.Exceptions;

namespace Atrc.Core.Preprocessing
{
    /// <summary>
    /// Represents the state of a conditional block during preprocessing.
    /// </summary>
    internal class ConditionalState
    {
        public bool IsTrueBlockActive { get; set; }
        public bool HasTrueBlockBeenEntered { get; set; }
        public bool IsSkipping { get; set; }
    }

    /// <summary>
    /// Provides functionality for preprocessing ATRC content, including conditional compilation,
    /// platform detection, and variable evaluation.
    /// </summary>
    public class AtrcPreprocessor
    {
        private readonly Stack<ConditionalState> _conditionalStack;
        private readonly Dictionary<string, string> _variables;
        private int _lineNumber;

        // Regex for preprocessor directives
        private static readonly Regex DirectiveRegex = new Regex(@"^\s*#\.(IF|ELIF|ELSE|ENDIF|ERROR)\s*(.*)$", RegexOptions.IgnoreCase);
        private static readonly Regex VariableRegex = new Regex(@"%([A-Z_]+)%"); // For %VARIABLE%

        /// <summary>
        /// Initializes a new instance of the <see cref="AtrcPreprocessor"/> class.
        /// </summary>
        public AtrcPreprocessor()
        {
            _conditionalStack = new Stack<ConditionalState>();
            _variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            InitializePlatformVariables();
        }

        /// <summary>
        /// Initializes platform-specific variables based on the current operating system.
        /// </summary>
        private void InitializePlatformVariables()
        {
            _variables["WINDOWS"] = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "1" : "0";
            _variables["LINUX"] = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "1" : "0";
            _variables["UNIX"] = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "1" : "0";
            // Add other platforms as needed
        }

        /// <summary>
        /// Preprocesses the given ATRC content, evaluating directives and expressions.
        /// </summary>
        /// <param name="content">The raw ATRC content as a single string.</param>
        /// <returns>The preprocessed content.</returns>
        /// <exception cref="AtrcInvalidDirectiveException">Thrown when an invalid preprocessor directive or syntax is encountered.</exception>
        public string Preprocess(string content)
        {
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var preprocessedLines = new List<string>();
            _lineNumber = 0;

            foreach (var line in lines)
            {
                _lineNumber++;
                var match = DirectiveRegex.Match(line);

                if (match.Success)
                {
                    var directive = match.Groups[1].Value.ToUpperInvariant();
                    var argument = match.Groups[2].Value.Trim();

                    HandleDirective(directive, argument);
                }
                else
                {
                    // Only add the line if we are not currently skipping
                    if (_conditionalStack.Count == 0 || !_conditionalStack.Peek().IsSkipping)
                    {
                        preprocessedLines.Add(line);
                    }
                }
            }

            if (_conditionalStack.Count > 0)
            {
                throw new AtrcInvalidDirectiveException("Unclosed preprocessor conditional block. Missing #.ENDIF.", _lineNumber);
            }

            return string.Join(Environment.NewLine, preprocessedLines);
        }

        /// <summary>
        /// Handles a single preprocessor directive.
        /// </summary>
        /// <param name="directive">The directive keyword (e.g., "IF", "ELIF").</param>
        /// <param name="argument">The argument for the directive (e.g., a boolean expression).</param>
        private void HandleDirective(string directive, string argument)
        {
            switch (directive)
            {
                case "IF":
                    HandleIfDirective(argument);
                    break;
                case "ELIF":
                    HandleElifDirective(argument);
                    break;
                case "ELSE":
                    HandleElseDirective();
                    break;
                case "ENDIF":
                    HandleEndifDirective();
                    break;
                case "ERROR":
                    HandleErrorDirective(argument);
                    break;
                default:
                    throw new AtrcInvalidDirectiveException($"Unknown preprocessor directive: '#.{directive}'.", _lineNumber);
            }
        }

        /// <summary>
        /// Handles the #.IF directive.
        /// </summary>
        /// <param name="expression">The boolean expression to evaluate.</param>
        private void HandleIfDirective(string expression)
        {
            bool conditionResult = EvaluateExpression(expression);
            bool isSkipping = _conditionalStack.Count > 0 && _conditionalStack.Peek().IsSkipping;

            _conditionalStack.Push(new ConditionalState
            {
                IsTrueBlockActive = conditionResult && !isSkipping,
                HasTrueBlockBeenEntered = conditionResult,
                IsSkipping = isSkipping || !conditionResult
            });
        }

        /// <summary>
        /// Handles the #.ELIF directive.
        /// </summary>
        /// <param name="expression">The boolean expression to evaluate.</param>
        private void HandleElifDirective(string expression)
        {
            if (_conditionalStack.Count == 0)
            {
                throw new AtrcInvalidDirectiveException("#.ELIF without a preceding #.IF.", _lineNumber);
            }

            var currentState = _conditionalStack.Peek();
            if (currentState.HasTrueBlockBeenEntered)
            {
                // A true block has already been entered in this conditional, so skip subsequent ELIF/ELSE
                currentState.IsTrueBlockActive = false;
                currentState.IsSkipping = true;
            }
            else
            {
                bool conditionResult = EvaluateExpression(expression);
                bool parentSkipping = _conditionalStack.Count > 1 && _conditionalStack.ToArray()[1].IsSkipping; // Check parent's skipping state

                currentState.IsTrueBlockActive = conditionResult && !parentSkipping;
                currentState.HasTrueBlockBeenEntered = conditionResult;
                currentState.IsSkipping = parentSkipping || !conditionResult;
            }
        }

        /// <summary>
        /// Handles the #.ELSE directive.
        /// </summary>
        private void HandleElseDirective()
        {
            if (_conditionalStack.Count == 0)
            {
                throw new AtrcInvalidDirectiveException("#.ELSE without a preceding #.IF.", _lineNumber);
            }

            var currentState = _conditionalStack.Peek();
            if (currentState.HasTrueBlockBeenEntered)
            {
                // A true block has already been entered, so skip the ELSE block
                currentState.IsTrueBlockActive = false;
                currentState.IsSkipping = true;
            }
            else
            {
                bool parentSkipping = _conditionalStack.Count > 1 && _conditionalStack.ToArray()[1].IsSkipping; // Check parent's skipping state
                currentState.IsTrueBlockActive = !parentSkipping;
                currentState.IsSkipping = parentSkipping;
                currentState.HasTrueBlockBeenEntered = true; // ELSE block is the final option, so it's considered "entered" if active
            }
        }

        /// <summary>
        /// Handles the #.ENDIF directive.
        /// </summary>
        private void HandleEndifDirective()
        {
            if (_conditionalStack.Count == 0)
            {
                throw new AtrcInvalidDirectiveException("#.ENDIF without a preceding #.IF.", _lineNumber);
            }
            _conditionalStack.Pop();
        }

        /// <summary>
        /// Handles the #.ERROR directive, throwing an exception with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        private void HandleErrorDirective(string message)
        {
            // Only throw error if not currently skipping the block
            if (_conditionalStack.Count == 0 || !_conditionalStack.Peek().IsSkipping)
            {
                throw new AtrcInvalidDirectiveException($"Preprocessor error: {message}", _lineNumber);
            }
        }

        /// <summary>
        /// Evaluates a boolean expression, substituting variables.
        /// Supports basic equality (==), inequality (!=), and boolean literals (0, 1).
        /// </summary>
        /// <param name="expression">The expression string.</param>
        /// <returns>The boolean result of the expression.</returns>
        /// <exception cref="AtrcInvalidDirectiveException">Thrown if the expression is malformed or contains unknown variables.</exception>
        private bool EvaluateExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return false; // Empty expression is false
            }

            // Substitute variables first
            string substitutedExpression = VariableRegex.Replace(expression, match =>
            {
                var varName = match.Groups[1].Value;
                if (_variables.TryGetValue(varName, out var value))
                {
                    return value;
                }
                throw new AtrcInvalidDirectiveException($"Unknown preprocessor variable: '%{varName}%'.", _lineNumber);
            });

            // Simple expression evaluation (e.g., "1 == 1", "0 != 1", "1")
            substitutedExpression = substitutedExpression.Trim();

            if (substitutedExpression.Equals("1") || substitutedExpression.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (substitutedExpression.Equals("0") || substitutedExpression.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Handle equality/inequality
            var parts = substitutedExpression.Split(new[] { "==", "!=" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var left = parts[0].Trim();
                var right = parts[1].Trim();
                bool isEqual = substitutedExpression.Contains("==");

                // Treat "1" as true, "0" as false for comparison
                bool leftBool = left.Equals("1", StringComparison.OrdinalIgnoreCase) || left.Equals("true", StringComparison.OrdinalIgnoreCase);
                bool rightBool = right.Equals("1", StringComparison.OrdinalIgnoreCase) || right.Equals("true", StringComparison.OrdinalIgnoreCase);

                if (isEqual)
                {
                    return leftBool == rightBool;
                }
                else
                {
                    return leftBool != rightBool;
                }
            }

            throw new AtrcInvalidDirectiveException($"Malformed preprocessor expression: '{expression}'.", _lineNumber);
        }
    }
}