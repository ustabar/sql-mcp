using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace QuickstartServer.Services;

public class SqlConnectionService
{
    private readonly string? _connectionString;

    /// <summary>
    /// Initializes a new instance of the SqlConnectionService class
    /// </summary>
    public SqlConnectionService()
    {
        _connectionString = "Server=sqlforopenaiserver.database.windows.net,1433;Database=sqlforopenai;User Id=barut;Password=Deneme!12345;TrustServerCertificate=True;";
        
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string 'AzureSqlConnection' not found in configuration.");
        }
    }

    /// <summary>
    /// Gets an open SQL connection using the configured connection string
    /// </summary>
    /// <returns>An open SqlConnection</returns>
    /// <exception cref="InvalidOperationException">Thrown if connection string is missing</exception>
    public async Task<SqlConnection> GetConnectionAsync()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string 'AzureSqlConnection' not found.");
        }
        
        var connection = new SqlConnection(_connectionString);
        
        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch (SqlException ex)
        {
            await connection.DisposeAsync();
            throw new InvalidOperationException($"Failed to open SQL connection: {ex.Message}", ex);
        }
    }
}
