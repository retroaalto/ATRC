using System;

namespace Atrc.Core.Models;

/// <summary>
/// Represents a key-value pair within an ATRC block.
/// </summary>
public class AtrcKey
{
    private string _name = string.Empty;
    private string _value = string.Empty;

    /// <summary>
    /// Gets or sets the name of the key.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is null, empty, or whitespace.</exception>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Key name cannot be null or whitespace.", nameof(Name));
            _name = value;
        }
    }

    /// <summary>
    /// Gets or sets the value of the key.
    /// </summary>
    public string Value
    {
        get => _value;
        set => _value = value ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtrcKey"/> class.
    /// </summary>
    /// <param name="name">The name of the key.</param>
    /// <param name="value">The value of the key.</param>
    public AtrcKey(string name, string value)
    {
        Name = name;
        Value = value;
    }
}