# SQL MCP Project Mind Map

## 1. Technologies & Architecture

- **.NET 9.0**
  - Modern, cross-platform runtime for C# applications.
- **Microsoft.Data.SqlClient**
  - SQL Server/Azure SQL connectivity.
- **Microsoft.Extensions.Hosting**
  - Dependency injection, configuration, and hosting.
- **ModelContextProtocol (MCP)**
  - Protocol for natural language to tool invocation.
- **ModelContextProtocol.Server**
  - MCP server implementation and tool registration.
- **GitHub Copilot Agent Mode**
  - Natural language interface for developers.
- **JSON**
  - Used for structured input/output between LLM, tools, and user.

---

## 2. Main Components & Functions

### Program.cs

- **Purpose:** Application entry, DI setup, MCP server bootstrap.
- **Key Flow:**
  - `Host.CreateEmptyApplicationBuilder(settings: null)`
  - `builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()`
  - `builder.Services.AddSingleton<SqlConnectionService>()`
  - `SqlServerTools.Configure(sqlConnectionService)`
  - `await app.RunAsync()`

### Services/SqlConnectionService.cs

- **Purpose:** Manage SQL Server connections.
- **Constructor:**  
  - Reads connection string from `SQLCONNECTIONSTRING` environment variable.
- **Function:**  
  - `Task<SqlConnection> GetConnectionAsync()`
    - **Returns:** Open `SqlConnection`
    - **Throws:** If connection string missing or connection fails.

### Tools/SqlServerTools.cs

- **Purpose:** Implements MCP tools for SQL interaction.
- **Functions:**
  - `void Configure(SqlConnectionService sqlConnectionService)`
    - **Injects** the connection service for static tool methods.
    - **Parameters:** `sqlConnectionService: SqlConnectionService`
  - `Task<string> ProcessSqlJsonInput(string jsonInput)`
    - **Processes** JSON input, executes SQL if `komut=E`, returns results.
    - **Parameters:** `jsonInput: string` (expects fields `ozet`, `sorgu`, `komut`)
    - **Returns:** Query results or error message.
  - `private static (string ozet, string sorgu, string komut) ParseJsonInput(string jsonInput)`
    - **Parses** JSON input into components.
  - `private static async Task<string> ExecuteSqlQueryWithService(string sorgu)`
    - **Executes** SQL using injected connection service.
    - **Parameters:** `sorgu: string`
    - **Returns:** Query results.
  - `private static async Task<string> ExecuteDirectSqlQuery(string sorgu)`
    - **Executes** SQL using direct connection (fallback).
    - **Parameters:** `sorgu: string`
    - **Returns:** Query results.
  - `private static string FormatQueryResults(DbDataReader reader)`
    - **Formats** results as a readable table.

---

## 3. Business Flow

1. **Startup**
   - Application starts via `dotnet run`.
   - DI and MCP server are configured.
2. **Connection Service**
   - `SqlConnectionService` initialized, reads connection string from environment.
3. **MCP Tool Invocation**
   - Natural language query is processed by `ProcessSqlJsonInput`.
   - JSON input is parsed for summary (`ozet`), SQL (`sorgu`), and command flag (`komut`).
4. **SQL Execution**
   - If `komut == "E"`, SQL is executed.
   - Uses connection service if available, else direct connection.
5. **Result Formatting**
   - Results formatted as table and returned as string.

---

## 4. Mind Map (Textual)

- SQL MCP Project
  - Technologies
    - .NET 9.0
    - SqlClient
    - MCP Protocol
    - GitHub Copilot
  - Architecture
    - Program.cs (Entry, DI, MCP Server)
    - Services/
      - SqlConnectionService.cs (DB Connection)
    - Tools/
      - SqlServerTools.cs (MCP Tool)
  - Functions
    - Configure()
    - ProcessSqlJsonInput()
    - ParseJsonInput()
    - ExecuteSqlQueryWithService()
    - ExecuteDirectSqlQuery()
    - FormatQueryResults()
  - Business Flow
    - Startup → DI → MCP Server → Tool Call → SQL Execution → Result Formatting
  - Data Flow
    - User Query → LLM → JSON → MCP Tool → SQL → Result → LLM → User

---

## 5. Gaps & Recommendations

- **Error Handling:**  
  - Error messages are returned as strings; consider structured error responses (JSON).
- **Security:**  
  - No SQL injection protection; recommend parameterized queries or validation.
- **Configuration:**  
  - Only supports environment variable for connection string; consider supporting `appsettings.json` or secrets.
- **Testing:**  
  - No unit/integration tests present.
- **Extensibility:**  
  - Only SQL Server supported; consider abstraction for other DBs.
- **Logging:**  
  - Uses `Debug.WriteLine`; recommend structured logging (ILogger).
- **Result Formatting:**  
  - Only table string output; consider JSON or other formats for downstream processing.
- **Documentation:**  
  - Good, but could add sequence diagrams and more usage examples.

---

## 6. #codebase

- All code is under `c:\Codes\MCP\sqlmcp`
- Main files:
  - `Program.cs`
  - `Services/SqlConnectionService.cs`
  - `Tools/SqlServerTools.cs`
  - `Data/prompt-tr.txt` (prompt for LLM)
  - `README.md`, `sample001.md` (docs)
- Entry point: `Program.cs`
- MCP Tool: `SqlServerTools.cs`
- DB Service: `SqlConnectionService.cs`
