# Project Structure: SQL MCP

## Frameworks and Libraries Used

- **.NET 9.0**  
  The project targets .NET 9.0, leveraging the latest features and performance improvements.
- **Microsoft.Data.SqlClient**  
  Used for SQL Server database connectivity.
- **Microsoft.Extensions.Hosting**  
  Provides hosting and dependency injection for the application.
- **ModelContextProtocol**  
  Implements the Model Context Protocol (MCP) for natural language interaction.
- **ModelContextProtocol.Server**  
  Used for MCP server tool integration.

---

## Main Components and Methods

### 1. `Program.cs`
**Purpose:**  
Application entry point. Configures dependency injection, MCP server, and starts the host.

**Key Methods/Flow:**
- **Host.CreateEmptyApplicationBuilder()**  
  Initializes the application builder.
- **AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()**  
  Registers the MCP server and tools for communication.
- **AddSingleton<SqlConnectionService>()**  
  Registers the SQL connection service as a singleton.
- **SqlServerTools.Configure(sqlConnectionService)**  
  Injects the connection service into the tools.
- **await app.RunAsync()**  
  Starts the application.

---

### 2. `Services/SqlConnectionService.cs`
**Purpose:**  
Manages SQL Server database connections.

**Key Methods:**
- **SqlConnectionService()**  
  Constructor. Reads the connection string from the `SQLCONNECTIONSTRING` environment variable.
- **Task<SqlConnection> GetConnectionAsync()**  
  Returns an open SQL connection.
  - **Parameters:** None
  - **Returns:** An open `SqlConnection` object.
  - **Exceptions:** Throws if the connection string is missing or connection fails.

---

### 3. `Tools/SqlServerTools.cs`
**Purpose:**  
Implements MCP tools for SQL interaction.

**Key Methods:**
- **void Configure(SqlConnectionService sqlConnectionService)**  
  Injects the connection service for use in static methods.
  - **Parameters:**  
    - `sqlConnectionService`: The service to manage SQL connections.
- **Task<string> ProcessSqlJsonInput(string jsonInput)**  
  Processes a JSON input, executes SQL if `komut=E`, and returns results.
  - **Parameters:**  
    - `jsonInput`: JSON string with fields `ozet`, `sorgu`, and `komut`.
  - **Returns:**  
    - Query results as a formatted string or error message.
- **(string ozet, string sorgu, string komut) ParseJsonInput(string jsonInput)**  
  Parses the JSON input into its components.
- **Task<string> ExecuteSqlQueryWithService(string sorgu)**  
  Executes a SQL query using the injected connection service.
- **Task<string> ExecuteDirectSqlQuery(string sorgu)**  
  Executes a SQL query using a direct connection (fallback).
- **string FormatQueryResults(DbDataReader reader)**  
  Formats the query results as a readable table.

---

## Business Flow

1. **Startup:**  
   - The application is started via `dotnet run`.
   - Dependency injection and MCP server are configured.

2. **Connection Service:**  
   - `SqlConnectionService` is initialized, reading the connection string from the environment.

3. **MCP Tool Invocation:**  
   - When a natural language query is received, it is processed by `ProcessSqlJsonInput`.
   - The JSON input is parsed for summary (`ozet`), SQL query (`sorgu`), and command flag (`komut`).

4. **SQL Execution:**  
   - If `komut` is `"E"`, the SQL query is executed.
   - If the connection service is available, it is used; otherwise, a direct connection is attempted.

5. **Result Formatting:**  
   - Query results are formatted as a table and returned as a string.

---

## Relation Between Components

- **Program.cs**  
  - Configures and wires up all services and tools.
- **SqlConnectionService**  
  - Provides database connections to `SqlServerTools`.
- **SqlServerTools**  
  - Implements the business logic for processing and executing SQL queries, using the connection service.

**Diagram:**
