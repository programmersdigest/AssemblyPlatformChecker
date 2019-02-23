using System;
using System.Linq;
using System.Reflection;

namespace AssemblyPlatformChecker
{
    /// <summary>
    /// Contains the entry point of the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point of the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            var commandArguments = new CommandArguments(args);

            if (commandArguments.ShowHelp)
            {
                PrintHelp();
            }
            else
            {
                CheckAssemblies(commandArguments);
            }
        }

        /// <summary>
        /// Prints the help to STD-out.
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine($"AssemblyPlatformChecker - {Assembly.GetExecutingAssembly().GetName().Version}");
            Console.WriteLine("Checks assemblies for platform compatibility (x86, x64, AnyCPU).");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("AssemblyPlatformChecker.exe [-v] [-f:filters] <searchPath>");
            Console.WriteLine("searchPath".PadRight(15) + "- The path to search for portable executable (PE) files. If the path points to a file, this file is checked. If the path points to a directory, all executables contained in this directory and al its sub directories are checked. In case a relative path is supplied, it is considered relative to the AssemblyPlatformChecker executable. The path may be ommited, in which case the directory containing the AssemblyPlatformChecker executable is searched.");
            Console.WriteLine("-v".PadRight(15) + "- Activates verbose output which prints more information on each assembly.");
            Console.WriteLine("-f:".PadRight(15) + "- Filters the output by platform. Platforms to be displayed can be provided as a comma separated list. Valid platforms are: Native32, Native64, Dotnet32, Dotnet64, AnyCPU, AnyCPU32Preferred, Unknown.");
            Console.WriteLine("-h".PadRight(15) + "- Prints this help.");
        }

        /// <summary>
        /// Finds and checks all executables in the given search path.
        /// </summary>
        /// <param name="commandArguments">The parsed command line arguments.</param>
        private static void CheckAssemblies(CommandArguments commandArguments)
        {
            var files = new ExecutableFileFinder().GetExecutables(commandArguments.SearchPath);
            Console.WriteLine($"Found {files.Count()} executables in total. Checking...");

            var peFileChecker = new PeFileChecker();
            foreach (var file in files)
            {
                var binaryType = peFileChecker.GetBinaryType(file);
                if (ShouldDisplayAssemblyInformation(binaryType, commandArguments.Filters))
                {
                    var message = PrepareAssemblyInformation(file, binaryType, commandArguments.Verbose);
                    Console.WriteLine(message);
                }
            }

            Console.WriteLine("Done!");
        }

        /// <summary>
        /// Checks whether to display the given binary type or not. If the binary type is contained
        /// in the filters flags, it should get displayed and <c>true</c> is returned.
        /// </summary>
        /// <param name="binaryType">The detected binary type of the checked assembly.</param>
        /// <param name="filters">The filters flags as read from the command line arguments.</param>
        /// <returns><c>True</c> if the information should be displayed. <c>False</c> otherwise.</returns>
        private static bool ShouldDisplayAssemblyInformation(BinaryType binaryType, BinaryTypeFilters filters)
        {
            if (filters == BinaryTypeFilters.All)
            {
                return true;
            }

            switch (binaryType)
            {
                case BinaryType.Unknown:
                    return (filters & BinaryTypeFilters.Unknown) == BinaryTypeFilters.Unknown;
                case BinaryType.Native32Bit:
                    return (filters & BinaryTypeFilters.Native32) == BinaryTypeFilters.Native32;
                case BinaryType.Native64Bit:
                    return (filters & BinaryTypeFilters.Native64) == BinaryTypeFilters.Native64;
                case BinaryType.Dotnet32Bit:
                    return (filters & BinaryTypeFilters.Dotnet32) == BinaryTypeFilters.Dotnet32;
                case BinaryType.Dotnet64Bit:
                    return (filters & BinaryTypeFilters.Dotnet64) == BinaryTypeFilters.Dotnet64;
                case BinaryType.DotnetAnyCpu:
                    return (filters & BinaryTypeFilters.AnyCPU) == BinaryTypeFilters.AnyCPU;
                case BinaryType.DotnetAnyCpuPrefer32Bit:
                    return (filters & BinaryTypeFilters.AnyCPU32Preferred) == BinaryTypeFilters.AnyCPU32Preferred;
            }

            return true;
        }

        /// <summary>
        /// Prepares the printable output string for a specific assembly.
        /// </summary>
        /// <param name="path">Path to the assembly.</param>
        /// <param name="binaryType">Detected binary type of the assembly.</param>
        /// <param name="verbose">If <c>true</c>, some additional information is printed for the assembly.</param>
        /// <returns>The prepared output string.</returns>
        private static string PrepareAssemblyInformation(string path, BinaryType binaryType, bool verbose)
        {
            string result = $"Assembly {path} ";

            switch (binaryType)
            {
                case BinaryType.Native32Bit:
                    result += "is a native 32 bit binary";
                    break;
                case BinaryType.Native64Bit:
                    result += "is a native 64 bit binary";
                    break;
                case BinaryType.Dotnet32Bit:
                    result += "is a 32 bit .NET assembly";
                    break;
                case BinaryType.Dotnet64Bit:
                    result += "is a 64 bit .NET assembly";
                    break;
                case BinaryType.DotnetAnyCpu:
                    result += "is a platform agnostic .NET assembly";
                    break;
                case BinaryType.DotnetAnyCpuPrefer32Bit:
                    result += "is a platform agnostic .NET assembly preferring 32 bit";
                    break;
                default:
                    result += "could not be analysed";
                    break;
            }

            if (verbose)
            {
                result += $" ({binaryType})";
            }

            return result + ".";
        }
    }
}
