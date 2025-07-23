using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atrc.Core.Parsing;
using Atrc.Exceptions;

namespace Atrc.Core.Models;

/// <summary>
/// Represents an ATRC file document with variables, blocks, and keys.
/// Equivalent to the C++ ATRC_FD class.
/// </summary>
public class AtrcFileData : IDisposable
{
    private readonly ConcurrentDictionary<string, AtrcVariable> _variables = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, AtrcBlock> _blocks = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;
    private readonly object _lockObject = new object();

    /// <summary>
    /// Gets the collection of variables in the ATRC file.
    /// </summary>
    public IReadOnlyCollection<AtrcVariable> Variables => _variables.Values.ToList().AsReadOnly();

    /// <summary>
    /// Gets the collection of blocks in the ATRC file.
    /// </summary>
    public IReadOnlyCollection<AtrcBlock> Blocks => _blocks.Values.ToList().AsReadOnly();

    /// <summary>
    /// Gets or sets the filename of the ATRC file.
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether changes should be automatically saved.
    /// </summary>
    public bool AutoSave { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether write operations should be validated.
    /// </summary>
    public bool WriteCheck { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtrcFileData"/> class.
    /// </summary>
    private AtrcFileData()
    {
        AutoSave = false;
        WriteCheck = true;
    }

    /// <summary>
    /// Gets or sets the value of a variable or key using indexer notation.
    /// For variables: fileData["variableName"]
    /// For keys: fileData["blockName.keyName"]
    /// </summary>
    /// <param name="path">The variable name or block.key path.</param>
    /// <returns>The value of the variable or key.</returns>
    public string? this[string path]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var dotIndex = path.IndexOf('.');
            if (dotIndex == -1)
            {
                // Simple variable access
                return ReadVariable(path);
            }

            // Block.Key access
            var blockName = path.Substring(0, dotIndex);
            var keyName = path.Substring(dotIndex + 1);
            return ReadKey(blockName, keyName);
        }
        set
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            var dotIndex = path.IndexOf('.');
            if (dotIndex == -1)
            {
                // Simple variable modification
                ModifyVariable(path, value ?? string.Empty);
            }
            else
            {
                // Block.Key modification
                var blockName = path.Substring(0, dotIndex);
                var keyName = path.Substring(dotIndex + 1);
                ModifyKey(blockName, keyName, value ?? string.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets the value of a key in a specific block.
    /// </summary>
    /// <param name="blockName">The name of the block.</param>
    /// <param name="keyName">The name of the key.</param>
    /// <returns>The value of the key.</returns>
    public string? this[string blockName, string keyName]
    {
        get => ReadKey(blockName, keyName);
        set => ModifyKey(blockName, keyName, value ?? string.Empty);
    }

    /// <summary>
    /// Creates an empty ATRC file data instance.
    /// </summary>
    /// <returns>A new empty <see cref="AtrcFileData"/> instance.</returns>
    public static AtrcFileData CreateEmpty()
    {
        return new AtrcFileData();
    }

    /// <summary>
    /// Loads an ATRC file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the ATRC file.</param>
    /// <param name="mode">The read mode for the file.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    public static async Task<AtrcFileData> LoadAsync(string filePath, ReadMode mode = ReadMode.ReadOnly, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var fileData = new AtrcFileData { Filename = filePath };

        try
        {
            await fileData.ReadAsync(filePath, mode, cancellationToken);
            return fileData;
        }
        catch
        {
            fileData.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Reads the ATRC file with the specified mode.
    /// </summary>
    /// <param name="filePath">The path to the ATRC file.</param>
    /// <param name="mode">The read mode.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ReadAsync(string filePath, ReadMode mode, CancellationToken cancellationToken = default)
    {
        switch (mode)
        {
            case ReadMode.ReadOnly:
                if (!File.Exists(filePath))
                    throw new AtrcFileNotFoundException($"File not found: {filePath}");
                break;
            case ReadMode.CreateAndRead:
                if (!File.Exists(filePath))
                {
#if NET6_0_OR_GREATER
                    await File.WriteAllTextAsync(filePath, string.Empty, cancellationToken);
#else
                    await Task.Run(() => File.WriteAllText(filePath, string.Empty), cancellationToken);
#endif
                }
                break;
            case ReadMode.ForceRead:
#if NET6_0_OR_GREATER
                await File.WriteAllTextAsync(filePath, string.Empty, cancellationToken);
#else
                await Task.Run(() => File.WriteAllText(filePath, string.Empty), cancellationToken);
#endif
                break;
        }

        if (File.Exists(filePath))
        {
#if NET6_0_OR_GREATER
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
#else
            var content = await Task.Run(() => File.ReadAllText(filePath), cancellationToken);
#endif
            // Parse the content using AtrcParser
            var parser = new AtrcParser();
            parser.ParseContent(content, this);
        }
    }

    /// <summary>
    /// Reads the value of a variable.
    /// </summary>
    /// <param name="variableName">The name of the variable.</param>
    /// <returns>The value of the variable, or null if not found.</returns>
    public string? ReadVariable(string variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
            return null;

        _variables.TryGetValue(variableName, out var variable);
        return variable?.Value;
    }

    /// <summary>
    /// Reads the value of a key in a block.
    /// </summary>
    /// <param name="blockName">The name of the block.</param>
    /// <param name="keyName">The name of the key.</param>
    /// <returns>The value of the key, or null if not found.</returns>
    public string? ReadKey(string blockName, string keyName)
    {
        if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(keyName))
            return null;

        if (_blocks.TryGetValue(blockName, out var block))
        {
            var key = block.FindKey(keyName);
            return key?.Value;
        }

        return null;
    }

    /// <summary>
    /// Checks if a block exists.
    /// </summary>
    /// <param name="blockName">The name of the block.</param>
    /// <returns>True if the block exists; otherwise, false.</returns>
    public bool DoesExistBlock(string blockName)
    {
        if (string.IsNullOrWhiteSpace(blockName))
            return false;

        return _blocks.ContainsKey(blockName);
    }

    /// <summary>
    /// Checks if a variable exists.
    /// </summary>
    /// <param name="variableName">The name of the variable.</param>
    /// <returns>True if the variable exists; otherwise, false.</returns>
    public bool DoesExistVariable(string variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
            return false;

        return _variables.ContainsKey(variableName);
    }

    /// <summary>
    /// Checks if a key exists in a block.
    /// </summary>
    /// <param name="blockName">The name of the block.</param>
    /// <param name="keyName">The name of the key.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    public bool DoesExistKey(string blockName, string keyName)
    {
        if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(keyName))
            return false;

        if (_blocks.TryGetValue(blockName, out var block))
        {
            return block.FindKey(keyName) != null;
        }

        return false;
    }

    /// <summary>
    /// Checks if a variable is public.
    /// </summary>
    /// <param name="variableName">The name of the variable.</param>
    /// <returns>True if the variable is public; otherwise, false.</returns>
    public bool IsPublic(string variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
            return false;

        _variables.TryGetValue(variableName, out var variable);
        return variable?.IsPublic ?? false;
    }

    /// <summary>
    /// Adds a new block to the file.
    /// </summary>
    /// <param name="blockName">The name of the block.</param>
    /// <returns>True if the block was added; false if it already exists.</returns>
    public bool AddBlock(string blockName)
    {
        if (string.IsNullOrWhiteSpace(blockName))
            return false;

        var block = new AtrcBlock(blockName);
        return _blocks.TryAdd(blockName, block);
    }

    /// <summary>
    /// Removes a block from the file.
    /// </summary>
    /// <param name="blockName">The name of the block to remove.</param>
    /// <returns>True if the block was removed; otherwise, false.</returns>
    public bool RemoveBlock(string blockName)
    {
        if (string.IsNullOrWhiteSpace(blockName))
            return false;

        return _blocks.TryRemove(blockName, out _);
    }

    /// <summary>
    /// Adds a new variable to the file.
    /// </summary>
    /// <param name="variableName">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <param name="isPublic">Whether the variable is public.</param>
    /// <returns>True if the variable was added; false if it already exists.</returns>
    public bool AddVariable(string variableName, string value, bool isPublic = true)
    {
        if (string.IsNullOrWhiteSpace(variableName))
            return false;

        var variable = new AtrcVariable(variableName, value ?? string.Empty, isPublic);
        return _variables.TryAdd(variableName, variable);
    }

    /// <summary>
    /// Removes a variable from the file.
    /// </summary>
    /// <param name="variableName">The name of the variable to remove.</param>
    /// <returns>True if the variable was removed; otherwise, false.</returns>
    public bool RemoveVariable(string variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
            return false;

        return _variables.TryRemove(variableName, out _);
    }

    /// <summary>
    /// Modifies the value of an existing variable.
    /// </summary>
    /// <param name="variableName">The name of the variable.</param>
    /// <param name="value">The new value.</param>
    /// <returns>True if the variable was modified; false if it doesn't exist.</returns>
    public bool ModifyVariable(string variableName, string value)
    {
        if (string.IsNullOrWhiteSpace(variableName))
            return false;

        if (_variables.TryGetValue(variableName, out var variable))
        {
            lock (_lockObject)
            {
                variable.Value = value ?? string.Empty;
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds a key to a block.
    /// </summary>
    /// <param name="blockName">The name of the block.</param>
    /// <param name="keyName">The name of the key.</param>
    /// <param name="value">The value of the key.</param>
    /// <returns>True if the key was added; otherwise, false.</returns>
    public bool AddKey(string blockName, string keyName, string value)
    {
        if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(keyName))
            return false;

        if (_blocks.TryGetValue(blockName, out var block))
        {
            var key = new AtrcKey(keyName, value ?? string.Empty);
            return block.AddKey(key);
        }

        return false;
    }

    /// <summary>
    /// Removes a key from a block.
    /// </summary>
    /// <param name="blockName">The name of the block.</param>
    /// <param name="keyName">The name of the key to remove.</param>
    /// <returns>True if the key was removed; otherwise, false.</returns>
    public bool RemoveKey(string blockName, string keyName)
    {
        if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(keyName))
            return false;

        if (_blocks.TryGetValue(blockName, out var block))
        {
            return block.RemoveKey(keyName);
        }

        return false;
    }

    /// <summary>
    /// Modifies the value of a key in a block.
    /// </summary>
    /// <param name="blockName">The name of the block.</param>
    /// <param name="keyName">The name of the key.</param>
    /// <param name="value">The new value.</param>
    /// <returns>True if the key was modified; false if it doesn't exist.</returns>
    public bool ModifyKey(string blockName, string keyName, string value)
    {
        if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(keyName))
            return false;

        if (_blocks.TryGetValue(blockName, out var block))
        {
            var key = block.FindKey(keyName);
            if (key != null)
            {
                lock (_lockObject)
                {
                    key.Value = value ?? string.Empty;
                }
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="AtrcFileData"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="AtrcFileData"/> class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _variables.Clear();
                _blocks.Clear();
            }

            _disposed = true;
        }
    }
}