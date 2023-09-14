using System;

namespace AppsettingsProtector;

public class AppsettingsProtectorException: Exception
{
    public AppsettingsProtectorException(string message) : base(message) { }
}