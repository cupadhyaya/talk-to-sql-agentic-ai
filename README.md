# Agentic AI - Simple Guide for Everyone

Simple spoken English. You do not need to be an engineer to understand this.

This repo has two parts:

1. `docs/AgenticAI-Simple-Guide.html` and `.pdf` - the beginner-friendly guide.
2. `AgenticAIDemo/` - a small C# console app using Google Gemini and SQL Server.

## What Is Agentic AI?

Agentic AI is AI that can do a job step by step.

A normal chatbot mostly gives text answers. An agentic AI can also use tools. For example, it can check a database, read a file, call an API, or run a small workflow.

Simple meaning:

```text
Agentic AI = AI brain + tools + step-by-step loop + safety rules
```

Example:

```text
User: How many gold customers do we have?

Agent:
1. I need real customer data.
2. I will check SQL Server.
3. I will run a safe SELECT query.
4. I will read the result.
5. I will answer in simple English.
```

## How The Demo Works

The app follows the ReAct loop:

```text
Read request -> Think -> Act -> See result -> Repeat -> Answer
```

When you ask a question:

1. `Program.cs` loads `appsettings.json`.
2. The app checks SQL Server.
3. The app chooses Gemini or Local Fallback.
4. `AgentService` runs the loop.
5. Gemini or Local Fallback chooses `run_sql` or `answer_user`.
6. `SqlServerService` runs only safe `SELECT` queries.
7. The final answer is printed in the console.

## What If Local Data Is Missing?

If Gemini API key is missing, the app can use Local Fallback mode.

If SQL Server is not running, the app stops and tells you to fix the database setup.

If the user asks for data that is not in the database, the agent should not guess. It should say that no matching data was found.

If internet is down and Gemini is selected, the Gemini call fails. You can use Local Fallback mode to continue learning.

## Project Structure

```text
Interview/
  README.md
  docs/
    AgenticAI-Simple-Guide.html
    AgenticAI-Simple-Guide.pdf
  AgenticAIDemo/
    AgenticAIDemo.csproj
    Program.cs
    appsettings.json
    Models/
      AppSettings.cs
      AgentModels.cs
    Services/
      AgentService.cs
      GeminiService.cs
      SqlServerService.cs
      LocalFallbackService.cs
    Scripts/
      SetupDatabase.sql
```

## Build The App From Scratch

### 1. Install tools

- .NET 8 SDK
- SQL Server Express or LocalDB
- SQL Server Management Studio
- Gemini API key, optional

### 2. Create the project

```powershell
mkdir AgenticAIDemo
cd AgenticAIDemo
dotnet new console
```

### 3. Add packages

```powershell
dotnet add package Microsoft.Data.SqlClient
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Configuration.Binder
dotnet add package Newtonsoft.Json
```

### 4. Create database

Run this script in SQL Server Management Studio:

```text
AgenticAIDemo/Scripts/SetupDatabase.sql
```

It creates:

- `AgenticAIDemo` database
- `Customers` table
- `Orders` table
- Sample customer and order records

### 5. Create models

- `Models/AppSettings.cs` - settings for Gemini, SQL Server, and Agent rules.
- `Models/AgentModels.cs` - shapes for agent step, final response, and AI decision.

### 6. Create services

- `Services/SqlServerService.cs` - connects to SQL Server and runs safe SELECT queries.
- `Services/GeminiService.cs` - talks to Google Gemini.
- `Services/LocalFallbackService.cs` - simple offline brain.
- `Services/AgentService.cs` - runs the main ReAct loop.

### 7. Create appsettings.json

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-2.5-flash"
  },
  "SqlServer": {
    "ConnectionString": "Server=localhost;Database=AgenticAIDemo;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Agent": {
    "MaxSteps": 5,
    "UseLocalFallback": true
  }
}
```

Keep real API keys and passwords private.

### 8. Create Program.cs

`Program.cs`:

- Prints the welcome message.
- Loads settings.
- Tests SQL Server.
- Chooses Gemini or Local Fallback.
- Creates `AgentService`.
- Runs the chat loop.
- Handles `help`, `schema`, and `exit`.

### 9. Run the app

```powershell
cd AgenticAIDemo
dotnet run
```

Try:

- How many customers do we have?
- Who are our gold members?
- What is the total sales amount?
- Which city has the most customers?
- Show recent orders.

## Read The Full Guide

Open:

```text
docs/AgenticAI-Simple-Guide.html
```

The PDF version is:

```text
docs/AgenticAI-Simple-Guide.pdf
```

## Interview Line

"I built a C# Agentic AI demo that uses Gemini and SQL Server to answer questions from real data using a safe ReAct loop."
