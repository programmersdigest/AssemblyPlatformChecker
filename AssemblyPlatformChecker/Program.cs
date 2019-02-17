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
            Console.WriteLine("AssemblyPlatformChecker.exe [-v] <searchPath>");
            Console.WriteLine("searchPath".PadRight(15) + "- The path to search for portable executable (PE) files. If the path points to a file, this file is checked. If the path points to a directory, all executables contained in this directory and al its sub directories are checked. In case a relative path is supplied, it is considered relative to the AssemblyPlatformChecker executable. The path may be ommited, in which case the directory containing the AssemblyPlatformChecker executable is searched.");
            Console.WriteLine("-v".PadRight(15) + "- Activates verbose output. The default output only shows executables which are compiled for 32 bit or 64 bit. With verbose output, all executables are dispayed with their respective target platform.");
            Console.WriteLine("-h".PadRight(15) + "- Prints this help.");
        }

        /// <summary>
        /// Finds and checks all executables in the given search path.
        /// </summary>
        /// <param name="commandArguments"></param>
        private static void CheckAssemblies(CommandArguments commandArguments)
        {
            var files = new ExecutableFileFinder().GetExecutables(commandArguments.SearchPath);
            Console.WriteLine($"Found {files.Count()} executables in total. Checking...");
            foreach (var file in files)
            {
                new AssemblyChecker(file, commandArguments.Verbose).CheckAssembly();
            }
            Console.WriteLine("Done!");
        }
    }
}
