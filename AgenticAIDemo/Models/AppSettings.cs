namespace AgenticAIDemo.Models;

public sealed class AppSettings
{
    public GeminiSettings Gemini { get; set; } = new();
    public SqlServerSettings SqlServer { get; set; } = new();
    public AgentSettings Agent { get; set; } = new();
}

public sealed class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
}

public sealed class SqlServerSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

public sealed class AgentSettings
{
    public int MaxSteps { get; set; } = 5;
    public bool UseLocalFallback { get; set; } = true;
}
