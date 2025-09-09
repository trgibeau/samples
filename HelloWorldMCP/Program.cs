using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Windows.EventTracing;
using ModelContextProtocol.Server;

namespace HelloWorldMCP
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove all default providers
                    // Only add debug logging in debug mode
                    #if DEBUG
                    logging.AddDebug();
                    #endif
                })
                .ConfigureServices(services =>
                {
                    services.AddMcpServer()
                        .WithStdioServerTransport()
                        .WithToolsFromAssembly();
                });

            await builder.Build().RunAsync();
        }
    }

    /// <summary>
    /// MCP tools for ETL trace analysis and system information
    /// </summary>
    [McpServerToolType]
    public static class HelloWorldTools
    {
        #region Basic Tools

        [McpServerTool, Description("Greet someone by name and return a personalized message")]
        public static string HelloWorld(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            var greeting = $"Hello, {name}! Welcome to the MCP Hello World Lab.";
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] HelloWorld called for: {name}");
            
            return greeting;
        }

        [McpServerTool, Description("Get comprehensive system information as a structured JSON object")]
        public static object GetSystemInfo()
        {
            var systemInfo = new
            {
                Timestamp = DateTime.UtcNow,
                Machine = new
                {
                    Name = Environment.MachineName,
                    UserName = Environment.UserName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    Platform = Environment.OSVersion.Platform.ToString()
                },
                Runtime = new
                {
                    DotNetVersion = Environment.Version.ToString(),
                    Framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                    Architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
                    OSArchitecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString()
                },
                Environment = new
                {
                    CurrentDirectory = Environment.CurrentDirectory,
                    CommandLine = Environment.CommandLine,
                    Is64BitProcess = Environment.Is64BitProcess,
                    Is64BitOperatingSystem = Environment.Is64BitOperatingSystem
                }
            };

            return systemInfo;
        }

        #endregion

        #region ETL Analysis Tools

        [McpServerTool, Description("Analyze an ETL trace file and return summary information using TraceProcessor")]
        public static object AnalyzeTrace(string tracePath)
        {
            if (string.IsNullOrWhiteSpace(tracePath))
                throw new ArgumentException("Trace path cannot be empty", nameof(tracePath));

            if (!File.Exists(tracePath))
                throw new ArgumentException($"Trace file not found: {tracePath}");

            try
            {
                using var trace = TraceProcessor.Create(tracePath);
                
                // Configure the trace processor to load necessary data sources
                var processData = trace.UseProcesses();
                var systemMetadata = trace.UseMetadata();
                
                // Process the trace
                trace.Process();

                // Gather trace analysis results
                var processes = processData.Result.Processes;
                var startTime = systemMetadata.StartTime;
                var endTime = systemMetadata.StopTime;
                var duration = endTime - startTime;

                var result = new
                {
                    TraceFile = new
                    {
                        Path = tracePath,
                        SizeBytes = new FileInfo(tracePath).Length,
                        LastModified = File.GetLastWriteTime(tracePath)
                    },
                    Timeline = new
                    {
                        StartTime = startTime,
                        EndTime = endTime,
                        DurationMs = duration.TotalMilliseconds,
                        DurationFormatted = $"{duration.TotalSeconds:F2} seconds"
                    },
                    Processes = new
                    {
                        TotalCount = processes.Count,
                        UniqueProcessNames = processes.Select(p => p.ImageName).Distinct().Count(),
                        TopProcesses = processes
                            .Where(p => !string.IsNullOrEmpty(p.ImageName))
                            .GroupBy(p => p.ImageName)
                            .OrderByDescending(g => g.Count())
                            .Take(5)
                            .Select(g => new 
                            { 
                                ProcessName = g.Key, 
                                InstanceCount = g.Count(),
                                ProcessIds = g.Select(p => p.Id).OrderBy(id => id).ToList()
                            })
                            .ToList(),
                        AllProcesses = processes
                            .Where(p => !string.IsNullOrEmpty(p.ImageName))
                            .Select(p => new
                            {
                                ProcessId = p.Id,
                                ProcessName = p.ImageName,
                                CommandLine = p.CommandLine ?? "N/A"
                            })
                            .OrderBy(p => p.ProcessName)
                            .ThenBy(p => p.ProcessId)
                            .ToList()
                    }
                };

                return result;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to analyze trace file: {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Get detailed CPU usage sampling for specific processes by PIDs from an ETL trace file")]
        public static object GetProcessCpuUsage(string tracePath, uint[] processIds)
        {
            if (string.IsNullOrWhiteSpace(tracePath))
                throw new ArgumentException("Trace path cannot be empty", nameof(tracePath));

            if (processIds == null || processIds.Length == 0)
                throw new ArgumentException("Process IDs array cannot be null or empty", nameof(processIds));

            if (!File.Exists(tracePath))
                throw new ArgumentException($"Trace file not found: {tracePath}");

            try
            {
                using var trace = TraceProcessor.Create(tracePath);
                
                // Configure the trace processor to load necessary data sources
                var processData = trace.UseProcesses();
                var cpuSamplingData = trace.UseCpuSamplingData();
                var systemMetadata = trace.UseMetadata();
                
                // Process the trace
                trace.Process();

                // Find processes matching the specified IDs
                var matchingProcesses = processData.Result.Processes
                    .Where(p => processIds.Contains(p.Id))
                    .ToList();

                if (!matchingProcesses.Any())
                {
                    throw new ArgumentException($"No processes found with IDs: [{string.Join(", ", processIds)}]");
                }

                var missingIds = processIds.Except(matchingProcesses.Select(p => p.Id)).ToList();

                // Get all CPU samples once and group by process ID for efficiency
                var cpuSamplesByProcessId = cpuSamplingData.Result.Samples
                    .Where(s => s.Process != null && processIds.Contains(s.Process.Id))
                    .GroupBy(s => s.Process.Id)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var processCpuData = new List<object>();
                var totalSamples = 0;
                var totalCpuTime = TimeSpan.Zero;

                foreach (var process in matchingProcesses)
                {
                    // Get CPU samples for this process from our pre-grouped data
                    var cpuSamples = cpuSamplesByProcessId.GetValueOrDefault(process.Id, new List<Microsoft.Windows.EventTracing.Cpu.ICpuSample>());

                    // Use trace start/end times (process times require complex conversion)
                    var processStartTime = systemMetadata.StartTime;
                    var processEndTime = systemMetadata.StopTime;
                    var processLifetime = processEndTime - processStartTime;

                    var sampleCount = cpuSamples.Count;
                    totalSamples += sampleCount;

                    // Calculate more accurate CPU metrics
                    // CPU sampling frequency is typically 1ms (1000 Hz) on Windows
                    // Each sample represents the CPU was active during that sampling interval
                    var samplingIntervalMs = 1.0; // 1ms is standard Windows profiling interval
                    var estimatedCpuTimeMs = sampleCount * samplingIntervalMs;
                    var cpuUsagePercent = processLifetime.TotalMilliseconds > 0 ? 
                        (estimatedCpuTimeMs / processLifetime.TotalMilliseconds) * 100.0 : 0.0;

                    processCpuData.Add(new
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ImageName,
                        CommandLine = process.CommandLine ?? "N/A",
                        StartTime = processStartTime,
                        EndTime = processEndTime,
                        Lifetime = new
                        {
                            TotalMs = processLifetime.TotalMilliseconds,
                            Formatted = $"{processLifetime.TotalSeconds:F2} seconds"
                        },
                        CpuSampling = new
                        {
                            SampleCount = sampleCount,
                            EstimatedCpuTimeMs = estimatedCpuTimeMs,
                            CpuUsagePercent = Math.Round(cpuUsagePercent, 2),
                            SamplingIntervalMs = samplingIntervalMs,
                            SamplesPerSecond = processLifetime.TotalSeconds > 0 ? sampleCount / processLifetime.TotalSeconds : 0
                        }
                    });

                    totalCpuTime = totalCpuTime.Add(TimeSpan.FromMilliseconds(estimatedCpuTimeMs));
                }

                var traceStartTime = systemMetadata.StartTime;
                var traceEndTime = systemMetadata.StopTime;
                var traceDuration = traceEndTime - traceStartTime;

                var result = new
                {
                    TraceFile = new
                    {
                        Path = tracePath,
                        SizeBytes = new FileInfo(tracePath).Length,
                        LastModified = File.GetLastWriteTime(tracePath)
                    },
                    Query = new
                    {
                        RequestedProcessIds = processIds,
                        FoundProcesses = matchingProcesses.Count,
                        MissingProcessIds = processIds.Except(matchingProcesses.Select(p => p.Id)).ToArray(),
                        AnalyzedAt = DateTime.UtcNow
                    },
                    Timeline = new
                    {
                        TraceStartTime = traceStartTime,
                        TraceEndTime = traceEndTime,
                        TraceDurationMs = traceDuration.TotalMilliseconds,
                        TraceDurationFormatted = $"{traceDuration.TotalSeconds:F2} seconds"
                    },
                    CpuUsageSummary = new
                    {
                        TotalSamples = totalSamples,
                        EstimatedTotalCpuTimeMs = totalCpuTime.TotalMilliseconds,
                        AverageCpuUsagePercent = traceDuration.TotalMilliseconds > 0 ? 
                            (totalCpuTime.TotalMilliseconds / traceDuration.TotalMilliseconds) * 100 : 0
                    },
                    ProcessDetails = processCpuData
                };

                return result;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to analyze CPU usage for process IDs [{string.Join(", ", processIds)}]: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
