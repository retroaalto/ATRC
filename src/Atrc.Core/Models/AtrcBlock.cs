using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Atrc.Core.Models;

/// <summary>
/// Represents a block in an ATRC file, which contains a collection of keys.
/// </summary>
public class AtrcBlock
{
    private string _name = string.Empty;
    private readonly ConcurrentDictionary<string, AtrcKey> _keys = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the name of the block.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is null, empty, or whitespace.</exception>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Block name cannot be null or whitespace.", nameof(Name));
            _name = value;
        }
    }

    /// <summary>
    /// Gets the collection of keys in the block.
    /// </summary>
    public ICollection<AtrcKey> Keys => _keys.Values;

    /// <summary>
    /// Initializes a new instance of the <see cref="AtrcBlock"/> class.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    public AtrcBlock(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Adds a key to the block.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <returns>True if the key was added, false if a key with the same name already exists.</returns>
    public bool AddKey(AtrcKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        return _keys.TryAdd(key.Name, key);
    }

    /// <summary>
    /// Removes a key from the block.
    /// </summary>
    /// <param name="keyName">The name of the key to remove.</param>
    /// <returns>True if the key was removed, false otherwise.</returns>
    public bool RemoveKey(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName)) return false;
        return _keys.TryRemove(keyName, out _);
    }

    /// <summary>
    /// Finds a key by its name.
    /// </summary>
    /// <param name="keyName">The name of the key to find.</param>
    /// <returns>The <see cref="AtrcKey"/> if found; otherwise, null.</returns>
    public AtrcKey? FindKey(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName)) return null;
        _keys.TryGetValue(keyName, out var key);
        return key;
    }
}