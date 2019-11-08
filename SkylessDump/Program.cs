using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SkylessDump
{
    class Program
    {
        private const string AssemblyName = "Assembly-CSharp.dll";

        const string USAGE =
            "==============================================\n" +
            "   Sunless Skies data dump tool by @akintos   \n" +
            "==============================================\n" +
            "\n" +
            "This tool requires entire managed game binaries (in Sunless Skies_Data\\Managed folder) to work.\n" +
            "\n" +
            "Usage:\n" +
            "\n" +
            "SkylessDump.exe extract <dllPath> <assetPath> <jsonOutputPath>\n" +
            "   Export game data from resources.assets file into JSON format.\n" +
            "   \n" +
            "   arguments:\n" +
            "       dllPath         : Path of game dll files.\n" +
            "       assetPath       : resources.assets file path.\n" +
            "       jsonOutputPath  : Dumped JSON output directory path.\n" +
            "\n" +
            "SkylessDump.exe import <dllPath> <assetInputPath> <jsonInputPath> <assetOutputPath>\n" +
            "   Import JSON files into resources.assets file.\n" +
            "   \n" +
            "   arguments:\n" +
            "       dllPath         : Path of game dll files.\n" +
            "       assetInputPath  : Original resources.assets file input path.\n" +
            "       jsonInputPath   : JSON input directory path.\n" +
            "       assetOutputPath : Patched resources.assets file output path.\n" +
            "\n" +
            "Examples:\n" +
            "   SkylessDump.exe extract .\\Managed\\ .\\Input\\resources.assets .\\dump\n" +
            "   SkylessDump.exe import .\\Managed\\ .\\Input\\resources.assets .\\dump .\\Output\\resources.assets ";

        const string dataPath = @"C:\Games\Sunless Skies\Sunless Skies_Data";
        
        static void PrintUsageAndExit(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine();
            PrintUsageAndExit();
        }

        static void PrintUsageAndExit()
        {
            Console.WriteLine(USAGE);
            Environment.Exit(0);
        }

        static void CheckFileExists(string path, string name)
        {
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"Failed to open {name}.\n{path}");
                Environment.Exit(1);
            }
        }

        static string[] FixArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Regex.Replace(args[i], @"(\\+)$", @"$1$1").TrimEnd('"');
            }

            return args;
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
                PrintUsageAndExit();

            string command = args[0].ToLower();

            string dllPath = args[1];
            Console.WriteLine(dllPath);
            string asmPath = null;

            if (File.Exists(dllPath))
            {
                asmPath = dllPath;
            }
            else
            {
                var tempPath = Path.Combine(dllPath, AssemblyName);
                if (File.Exists(tempPath))
                {
                    asmPath = tempPath;
                }
                else
                {
                    PrintUsageAndExit($"Cannot open Assembly-CSharp.dll, check arguments.");
                }
            }

            SkylessSerializer serializer;

            try
            {
                serializer = new SkylessSerializer(asmPath);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to load SkylessSerializer.\n{e.ToString()}");
                return;
            }

            SkylessDumpTool tool = new SkylessDumpTool(serializer);
            tool.LogCallback += Console.WriteLine;

            if (command == "export")
            {
                if (args.Length < 4)
                    PrintUsageAndExit($"Insufficient arguments : expected 4, got {args.Length}");
                CheckFileExists(args[2], "assetPath");

                tool.ExportAllToJson(args[2], args[3]);
                Console.WriteLine($"Exported JSON files to {args[3]}");
            }
            else if (command == "import")
            {
                if (args.Length < 5)
                    PrintUsageAndExit($"Insufficient arguments : expected 5, got {args.Length}");
                CheckFileExists(args[2], "assetPath");

                tool.ImportAllJsonToAsset(args[2], args[3], args[4]);
                Console.WriteLine($"Imported all JSON files to {args[4]}");
            }
            else
            {
                PrintUsageAndExit($"Error : Unknown command {command}");
            }
        }
    }
}
