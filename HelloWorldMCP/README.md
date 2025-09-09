# Hello World MCP Server - Complete ETL Analysis Lab

This is a comprehensive Model Context Protocol (MCP) server written in C# that demonstrates essential MCP patterns with advanced ETL trace analysis capabilities. Perfect for learning MCP development while working with real-world Windows performance analysis.

## 🎯 Features

### **4 Essential Tools**
1. **HelloWorld** - Simple string parameter handling and response
2. **GetSystemInfo** - Comprehensive structured JSON object return
3. **AnalyzeTrace** - ETL trace analysis with process discovery and detailed metadata
4. **GetProcessCpuUsage** - Detailed CPU sampling analysis for specific processes by PID

## 🎓 Learning Objectives

This comprehensive lab demonstrates advanced MCP patterns:
- ✅ **Basic string input/output** with parameter validation
- ✅ **Complex JSON object responses** with nested data structures
- ✅ **File-based analysis** using TraceProcessor for ETL traces
- ✅ **Array parameter handling** with process ID arrays
- ✅ **Two-step analysis workflow** (discovery → detailed analysis)
- ✅ **Error handling** with meaningful messages
- ✅ **Real-world integration** with Windows Event Tracing APIs

## 🚀 Quick Start

### Build and Run:
```bash
dotnet build
dotnet run
```

### Test the 4 Tools:

#### Command Line Testing (Basic):
```bash
# List all available tools (should show 4)
echo '{"jsonrpc": "2.0", "id": 1, "method": "tools/list"}' | dotnet run

# Test HelloWorld - Basic string parameter
echo '{"jsonrpc": "2.0", "id": 2, "method": "tools/call", "params": {"name": "HelloWorld", "arguments": {"name": "ETL Developer"}}}' | dotnet run

# Test GetSystemInfo - JSON object response  
echo '{"jsonrpc": "2.0", "id": 3, "method": "tools/call", "params": {"name": "GetSystemInfo", "arguments": {}}}' | dotnet run

# Test AnalyzeTrace - ETL trace analysis (requires valid .etl file)
echo '{"jsonrpc": "2.0", "id": 4, "method": "tools/call", "params": {"name": "AnalyzeTrace", "arguments": {"tracePath": "path/to/trace.etl"}}}' | dotnet run

# Test GetProcessCpuUsage - Detailed CPU analysis by PID array
echo '{"jsonrpc": "2.0", "id": 5, "method": "tools/call", "params": {"name": "GetProcessCpuUsage", "arguments": {"tracePath": "path/to/trace.etl", "processIds": [1272, 980, 1324]}}}' | dotnet run
```

#### VS Code Copilot Integration (Recommended):
1. **Install the MCP extension** in VS Code
2. **Configure MCP server** in your global MCP configuration:
   - Open Command Palette: "MCP: Edit Configuration"
   - Add to your `mcp.json`:
   ```jsonc
   {
     "servers": {
       "hello-world-lab": {
         "type": "stdio",
         "command": "dotnet",
         "args": ["run", "--project", "C:\\path\\to\\HelloWorldMCP\\HelloWorldMCP.csproj"],
         "cwd": "C:\\path\\to\\HelloWorldMCP"
       }
     }
   }
   ```
3. **Restart VS Code** and open Copilot Chat (Ctrl+Shift+I):
   ```
   @workspace Use the HelloWorld tool to greet "ETL Developer"
   @workspace Use GetSystemInfo to check my development environment  
   @workspace Use AnalyzeTrace to analyze an ETL file and show me the top processes
   @workspace Use GetProcessCpuUsage to analyze specific process IDs from the trace
   ```

## 📋 Tool Reference

| Tool | Description | Parameters | Example Response |
|------|-------------|------------|------------------|
| `HelloWorld` | Greet someone by name | `name: string` | `"Hello, ETL Developer! Welcome to the MCP Hello World Lab."` |
| `GetSystemInfo` | Get comprehensive system info | None | Complex JSON with Machine, Runtime, Environment details |
| `AnalyzeTrace` | Analyze ETL trace file | `tracePath: string` | JSON with trace metadata, processes with PIDs, timeline analysis |
| `GetProcessCpuUsage` | Detailed CPU analysis | `tracePath: string, processIds: uint[]` | JSON with detailed CPU sampling data for specific processes |

## 🔧 Advanced ETL Analysis Workflow

### Two-Step Analysis Pattern:
1. **Discovery Phase**: `AnalyzeTrace` → Get overview with process IDs
2. **Deep Dive Phase**: `GetProcessCpuUsage` → Analyze specific processes by PID

The enhanced `AnalyzeTrace` tool now provides:
- **Trace Metadata**: Duration, timeline, total processes
- **Process Discovery**: All processes with PIDs for targeted analysis  
- **Timeline Analysis**: Duration and time range information
- **Process Overview**: CPU, memory, and I/O patterns

The `GetProcessCpuUsage` tool provides:
- **CPU Sampling**: Detailed CPU usage data for specific processes
- **Process Filtering**: Analyze only the processes you're interested in
- **Performance Metrics**: CPU utilization patterns and statistics

### Sample AnalyzeTrace Response:
```json
{
  "traceFile": {
    "path": "C:\\PerfTools\\ETW.XPerf\\testassets\\baselines\\defaulttrace.etl",
    "sizeBytes": 44539467,
    "lastModified": "2024-07-03T14:57:20.7301918-07:00"
  },
  "timeline": {
    "startTime": "2012-12-12T12:06:43.3836955-08:00",
    "endTime": "2012-12-12T12:06:55.5274121-08:00",
    "durationMs": 12143.7166,
    "durationFormatted": "12.14 seconds"
  },
  "processes": {
    "totalCount": 103,
    "uniqueProcessNames": 67,
    "processIds": [4, 8, 156, 892, 1024, 1156, 2048, 3456],
    "topProcesses": [
      {"processName": "svchost.exe", "instanceCount": 15},
      {"processName": "WmiPrvSE.exe", "instanceCount": 6},
      {"processName": "conhost.exe", "instanceCount": 5},
      {"processName": "cmd.exe", "instanceCount": 5},
      {"processName": "TestjobResultExplorer.exe", "instanceCount": 3}
    ]
  }
}
```

### Sample GetProcessCpuUsage Response:
```json
{
  "tracePath": "C:\\PerfTools\\ETW.XPerf\\testassets\\baselines\\defaulttrace.etl",
  "requestedProcessIds": [4, 892, 1024],
  "processAnalysis": [
    {
      "processId": 4,
      "processName": "System", 
      "cpuSampleCount": 245,
      "totalCpuTimeMs": 1250.5,
      "averageCpuPercent": 15.2,
      "peakCpuPercent": 45.8
    },
    {
      "processId": 892,
      "processName": "svchost.exe",
      "cpuSampleCount": 156, 
      "totalCpuTimeMs": 892.3,
      "averageCpuPercent": 8.7,
      "peakCpuPercent": 28.4
    }
  ]
}
```

## 🎯 Key Learning Patterns

### 1. Simple String Parameter Pattern (HelloWorld)
```csharp
[McpServerTool, Description("Greet someone by name and return a personalized message")]
public static string HelloWorld(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Name cannot be empty", nameof(name));

    return $"Hello, {name}! Welcome to the MCP Hello World Lab.";
}
```

### 2. Complex JSON Object Pattern (GetSystemInfo)
```csharp
[McpServerTool, Description("Get comprehensive system information as a structured JSON object")]
public static object GetSystemInfo()
{
    return new
    {
        Timestamp = DateTime.UtcNow,
        Machine = new { /* nested object */ },
        Runtime = new { /* nested object */ },
        Environment = new { /* nested object */ }
    };
}
```

### 3. File-Based Analysis Pattern (AnalyzeTrace)
```csharp
[McpServerTool, Description("Analyze an ETL trace file and return summary information")]
public static object AnalyzeTrace(string tracePath)
{
    if (!File.Exists(tracePath))
        throw new ArgumentException($"Trace file not found: {tracePath}");

    using var trace = TraceProcessor.Create(tracePath);
    // Process and analyze the trace with processIds
    return analysisResult;
}
```

### 4. Array Parameter Pattern (GetProcessCpuUsage)
```csharp
[McpServerTool, Description("Get detailed CPU usage sampling for specific processes")]
public static object GetProcessCpuUsage(string tracePath, uint[] processIds)
{
    if (processIds == null || processIds.Length == 0)
        throw new ArgumentException("At least one process ID must be provided");

    // Analyze specific processes by PID
    return detailedAnalysis;
}
```

## 🧪 Testing Your Setup

### Via Copilot Chat:
```
What MCP tools are available?
Use the HelloWorld tool to greet "Your Name"
Use GetSystemInfo to check my development environment
Use AnalyzeTrace to analyze an ETL file (if available)
Use GetProcessCpuUsage to analyze specific process IDs from the trace
```

### Expected Results:
- **HelloWorld**: Returns personalized greeting message
- **GetSystemInfo**: Returns detailed system information JSON
- **AnalyzeTrace**: Returns comprehensive ETL analysis with process data, timeline, and processIds
- **GetProcessCpuUsage**: Returns detailed CPU analysis for specific processes

## 📁 Project Structure

```
HelloWorldMCP/
├── Program.cs                  # Main MCP server with 4 essential tools
├── HelloWorldMCP.csproj        # Project configuration with TraceProcessor
├── README.md                   # This documentation
├── LAB_GUIDE.md               # Step-by-step lab exercises for VS Code Copilot
└── bin/Debug/net8.0/          # Compiled output
```

## 📚 Dependencies

- **.NET 8.0 SDK**
- **ModelContextProtocol** (0.3.0-preview.4) - MCP server framework
- **Microsoft.Extensions.Hosting** (9.0.0) - Hosting infrastructure
- **Microsoft.Windows.EventTracing** (2.0.711-preview) - ETL trace analysis

## 🎓 Educational Value

This lab teaches:
- **MCP protocol fundamentals** through hands-on implementation
- **String parameter handling** with validation patterns
- **Complex data structures** and JSON serialization
- **File-based analysis** using industry-standard TraceProcessor
- **Array parameter handling** for multi-process analysis
- **Two-phase analysis workflow** (discovery → targeted analysis)
- **VS Code integration** with modern development workflows
- **Error handling** best practices for MCP tools

Perfect for learning MCP development while working with real-world performance analysis scenarios! 🚀
