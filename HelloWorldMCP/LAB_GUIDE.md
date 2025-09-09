# Hello World MCP Lab Guide - VS Code Copilot Integration

This lab guide focuses on using VS Code's Copilot MCP integration to interact with four essential MCP tools. You'll learn string handling, JSON objects, ETL trace analysis, and targeted process analysis with TraceProcessor through VS Code's native MCP support.

## 🎯 Learning Goals

By the end of this lab, you will understand:
- How to configure VS Code MCP integration with Copilot
- Simple string parameter handling and validation via Copilot
- Complex JSON object construction and return through MCP tools
- ETL trace analysis using Microsoft.Windows.EventTracing via Copilot
- Targeted process analysis with CPU sampling through MCP
- Two-phase analysis workflow: discovery → deep-dive
- Error handling patterns for different scenarios
- Real-world MCP integration patterns in VS Code

## 📋 Prerequisites

- C# programming knowledge
- .NET 8.0 SDK installed
- **VS Code** with **GitHub Copilot** extension
- **MCP extension** for VS Code (install from marketplace)
- Basic understanding of ETL traces (helpful but not required)

## 🚀 Lab Setup

### Step 1: Configure VS Code MCP Integration

1. **Install the MCP extension** in VS Code
2. **Configure the MCP server** in your global MCP configuration:
   - Open VS Code Command Palette (Ctrl+Shift+P)
   - Search for "MCP: Edit Configuration"
   - Add the HelloWorldMCP server to your `mcp.json`:

```jsonc
{
  "servers": {
    "hello-world-lab": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "HelloWorldMCP.csproj"],
      "cwd": "/path/to/your/HelloWorldMCP"
    }
  },
  "inputs": []
}
```

**Note**: Replace `/path/to/your/HelloWorldMCP` with the actual path to your HelloWorldMCP project directory. Examples:
- Windows: `"C:\\Users\\YourUsername\\source\\repos\\HelloWorldMCP"`
- macOS/Linux: `"/home/username/projects/HelloWorldMCP"`

**Alternative**: If you have other MCP servers configured, add the `hello-world-lab` entry to your existing `servers` object.

### Step 2: Build and Start the MCP Server

1. **Build the project**: `dotnet build`
2. **Restart VS Code** to reload the MCP configuration
3. **Verify MCP server connection**: Open Copilot Chat and ask:
   ```
   What MCP tools are available?
   ```
4. **Check for HelloWorldMCP tools** - you should see: HelloWorld, GetSystemInfo, AnalyzeTrace, GetProcessCpuUsage

## 📚 Part 1: Using Copilot with MCP Tools

### Exercise 1.1: HelloWorld via Copilot - String Parameter Pattern

**Objective**: Use Copilot to interact with the HelloWorld MCP tool

1. **Open Copilot Chat** in VS Code (Ctrl+Shift+I)

2. **Ask Copilot to use the HelloWorld tool**:
   ```
   @workspace Use the HelloWorld MCP tool to greet "ETL Developer"
   ```

3. **Observe the interaction**:
   - Copilot should automatically discover and use the MCP tool
   - Notice how parameters are passed and responses received
   - See the personalized greeting response

4. **Test error handling** by asking Copilot:
   ```
   @workspace Use the HelloWorld tool with an empty name
   ```

**Key Learning Points**:
- Copilot automatically discovers MCP tools
- Parameter validation with `ArgumentException`
- Simple string return type handled seamlessly
- Error messages are surfaced through Copilot

### Exercise 1.2: GetSystemInfo via Copilot - JSON Object Pattern

**Objective**: Use Copilot to interact with complex JSON responses from MCP

1. **Ask Copilot for system information**:
   ```
   @workspace Use the GetSystemInfo MCP tool to get current system details
   ```

2. **Examine the structured response**:
   - Copilot will display the nested JSON object
   - Notice Machine, Runtime, and Environment sections
   - See how DateTime values are formatted

3. **Ask Copilot to analyze the data**:
   ```
   @workspace What can you tell me about the system from the GetSystemInfo response?
   ```

**Key Learning Points**:
- Anonymous object construction with nested properties
- No parameters required (empty arguments object)
- Structured data organization through MCP
- Copilot can interpret and analyze MCP tool responses

### Exercise 1.3: AnalyzeTrace via Copilot - File Analysis Pattern

**Objective**: Use Copilot to perform ETL trace analysis through MCP

1. **Test error handling first** (no real trace file):
   ```
   @workspace Use the AnalyzeTrace tool with the path "nonexistent.etl"
   ```

2. **Observe the error handling**:
   - Copilot receives and displays the error message
   - File existence validation is demonstrated
   - Error messages are user-friendly through Copilot

3. **If you have an ETL file, test with real data**:
   ```
   @workspace Use the AnalyzeTrace tool to analyze the ETL trace at "/path/to/your/trace.etl"
   ```

**Key Learning Points**:
- File existence validation through MCP
- Using `TraceProcessor.Create()` for ETL analysis
- Complex data aggregation results displayed via Copilot
- Exception handling with meaningful error messages

### Exercise 1.4: GetProcessCpuUsage via Copilot - Targeted Analysis Pattern

**Objective**: Use Copilot to perform detailed CPU analysis on specific processes from trace data

1. **Test with invalid parameters first**:
   ```
   @workspace Use the GetProcessCpuUsage tool with an empty process ID array
   ```

2. **If you have an ETL file, use AnalyzeTrace first to discover process IDs**:
   ```
   @workspace First use AnalyzeTrace to analyze "/path/to/your/trace.etl", then use GetProcessCpuUsage to analyze the top 3 process IDs from the results
   ```

3. **Test the two-phase workflow**:
   ```
   @workspace Show me how to use AnalyzeTrace and GetProcessCpuUsage together to investigate performance issues
   ```

**Key Learning Points**:
- Array parameter handling for multiple process IDs
- Two-phase analysis workflow: discovery → detailed analysis
- Process-specific CPU sampling and performance metrics
- Integration between multiple MCP tools for comprehensive analysis

## 🛠️ Part 2: Advanced Copilot + MCP Interactions

### Exercise 2.1: Copilot Reasoning with MCP Data

**Objective**: Learn how Copilot can reason about and analyze MCP tool results

1. **Get system info and ask for analysis**:
   ```
   @workspace Use GetSystemInfo and then explain what version of .NET this system is running
   ```

2. **Chain multiple interactions**:
   ```
   @workspace First use GetSystemInfo, then greet the current user with HelloWorld using the machine name
   ```

3. **Ask Copilot to compare results**:
   ```
   @workspace Use GetSystemInfo twice and tell me if anything changed between calls
   ```

**Key Learning Points**:
- Copilot can chain MCP tool calls
- Results can be analyzed and interpreted
- Data from MCP tools becomes part of Copilot's context

### Exercise 2.2: Creating ETL Traces for Analysis

**Objective**: Generate test data to use with the AnalyzeTrace endpoint via Copilot

1. **Ask Copilot for guidance on creating traces**:
   ```
   @workspace How can I create an ETL trace file on Windows to test the AnalyzeTrace tool?
   ```

2. **If you have Windows Performance Toolkit, create a trace**:
   - Copilot will guide you through the process
   - Use `wpr -start GeneralProfile` and `wpr -stop mytrace.etl`

3. **Test with Copilot**:
   ```
   @workspace Use AnalyzeTrace on my newly created trace file and summarize the findings
   ```

**Key Learning Points**:
- Copilot can provide guidance on using MCP tools effectively
- Real ETL data produces rich analysis results
- Copilot can summarize and interpret complex TraceProcessor output

## 🧪 Part 3: Error Handling and Debugging with Copilot

### Exercise 3.1: Comprehensive Error Testing via Copilot

**Objective**: Understand how Copilot handles MCP tool errors

1. **Test HelloWorld parameter validation**:
   ```
   @workspace Try using the HelloWorld tool with these problematic inputs: empty string, null, and whitespace-only name
   ```

2. **Test AnalyzeTrace file handling**:
   ```
   @workspace Test the AnalyzeTrace tool with: a non-existent file, a directory path instead of file, and an invalid file extension
   ```

3. **Observe Copilot's error interpretation**:
   - Notice how Copilot presents error messages
   - See how validation errors are handled gracefully
   - Understand the difference between parameter validation and runtime errors

**Key Learning Points**:
- MCP tools can throw exceptions that Copilot handles gracefully
- Parameter validation happens before tool execution
- Error messages are user-friendly when presented through Copilot
- Copilot can suggest corrections based on error messages

## 🚀 Part 4: VS Code MCP Integration Best Practices

### Exercise 4.1: VS Code MCP Panel Usage

**Objective**: Master the VS Code MCP panel and Copilot integration

1. **Access the MCP Panel**:
   - Open View → MCP (or Ctrl+Shift+P → "MCP: Show Panel")
   - Verify your "hello-world-lab" server is listed
   - Check that all 4 tools are available

2. **Direct tool usage from MCP panel**:
   - Click on each tool to see descriptions
   - Use the panel to invoke tools directly
   - Compare with Copilot chat integration

3. **Monitor MCP logs**:
   - Check MCP output panel for server communication
   - Observe JSON-RPC messages being exchanged
   - Debug any connection issues

**Key Learning Points**:
- VS Code provides multiple ways to interact with MCP tools
- The MCP panel offers direct tool access
- Copilot chat provides natural language interaction
- Logging helps debug MCP integration issues

### Exercise 4.2: Copilot + MCP Workflow Integration

**Objective**: Integrate MCP tools into your development workflow via Copilot

1. **System analysis workflow**:
   ```
   @workspace Before I start development, use GetSystemInfo to check my current environment setup
   ```

2. **Performance analysis workflow**:
   ```
   @workspace I'm investigating a performance issue. If I provide an ETL trace, can you use AnalyzeTrace to help identify potential bottlenecks?
   ```

3. **Automated greetings**:
   ```
   @workspace Create a personalized welcome message using HelloWorld with my username
   ```

**Key Learning Points**:
- MCP tools become part of your development toolkit
- Copilot makes MCP tool usage natural and conversational
- Complex analysis tasks become accessible through simple chat commands
- MCP extends Copilot's capabilities beyond code generation
## 🎯 Part 5: Advanced MCP Concepts

### Exercise 5.1: Understanding MCP Tool Discovery

**Objective**: Learn how Copilot discovers and uses MCP tools

1. **Ask Copilot about available tools**:
   ```
   @workspace What MCP tools are available in this workspace?
   ```

2. **Test tool discovery**:
   ```
   @workspace Can you list all the capabilities you have access to through MCP?
   ```

3. **Examine tool descriptions**:
   ```
   @workspace What does each of the MCP tools do? Explain their purposes.
   ```

**Key Learning Points**:
- Copilot automatically discovers MCP tools when the server is running
- Tool descriptions and parameters are introspected from the MCP server
- Copilot can explain tool capabilities based on MCP metadata

### Exercise 5.2: Extending the Lab (Optional Challenge)

**Objective**: Create a 4th endpoint following the established patterns

**Challenge**: Add a `ValidateTraceFile` endpoint that checks if a file is a valid ETL trace:

```csharp
[McpServerTool, Description("Validate if a file is a valid ETL trace")]
public static object ValidateTraceFile(string tracePath)
{
    // Your implementation here:
    // 1. Check file exists
    // 2. Check file extension
    // 3. Try to open with TraceProcessor (catch exceptions)
    // 4. Return validation result with details
}
```

**Test your implementation with Copilot**:
```
@workspace Use the new ValidateTraceFile tool to check if my file is a valid ETL trace
```

### Exercise 5.2: Enhanced Error Reporting

**Objective**: Improve error messages with more context

Modify the existing endpoints to include:
- More detailed error messages
- Suggested fixes for common issues
- Context about what operation was being performed

## 📝 Lab Report Questions

1. **Copilot Integration**: How does using Copilot with MCP tools differ from direct JSON-RPC communication?

2. **Pattern Recognition**: What are the key differences between the string return pattern (HelloWorld) and object return pattern (GetSystemInfo) when used through Copilot?

3. **Error Handling**: How does Copilot present MCP tool errors to users, and how does this improve the user experience?

4. **TraceProcessor Integration**: What advantages does using TraceProcessor through MCP + Copilot provide over direct command-line tools?

5. **Workflow Integration**: How can MCP tools be integrated into VS Code development workflows via Copilot?

6. **Tool Discovery**: How does Copilot automatically discover and understand MCP tool capabilities?

## 🎓 Completion Checklist

- [ ] Successfully build and run the MCP server
- [ ] Configure VS Code MCP integration with Copilot
- [ ] Test all 3 endpoints via Copilot chat
- [ ] Use the VS Code MCP panel
- [ ] Understand string vs object return patterns through Copilot
- [ ] Test comprehensive error handling via Copilot
- [ ] Analyze the TraceProcessor integration approach
- [ ] Create at least one sample ETL trace for testing (optional)
- [ ] Use MCP tools in VS Code development workflow with Copilot
- [ ] Implement one custom endpoint and test with Copilot (optional)

## 🏆 Conclusion

You've mastered MCP integration with VS Code Copilot across three essential patterns:
- ✅ **Simple string I/O** with robust parameter validation through natural language
- ✅ **Complex JSON objects** with structured nested data accessible via Copilot
- ✅ **File-based analysis** using TraceProcessor with conversational interaction
- ✅ **VS Code integration** making MCP tools part of your development workflow
- ✅ **Copilot enhancement** extending AI capabilities with custom tools

This foundation prepares you to build production MCP servers that enhance developer productivity through VS Code and Copilot integration! 🚀
- ✅ **Complex JSON objects** with structured nested data
- ✅ **File-based analysis** using real-world ETL processing
- ✅ **Professional error handling** with meaningful messages
- ✅ **VS Code integration** for development workflow enhancement

These patterns provide the foundation for building production-ready MCP servers that can handle real-world ETL analysis scenarios.

**Congratulations! You're ready to build sophisticated MCP tools for ETL analysis and beyond! 🎉**
