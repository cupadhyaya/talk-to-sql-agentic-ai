using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace AgenticAIDemo.Services;

public sealed class SqlServerService
{
    private readonly string _connectionString;

    public SqlServerService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetSchemaDescriptionAsync()
    {
        const string schemaQuery = """
            SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = 'dbo'
              AND TABLE_NAME IN ('Customers', 'Orders')
            ORDER BY TABLE_NAME, ORDINAL_POSITION
            """;

        var rows = await ExecuteSafeQueryAsync(schemaQuery);
        if (rows.Count == 0)
        {
            return "No schema found. Please run Scripts/SetupDatabase.sql first.";
        }

        return string.Join(Environment.NewLine, rows);
    }

    public async Task<List<string>> ExecuteSafeQueryAsync(string sql)
    {
        var cleaned = sql.Trim().TrimEnd(';');

        if (!IsReadOnlySelectQuery(cleaned))
        {
            throw new InvalidOperationException(
                "Only read-only SELECT queries are allowed. No INSERT, UPDATE, DELETE, or DROP.");
        }

        var results = new List<string>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(cleaned, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var columnCount = reader.FieldCount;
        var headers = Enumerable.Range(0, columnCount)
            .Select(i => reader.GetName(i))
            .ToArray();

        results.Add(string.Join(" | ", headers));
        results.Add(new string('-', Math.Min(80, headers.Sum(h => h.Length + 3))));

        var rowCount = 0;
        while (await reader.ReadAsync() && rowCount < 50)
        {
            var values = new string[columnCount];
            for (var i = 0; i < columnCount; i++)
            {
                values[i] = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString() ?? "";
            }

            results.Add(string.Join(" | ", values));
            rowCount++;
        }

        if (rowCount == 0)
        {
            results.Add("(No rows returned)");
        }
        else if (rowCount == 50)
        {
            results.Add("(Results limited to 50 rows)");
        }

        return results;
    }

    private static bool IsReadOnlySelectQuery(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return false;
        }

        var normalized = Regex.Replace(sql, @"\s+", " ", RegexOptions.None).Trim();

        if (!normalized.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string[] blockedKeywords =
        [
            "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "CREATE",
            "TRUNCATE", "EXEC", "EXECUTE", "MERGE", "GRANT", "REVOKE",
            "INTO", ";" 
        ];

        foreach (var keyword in blockedKeywords)
        {
            if (keyword == "INTO" && normalized.Contains(" INTO ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (keyword != "INTO" && keyword != ";" &&
                Regex.IsMatch(normalized, $@"\b{keyword}\b", RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        if (normalized.Contains(';'))
        {
            return false;
        }

        return true;
    }
}
