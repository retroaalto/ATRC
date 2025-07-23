using System;

namespace Atrc.Core.Models;

/// <summary>
/// Represents a variable in an ATRC file.
/// </summary>
public class AtrcVariable
{
    private string _name = string.Empty;
    private string _value = string.Empty;

    /// <summary>
    /// Gets or sets the name of the variable.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is null, empty, or whitespace.</exception>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Variable name cannot be null or whitespace.", nameof(Name));
            _name = value;
        }
    }

    /// <summary>
    /// Gets or sets the value of the variable.
    /// </summary>
    public string Value
    {
        get => _value;
        set => _value = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the variable is public.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtrcVariable"/> class.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <param name="isPublic">A value indicating whether the variable is public.</param>
    public AtrcVariable(string name, string value, bool isPublic = true)
    {
        Name = name;
        Value = value;
        IsPublic = isPublic;
    }
}