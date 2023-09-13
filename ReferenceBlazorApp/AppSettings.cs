namespace ReferenceBlazorApp;

public class AppSettings
{
    public Logging? Logging { get; set; }
    public string? AllowedHosts { get; set; }
    
    public string? Secret { get; set; }

    public ConnectionStrings? ConnectionStrings { get; set; }
}

public class ConnectionStrings
{
    public string? Google { get; set; }
    public string? Microsoft { get; set; }
}

public class Logging
{
    public Loglevel? LogLevel { get; set; }
}

public class Loglevel
{
    public string? Default { get; set; }
    public string? MicrosoftAspNetCore { get; set; }
}