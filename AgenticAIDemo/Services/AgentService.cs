using AgenticAIDemo.Models;

namespace AgenticAIDemo.Services;

public sealed class AgentService
{
    private readonly GeminiService? _gemini;
    private readonly LocalFallbackService _localFallback;
    private readonly SqlServerService _sql;
    private readonly int _maxSteps;
    private readonly bool _useGemini;

    public AgentService(
        GeminiService? gemini,
        LocalFallbackService localFallback,
        SqlServerService sql,
        int maxSteps,
        bool useGemini)
    {
        _gemini = gemini;
        _localFallback = localFallback;
        _sql = sql;
        _maxSteps = maxSteps;
        _useGemini = useGemini;
    }

    public async Task<AgentResponse> RunAsync(string userQuestion)
    {
        var steps = new List<AgentStep>();
        var schema = await _sql.GetSchemaDescriptionAsync();
        var usedDatabase = false;
        string finalAnswer = "Sorry, I could not complete your request.";

        for (var step = 1; step <= _maxSteps; step++)
        {
            var decision = _useGemini && _gemini != null
                ? await _gemini.GetNextStepAsync(userQuestion, schema, steps)
                : _localFallback.GetNextStep(userQuestion, steps);

            var agentStep = new AgentStep
            {
                StepNumber = step,
                Thought = decision.Thought,
                Action = decision.Action,
                ActionInput = decision.ActionInput
            };

            if (decision.Action.Equals("run_sql", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var rows = await _sql.ExecuteSafeQueryAsync(decision.ActionInput);
                    agentStep = agentStep with { Observation = string.Join(Environment.NewLine, rows) };
                    usedDatabase = true;
                }
                catch (Exception ex)
                {
                    agentStep = agentStep with { Observation = $"SQL error: {ex.Message}" };
                }

                steps.Add(agentStep);
                continue;
            }

            if (decision.Action.Equals("answer_user", StringComparison.OrdinalIgnoreCase))
            {
                agentStep = agentStep with { Observation = "Answer ready." };
                steps.Add(agentStep);
                finalAnswer = decision.ActionInput;
                break;
            }

            agentStep = agentStep with { Observation = "Unknown action. Stopping." };
            steps.Add(agentStep);
            break;
        }

        if (steps.Count > 0 && steps[^1].Action != "answer_user")
        {
            finalAnswer = "I reached the step limit. Please try a simpler question.";
        }

        return new AgentResponse
        {
            FinalAnswer = finalAnswer,
            Steps = steps,
            UsedDatabase = usedDatabase
        };
    }
}
