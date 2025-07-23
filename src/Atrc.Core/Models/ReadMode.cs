namespace Atrc.Core.Models;

/// <summary>
/// Specifies the mode for reading an ATRC file.
/// </summary>
public enum ReadMode
{
    /// <summary>
    /// Opens a file for reading only. The file must exist.
    /// Corresponds to ATRC_READ_ONLY.
    /// </summary>
    ReadOnly,

    /// <summary>
    /// Creates a new file if it does not exist, otherwise opens the existing file for reading and writing.
    /// Corresponds to ATRC_CREATE_READ.
    /// </summary>
    CreateAndRead,

    /// <summary>
    /// Forces the creation of a new file, overwriting it if it exists.
    /// Corresponds to ATRC_FORCE_READ.
    /// </summary>
    ForceRead
}