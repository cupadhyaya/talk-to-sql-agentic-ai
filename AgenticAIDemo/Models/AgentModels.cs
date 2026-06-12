namespace AgenticAIDemo.Models;

public sealed record AgentStep
{
    public int StepNumber { get; init; }
    public string Thought { get; init; } = string.Empty;
    public string? Action { get; init; }
    public string? ActionInput { get; init; }
    public string? Observation { get; init; }
}

public sealed class AgentResponse
{
    public string FinalAnswer { get; init; } = string.Empty;
    public List<AgentStep> Steps { get; init; } = [];
    public bool UsedDatabase { get; init; }
}

public sealed class GeminiDecision
{
    public string Thought { get; init; } = string.Empty;
    public string Action { get; init; } = "answer_user";
    public string ActionInput { get; init; } = string.Empty;
}
