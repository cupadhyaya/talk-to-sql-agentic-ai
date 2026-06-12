using AgenticAIDemo.Models;
using AgenticAIDemo.Services;
using Microsoft.Extensions.Configuration;

Console.OutputEncoding = System.Text.Encoding.UTF8;

PrintWelcome();

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = new AppSettings();
config.Bind(settings);

var sql = new SqlServerService(settings.SqlServer.ConnectionString);
var localFallback = new LocalFallbackService();

var dbOk = await sql.TestConnectionAsync();
if (!dbOk)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n[!] Could not connect to SQL Server.");
    Console.WriteLine("    1. Make sure SQL Server / LocalDB is running");
    Console.WriteLine("    2. Run Scripts/SetupDatabase.sql in SSMS");
    Console.WriteLine("    3. Check ConnectionString in appsettings.json");
    Console.ResetColor();
    return;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\n[OK] Connected to SQL Server (AgenticAIDemo database)");
Console.ResetColor();

var useGemini = false;
GeminiService? gemini = null;

if (!string.IsNullOrWhiteSpace(settings.Gemini.ApiKey) &&
    settings.Gemini.ApiKey != "YOUR_GEMINI_API_KEY_HERE")
{
    gemini = new GeminiService(settings.Gemini.ApiKey, settings.Gemini.Model);
    useGemini = gemini.HasValidApiKey();
}

if (useGemini)
{
    Console.WriteLine($"[OK] Using Google Gemini ({settings.Gemini.Model})");
}
else if (settings.Agent.UseLocalFallback)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("[i] No Gemini API key found — using LOCAL FALLBACK mode");
    Console.WriteLine("    Add your key to appsettings.json to use real Gemini AI");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("[!] Gemini API key required. Set it in appsettings.json");
    Console.ResetColor();
    return;
}

var agent = new AgentService(
    gemini,
    localFallback,
    sql,
    settings.Agent.MaxSteps,
    useGemini);

PrintHelp();

while (true)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("You: ");
    Console.ResetColor();

    var input = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye! Good luck with your interview.");
        break;
    }

    if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
    {
        PrintHelp();
        continue;
    }

    if (input.Equals("schema", StringComparison.OrdinalIgnoreCase))
    {
        var schema = await sql.GetSchemaDescriptionAsync();
        Console.WriteLine("\n--- Database Schema ---");
        Console.WriteLine(schema);
        continue;
    }

    Console.WriteLine("\nAgent is thinking...");

    try
    {
        var result = await agent.RunAsync(input);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\n--- Agent Steps (ReAct Loop) ---");
        foreach (var step in result.Steps)
        {
            Console.WriteLine($"  Step {step.StepNumber}");
            Console.WriteLine($"    Think : {step.Thought}");
            Console.WriteLine($"    Action: {step.Action}");
            if (!string.IsNullOrWhiteSpace(step.ActionInput))
            {
                Console.WriteLine($"    Input : {step.ActionInput}");
            }
            if (!string.IsNullOrWhiteSpace(step.Observation))
            {
                Console.WriteLine($"    Result: {step.Observation}");
            }
        }
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nAgent: " + result.FinalAnswer);
        Console.ResetColor();

        if (result.UsedDatabase)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("(Answer used live data from SQL Server)");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}

static void PrintWelcome()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("""
    
    ========================================================
      AGENTIC AI DEMO
      C# Console + Google Gemini + SQL Server
    ========================================================
    """);
    Console.ResetColor();
}

static void PrintHelp()
{
    Console.WriteLine("""
    
    Try asking:
      - How many customers do we have?
      - Who are our gold members?
      - What is the total sales amount?
      - Which city has the most customers?
      - Show recent orders

    Commands: help | schema | exit
    """);
}
