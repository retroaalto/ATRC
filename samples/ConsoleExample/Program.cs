using Atrc.Core;
using Atrc.Core.Models;
using Atrc.Core.StandardLibrary;

namespace ConsoleExample;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("ATRC Console Example");
        Console.WriteLine("===================");
        
        try
        {
            // Load configuration file
            var configFile = "sample.atrc";
            var atrcData = await AtrcFileData.LoadAsync(configFile, ReadMode.ReadOnly);
            
            Console.WriteLine($"Loaded configuration from: {configFile}");
            Console.WriteLine();
            
            // Display variables
            Console.WriteLine("Variables:");
            foreach (var variable in atrcData.Variables)
            {
                Console.WriteLine($"  {variable.Name} = {variable.Value}");
            }
            Console.WriteLine();
            
            // Display blocks and keys
            Console.WriteLine("Configuration Blocks:");
            foreach (var block in atrcData.Blocks)
            {
                Console.WriteLine($"  [{block.Name}]");
                foreach (var key in block.Keys)
                {
                    Console.WriteLine($"    {key.Name} = {key.Value}");
                }
                Console.WriteLine();
            }
            
            // Demonstrate type conversions
            Console.WriteLine("Type Conversion Examples:");
            
            // Get debug mode as boolean
            var debugMode = atrcData["Application.Debug"];
            var isDebug = debugMode?.ToBool() ?? false;
            Console.WriteLine($"  Debug Mode (bool): {isDebug}");
            
            // Get max connections as integer
            var maxConnStr = atrcData["Database.MaxConnections"];
            var maxConn = maxConnStr?.ToInt() ?? 0;
            Console.WriteLine($"  Max Connections (int): {maxConn}");
            
            // Get timeout as double
            var timeoutStr = atrcData["Database.TimeoutSeconds"];
            var timeout = timeoutStr?.ToDouble() ?? 0.0;
            Console.WriteLine($"  Timeout (double): {timeout}");
            
            // Parse array of formats
            var formatsStr = atrcData["SupportedFormats.Formats"];
            var formats = formatsStr?.ToStringArray() ?? new string[0];
            Console.WriteLine($"  Supported Formats: [{string.Join(", ", formats)}]");
            
            Console.WriteLine();
            Console.WriteLine("Configuration loaded successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}