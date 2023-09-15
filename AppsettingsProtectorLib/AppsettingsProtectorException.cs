using System;

namespace AppsettingsProtector;

/// <summary>
/// An error that is specific to this library (<see cref="AppsettingsProtector"/>)
/// </summary>
public class AppsettingsProtectorException: Exception
{
    /// <inheritdoc />
    public AppsettingsProtectorException(string message) : base(message) { }
    
    /// <inheritdoc />
    public AppsettingsProtectorException(string message, Exception? innerException) : base(message, innerException) { }
}