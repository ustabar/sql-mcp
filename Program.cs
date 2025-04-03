using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using QuickstartServer.Services;
using QuickstartServer.Tools;

// Create application builder
var builder = Host.CreateEmptyApplicationBuilder(settings: null);

// Add MCP server
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Register services
builder.Services.AddSingleton<SqlConnectionService>();

// Build the application
var app = builder.Build();

// Configure services
var sqlConnectionService = app.Services.GetRequiredService<SqlConnectionService>();
SqlServerTools.Configure(sqlConnectionService);

// Start the application
await app.RunAsync();