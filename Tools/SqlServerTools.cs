using ModelContextProtocol.Server;
using QuickstartServer.Services;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuickstartServer.Tools;

[McpServerToolType]
public static class SqlServerTools
{
    private static SqlConnectionService? _sqlConnectionService;

    /// <summary>
    /// Configures the service with dependency injection
    /// </summary>
    public static void Configure(SqlConnectionService sqlConnectionService)
    {
        _sqlConnectionService = sqlConnectionService ?? throw new ArgumentNullException(nameof(sqlConnectionService));
        Debug.WriteLine("SqlConnectionService configured successfully");
    }

    /// <summary>
    /// Process JSON input and execute SQL command if komut=E
    /// </summary>
    [McpServerTool, Description("Process JSON input and execute SQL command if komut=E")]
    public static async Task<string> ProcessSqlJsonInput(
        [Description("JSON input with ozet, sorgu and komut fields")] string jsonInput)
    {
        if (string.IsNullOrEmpty(jsonInput))
        {
            return "Error: JSON input is empty or null";
        }

        try
        {
            Debug.WriteLine("Starting ProcessSqlJsonInput...");
            Debug.WriteLine($"Input JSON: {jsonInput}");

            var (ozet, sorgu, komut) = ParseJsonInput(jsonInput);
            Debug.WriteLine($"Parsed JSON - Ozet: {ozet}, Sorgu: {sorgu}, Komut: {komut}");

            if (!komut.Equals("E", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine("SQL command not executed.");
                return $"SQL command not executed (komut={komut}).\nQuery: {sorgu}";
            }

            if (_sqlConnectionService == null)
            {
                Debug.WriteLine("SqlConnectionService is not configured. Using direct connection.");
                return await ExecuteDirectSqlQuery(sorgu);
            }

            return await ExecuteSqlQueryWithService(sorgu);
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"JSON parsing error: {ex.Message}");
            return $"Error parsing JSON input: {ex.Message}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            return $"Error processing SQL JSON input: {ex.Message}";
        }
    }

    /// <summary>
    /// Parses JSON input into components
    /// </summary>
    private static (string ozet, string sorgu, string komut) ParseJsonInput(string jsonInput)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonInput);
        return (
            jsonElement.TryGetProperty("ozet", out var ozetElement) ? ozetElement.GetString() ?? "" : "",
            jsonElement.TryGetProperty("sorgu", out var sorguElement) ? sorguElement.GetString() ?? "" : "",
            jsonElement.TryGetProperty("komut", out var komutElement) ? komutElement.GetString() ?? "" : ""
        );
    }

    /// <summary>
    /// Executes SQL query using the SqlConnectionService
    /// </summary>
    private static async Task<string> ExecuteSqlQueryWithService(string sorgu)
    {
        if (_sqlConnectionService == null)
        {
            throw new InvalidOperationException("SqlConnectionService is not configured");
        }

        using var connection = await _sqlConnectionService.GetConnectionAsync();
        Debug.WriteLine("Database connection established through service.");

        using var command = connection.CreateCommand();
        command.CommandText = sorgu;
        command.CommandTimeout = 30; // Set a reasonable timeout

        using var reader = await command.ExecuteReaderAsync();
        Debug.WriteLine("SQL command executed.");

        if (!reader.HasRows)
        {
            return "No rows returned.";
        }

        return FormatQueryResults(reader);
    }

    /// <summary>
    /// Direct execution for fallback when service is not available
    /// </summary>
    private static async Task<string> ExecuteDirectSqlQuery(string sorgu)
    {
        // This is a fallback method and should be avoided in production
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(
            "Server=***.database.windows.net,1433;Database=sqlforopenai;User Id=***;Password=***;TrustServerCertificate=True;");
        
        await connection.OpenAsync();
        Debug.WriteLine("Database connection established directly.");

        using var command = connection.CreateCommand();
        command.CommandText = sorgu;
        command.CommandTimeout = 30;

        using var reader = await command.ExecuteReaderAsync();
        Debug.WriteLine("SQL command executed.");

        if (!reader.HasRows)
        {
            Debug.WriteLine("No rows returned.");
            return "No rows returned.";
        }

        return FormatQueryResults(reader);
    }

    /// <summary>
    /// Formats query results into a readable table format
    /// </summary>
    private static string FormatQueryResults(DbDataReader reader)
    {
        var results = new StringBuilder();
        var fieldCount = reader.FieldCount;

        WriteHeader(results, reader, fieldCount);

        while (reader.Read())
        {
            WriteRow(results, reader, fieldCount);
        }

        Debug.WriteLine("Query results processed.");
        return results.ToString();
    }

    /// <summary>
    /// Writes table header from reader metadata
    /// </summary>
    private static void WriteHeader(StringBuilder results, DbDataReader reader, int fieldCount)
    {
        for (int i = 0; i < fieldCount; i++)
        {
            if (i > 0) results.Append(" | ");
            results.Append(reader.GetName(i));
        }
        results.AppendLine();
        results.AppendLine(new string('-', results.Length));
    }

    /// <summary>
    /// Writes a data row from reader
    /// </summary>
    private static void WriteRow(StringBuilder results, DbDataReader reader, int fieldCount)
    {
        for (int i = 0; i < fieldCount; i++)
        {
            if (i > 0) results.Append(" | ");
            // Using safer async version would be better but this keeps format similar
            results.Append(reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString());
        }
        results.AppendLine();
    }
}