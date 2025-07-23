using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Atrc.Core.Models;
using Atrc.Core.Preprocessing;
using Atrc.Exceptions;

namespace Atrc.Core.Parsing
{
    /// <summary>
    /// Parser for ATRC configuration files.
    /// </summary>
    public class AtrcParser
    {
        private static readonly Regex VariablePattern = new Regex(@"^%([^%]+)%\s*=\s*(.*)$", RegexOptions.Compiled);
        private static readonly Regex BlockPattern = new Regex(@"^\[([^\]]+)\]$", RegexOptions.Compiled);
        private static readonly Regex KeyPattern = new Regex(@"^([^=]+)=(.*)$", RegexOptions.Compiled);
        private static readonly Regex VariableSubstitutionPattern = new Regex(@"%([^%]+)%", RegexOptions.Compiled);

        /// <summary>
        /// Parses ATRC content from a string.
        /// </summary>
        /// <param name="content">The ATRC file content.</param>
        /// <param name="fileData">The AtrcFileData instance to populate.</param>
        /// <exception cref="AtrcParserException">Thrown when parsing fails.</exception>
        public void ParseContent(string content, AtrcFileData fileData)
        {
            if (string.IsNullOrEmpty(content))
                return;

            // First, preprocess the content to handle conditional directives
            var preprocessor = new AtrcPreprocessor();
            var preprocessedContent = preprocessor.Preprocess(content);

            var lines = preprocessedContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string? currentBlockName = null;
            bool inPrivateSection = false;
            int lineNumber = 0;

            foreach (var rawLine in lines)
            {
                lineNumber++;
                var line = rawLine.Trim();

                // Skip empty lines and comments (but not preprocessor directives which are already handled)
                if (string.IsNullOrWhiteSpace(line) || (line.StartsWith("#") && !line.StartsWith("#.")))
                {
                    continue; // Skip empty lines and regular comments
                }
                
                // Check for special PRIVATE section
                if (line == "[PRIVATE]")
                {
                    inPrivateSection = true;
                    currentBlockName = null;
                    continue;
                }

                try
                {
                    // Try to match variable pattern
                    var variableMatch = VariablePattern.Match(line);
                    if (variableMatch.Success)
                    {
                        var name = variableMatch.Groups[1].Value.Trim();
                        var value = variableMatch.Groups[2].Value.Trim();
                        
                        // Don't resolve variable substitutions yet - store raw values first
                        if (!fileData.AddVariable(name, value, !inPrivateSection))
                        {
                            // Variable already exists, modify it
                            fileData.ModifyVariable(name, value);
                        }
                        continue;
                    }

                    // Try to match block pattern
                    var blockMatch = BlockPattern.Match(line);
                    if (blockMatch.Success)
                    {
                        currentBlockName = blockMatch.Groups[1].Value.Trim();
                        
                        // Reset private section when entering a named block
                        if (currentBlockName != "PRIVATE")
                        {
                            inPrivateSection = false;
                        }
                        
                        if (!fileData.AddBlock(currentBlockName))
                        {
                            // Block already exists, continue using it
                        }
                        continue;
                    }

                    // Try to match key pattern
                    var keyMatch = KeyPattern.Match(line);
                    if (keyMatch.Success)
                    {
                        if (string.IsNullOrEmpty(currentBlockName))
                        {
                            throw new AtrcParserException($"Key found outside of block at line {lineNumber}: {line}", lineNumber);
                        }

                        var keyName = keyMatch.Groups[1].Value.Trim();
                        var keyValue = keyMatch.Groups[2].Value.Trim();
                        
                        // Don't resolve variable substitutions yet - store raw values first
                        if (!fileData.AddKey(currentBlockName, keyName, keyValue))
                        {
                            // Key already exists, modify it
                            fileData.ModifyKey(currentBlockName, keyName, keyValue);
                        }
                        continue;
                    }

                    // If we get here, the line didn't match any expected pattern
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        throw new AtrcParserException($"Unrecognized line format at line {lineNumber}: {line}", lineNumber);
                    }
                }
                catch (AtrcParserException)
                {
                    throw; // Re-throw parser exceptions
                }
                catch (Exception ex)
                {
                    throw new AtrcParserException($"Error parsing line {lineNumber}: {line}", ex, lineNumber);
                }
            }

            // Second phase: Resolve all variable substitutions now that all variables are defined
            ResolveAllVariableSubstitutions(fileData);
        }

        /// <summary>
        /// Resolves all variable substitutions in the ATRC file data after all variables and keys have been parsed.
        /// </summary>
        /// <param name="fileData">The ATRC file data containing variables and keys to resolve.</param>
        private void ResolveAllVariableSubstitutions(AtrcFileData fileData)
        {
            // Resolve all variable values
            foreach (var variable in fileData.Variables.ToList())
            {
                var resolvedValue = ProcessVariableSubstitution(variable.Value, fileData);
                fileData.ModifyVariable(variable.Name, resolvedValue);
            }

            // Resolve all key values
            foreach (var block in fileData.Blocks.ToList())
            {
                foreach (var key in block.Keys.ToList())
                {
                    var resolvedValue = ProcessVariableSubstitution(key.Value, fileData);
                    fileData.ModifyKey(block.Name, key.Name, resolvedValue);
                }
            }
        }

        /// <summary>
        /// Processes variable substitution in a value string, supporting nested substitutions and circular reference detection.
        /// </summary>
        /// <param name="value">The value string that may contain variable references.</param>
        /// <param name="fileData">The ATRC file data containing variables.</param>
        /// <returns>The value with variables substituted.</returns>
        /// <exception cref="AtrcCircularReferenceException">Thrown when a circular variable reference is detected.</exception>
        /// <exception cref="AtrcParserException">Thrown when a variable is not found or maximum substitution depth is exceeded.</exception>
        private string ProcessVariableSubstitution(string value, AtrcFileData fileData)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Handle time injection first (%*%)
            if (value.Contains("%*%"))
            {
                var timePercent = DateTime.Now.Millisecond / 10.0; // Convert to percentage
                value = value.Replace("%*%", timePercent.ToString("F1"));
            }

            // Start recursive processing with an empty set of visited variables and initial depth
            return ResolveVariableSubstitutionsRecursive(value, fileData, new HashSet<string>(), 0);
        }

        /// <summary>
        /// Recursively resolves variable substitutions in a string.
        /// </summary>
        /// <param name="input">The string potentially containing variable references.</param>
        /// <param name="fileData">The ATRC file data containing variables.</param>
        /// <param name="visitedVariables">A set to track variables already visited in the current resolution path to detect circular references.</param>
        /// <param name="depth">The current recursion depth.</param>
        /// <returns>The string with all variables substituted.</returns>
        /// <exception cref="AtrcCircularReferenceException">Thrown when a circular variable reference is detected.</exception>
        /// <exception cref="AtrcParserException">Thrown when a variable is not found or maximum substitution depth is exceeded.</exception>
        private string ResolveVariableSubstitutionsRecursive(string input, AtrcFileData fileData, HashSet<string> visitedVariables, int depth)
        {
            const int MaxSubstitutionDepth = 10; // Limit to prevent infinite recursion for very complex nested structures

            if (depth > MaxSubstitutionDepth)
            {
                throw new AtrcParserException($"Maximum variable substitution depth ({MaxSubstitutionDepth}) exceeded. This may indicate a very deep nesting or a complex circular reference not caught earlier.");
            }

            string result = input;
            bool changed = true;

            // Continue replacing until no more substitutions can be made in a pass
            while (changed)
            {
                changed = false;
                result = VariableSubstitutionPattern.Replace(result, match =>
                {
                    var variableName = match.Groups[1].Value;

                    // Check for circular reference
                    if (visitedVariables.Contains(variableName))
                    {
                        throw new AtrcCircularReferenceException($"Circular variable reference detected for '{variableName}'. Path: {string.Join(" -> ", visitedVariables)} -> {variableName}");
                    }

                    var variableValue = fileData.ReadVariable(variableName);

                    if (variableValue == null)
                    {
                        // If variable not found, throw an error. This prevents silent failures for undefined variables.
                        throw new AtrcParserException($"Undefined variable '{variableName}' referenced.");
                    }

                    // Add to visited set for this path
                    visitedVariables.Add(variableName);

                    // Recursively resolve the substituted value itself
                    var resolvedValue = ResolveVariableSubstitutionsRecursive(variableValue, fileData, visitedVariables, depth + 1);

                    // Remove from visited set as we backtrack from this variable's resolution
                    visitedVariables.Remove(variableName);

                    // If the value actually changed, indicate that another pass might be needed
                    if (resolvedValue != match.Value)
                    {
                        changed = true;
                    }

                    return resolvedValue;
                });
            }

            return result;
        }

        /// <summary>
        /// Parses an ATRC file asynchronously.
        /// </summary>
        /// <param name="filePath">The path to the ATRC file.</param>
        /// <param name="mode">The read mode.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous parse operation.</returns>
        public async Task<AtrcFileData> ParseFileAsync(string filePath, ReadMode mode = ReadMode.ReadOnly, CancellationToken cancellationToken = default)
        {
            try
            {
                var fileData = await AtrcFileData.LoadAsync(filePath, mode, cancellationToken);
                return fileData;
            }
            catch (Exception ex) when (!(ex is AtrcException))
            {
                throw new AtrcParserException($"Failed to parse ATRC file: {filePath}", ex);
            }
        }

        /// <summary>
        /// Validates ATRC file syntax without creating a full document.
        /// </summary>
        /// <param name="content">The ATRC content to validate.</param>
        /// <returns>True if syntax is valid; otherwise, false.</returns>
        public bool ValidateSyntax(string content)
        {
            try
            {
                var tempFileData = AtrcFileData.CreateEmpty();
                ParseContent(content, tempFileData);
                tempFileData.Dispose();
                return true;
            }
            catch (AtrcParserException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets parsing statistics for an ATRC file.
        /// </summary>
        /// <param name="content">The ATRC content.</param>
        /// <returns>Parsing statistics.</returns>
        public AtrcParsingStats GetParsingStats(string content)
        {
            if (string.IsNullOrEmpty(content))
                return new AtrcParsingStats();

            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var stats = new AtrcParsingStats
            {
                TotalLines = lines.Length
            };

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    stats.EmptyLines++;
                }
                else if (trimmedLine.StartsWith("#"))
                {
                    stats.CommentLines++;
                }
                else if (VariablePattern.IsMatch(trimmedLine))
                {
                    stats.VariableLines++;
                }
                else if (BlockPattern.IsMatch(trimmedLine))
                {
                    stats.BlockLines++;
                }
                else if (KeyPattern.IsMatch(trimmedLine))
                {
                    stats.KeyLines++;
                }
                else
                {
                    stats.UnrecognizedLines++;
                }
            }

            return stats;
        }
    }

    /// <summary>
    /// Statistics about ATRC file parsing.
    /// </summary>
    public class AtrcParsingStats
    {
        /// <summary>
        /// Gets or sets the total number of lines.
        /// </summary>
        public int TotalLines { get; set; }

        /// <summary>
        /// Gets or sets the number of empty lines.
        /// </summary>
        public int EmptyLines { get; set; }

        /// <summary>
        /// Gets or sets the number of comment lines.
        /// </summary>
        public int CommentLines { get; set; }

        /// <summary>
        /// Gets or sets the number of variable definition lines.
        /// </summary>
        public int VariableLines { get; set; }

        /// <summary>
        /// Gets or sets the number of block definition lines.
        /// </summary>
        public int BlockLines { get; set; }

        /// <summary>
        /// Gets or sets the number of key definition lines.
        /// </summary>
        public int KeyLines { get; set; }

        /// <summary>
        /// Gets or sets the number of unrecognized lines.
        /// </summary>
        public int UnrecognizedLines { get; set; }
    }
}