# SQL MCP Project - Technical Documentation Mind Map

---

## 1. Technology Stack

- **Programming Languages**
  - C# (Primary)
  - JSON (for configuration and data exchange)
  - Markdown (for documentation)
- **Frameworks and Libraries**
  - .NET 9.0
  - Microsoft.Data.SqlClient
  - Microsoft.Extensions.Hosting
  - ModelContextProtocol (MCP)
  - ModelContextProtocol.Server
- **Architecture Patterns**
  - Dependency Injection (DI)
  - Singleton Service Pattern
  - Static Tool Registration
- **Infrastructure Components**
  - SQL Server / Azure SQL Database
  - Environment Variables for configuration
  - VS Code with GitHub Copilot Agent Mode
- **Version Control & Deployment Tools**
  - Git (implied by GitHub repo)
  - .NET CLI (`dotnet build`, `dotnet run`)
  - VS Code settings for MCP server integration

---

## 2. Code Architecture

- **System Components**
  - `Program.cs`: Application entry, DI, MCP server bootstrap
  - `Services/SqlConnectionService.cs`: SQL connection management
  - `Tools/SqlServerTools.cs`: MCP tool for SQL execution
  - `Data/prompt-tr.txt`: LLM prompt template
  - Configuration: `.mcp.json`, `appsettings.json`
- **Design Patterns Used**
  - Dependency Injection (for service registration)
  - Singleton (for `SqlConnectionService`)
  - Static Class/Method (for tool registration)
- **Data Flow Diagrams**
  - User Query → LLM → JSON → MCP Tool → SQL → Result → LLM → User
  - See `sample001.md` and `test.excalidraw` for visual diagrams
- **Integration Points**
  - MCP protocol integration with GitHub Copilot
  - SQL Server/Azure SQL via connection string
  - VS Code Agent Mode via `.mcp.json` and settings

---

## 3. Function Documentation

### `SqlConnectionService()`
- **Description:** Constructor, reads connection string from environment
- **Input:** None
- **Return:** Instance of service
- **Usage:** Registered as singleton in DI
- **Dependencies:** Environment variable `SQLCONNECTIONSTRING`
- **Complexity:** O(1)

### `Task<SqlConnection> GetConnectionAsync()`
- **Description:** Opens and returns a SQL connection
- **Input:** None
- **Return:** Open `SqlConnection`
- **Usage:** Used by tools to execute queries
- **Dependencies:** Microsoft.Data.SqlClient
- **Complexity:** O(1) (network I/O)

### `void Configure(SqlConnectionService sqlConnectionService)`
- **Description:** Injects the connection service into static tool class
- **Input:** `SqlConnectionService`
- **Return:** None
- **Usage:** Called at startup
- **Dependencies:** DI container
- **Complexity:** O(1)

### `Task<string> ProcessSqlJsonInput(string jsonInput)`
- **Description:** Parses JSON, executes SQL if `komut=E`, returns results
- **Input:** `jsonInput: string` (expects fields `ozet`, `sorgu`, `komut`)
- **Return:** Query results or error message as string
- **Usage:** MCP tool entry point
- **Dependencies:** System.Text.Json, SqlConnectionService
- **Complexity:** O(N) for result formatting

### `private static (string ozet, string sorgu, string komut) ParseJsonInput(string jsonInput)`
- **Description:** Parses JSON into tuple
- **Input:** `jsonInput: string`
- **Return:** Tuple of strings
- **Usage:** Internal to tool
- **Dependencies:** System.Text.Json
- **Complexity:** O(1)

### `private static async Task<string> ExecuteSqlQueryWithService(string sorgu)`
- **Description:** Executes SQL using injected service
- **Input:** `sorgu: string`
- **Return:** Query results as string
- **Usage:** Internal to tool
- **Dependencies:** SqlConnectionService
- **Complexity:** O(N) for result rows

### `private static async Task<string> ExecuteDirectSqlQuery(string sorgu)`
- **Description:** Executes SQL using direct connection (fallback)
- **Input:** `sorgu: string`
- **Return:** Query results as string
- **Usage:** Internal to tool
- **Dependencies:** Microsoft.Data.SqlClient
- **Complexity:** O(N)

### `private static string FormatQueryResults(DbDataReader reader)`
- **Description:** Formats results as table string
- **Input:** `DbDataReader`
- **Return:** String
- **Usage:** Internal to tool
- **Dependencies:** System.Text.StringBuilder
- **Complexity:** O(N*M) (rows x columns)

---

## 4. Business Process Flows

- **Core Workflows**
  1. Application starts (`dotnet run`)
  2. DI and MCP server configured
  3. `SqlConnectionService` initialized (reads env var)
  4. MCP tool invoked with JSON input
  5. JSON parsed for `ozet`, `sorgu`, `komut`
  6. If `komut == "E"`, SQL executed
  7. Results formatted and returned
- **Decision Points**
  - If `komut != "E"`, SQL not executed
  - If service not configured, fallback to direct connection
- **Error Handling**
  - Returns error messages as strings (JSON parse, SQL errors)
  - Throws exceptions for missing connection string
- **User Interactions**
  - User queries via Copilot Agent Mode
  - Receives formatted results or error messages
- **System Interactions**
  - LLM generates JSON for tool
  - Tool executes SQL and returns results

---

## 5. Implementation Analysis

- **Code Quality Metrics**
  - Clear separation of concerns
  - Uses DI and static tool registration
  - Lacks unit/integration tests
- **Potential Improvements**
  - Structured error responses (JSON)
  - Parameterized queries for security
  - Support for multiple configuration sources
- **Technical Debt**
  - Only supports SQL Server
  - Minimal logging (uses Debug.WriteLine)
  - No test coverage
- **Security Considerations**
  - No SQL injection protection
  - Connection string in environment variable only
- **Performance Bottlenecks**
  - Synchronous result formatting for large result sets

---

## 6. Recommendations

- **Critical Issues**
  - Add SQL injection protection (parameterized queries)
  - Improve error handling (structured JSON)
- **Architectural Improvements**
  - Abstract DB layer for multi-DB support
  - Use ILogger for structured logging
- **Code Refactoring Suggestions**
  - Move result formatting to support JSON output
  - Remove direct connection fallback if not needed
- **Testing Coverage**
  - Add unit and integration tests for all components
- **Documentation Gaps**
  - Add sequence diagrams and more usage examples
  - Document security and configuration best practices

---

## Mind Map (Textual)

- SQL MCP Project
  - Technology Stack
    - C#, .NET 9.0, SqlClient, MCP, Copilot
  - Architecture
    - Program.cs (Entry, DI, MCP)
    - Services/SqlConnectionService.cs (DB)
    - Tools/SqlServerTools.cs (Tool)
    - Data/prompt-tr.txt (Prompt)
  - Functions
    - Configure(), ProcessSqlJsonInput(), ParseJsonInput(), ExecuteSqlQueryWithService(), ExecuteDirectSqlQuery(), FormatQueryResults()
  - Business Flow
    - Startup → DI → MCP → Tool Call → SQL → Result Formatting
  - Implementation
    - Error handling, security, extensibility, logging, testing
  - Recommendations
    - Security, error handling, abstraction, logging, tests, docs

---
