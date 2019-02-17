using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssemblyPlatformChecker
{
    /// <summary>
    /// Provides methods to get all executables in a given path.
    /// </summary>
    internal class ExecutableFileFinder
    {
        /// <summary>
        /// Valid file extensions for executable files.
        /// </summary>
        private static readonly string[] _executableExtensions = new[] { ".exe", ".dll" };

        /// <summary>
        /// Finds all executable (.exe, .dll) files in the given <paramref name="searchPath"/>.
        /// If the path is an executable, only this file is returned.
        /// If the path is a directory all contained executables (including all sub directories)
        /// are returned.
        /// </summary>
        /// <param name="searchPath">The path at which to search for executables.</param>
        /// <returns>A list of absolute paths.</returns>
        public IEnumerable<string> GetExecutables(string searchPath)
        {
            if (!Path.IsPathRooted(searchPath))
            {
                var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                searchPath = Path.Combine(exeDir, searchPath ?? "");
            }

            var files = new List<string>();
            if (File.Exists(searchPath))
            {
                files.Add(searchPath);
            }
            else if (Directory.Exists(searchPath))
            {
                var dir = new DirectoryInfo(searchPath);
                files.AddRange(GetFilesRecursive(dir));
            }
            return files.Where(f => _executableExtensions.Contains(Path.GetExtension(f).ToLower()))
                        .ToList();
        }

        /// <summary>
        /// Recursivly retrieves all files in the given directory and all its sub directories.
        /// Access violations and other errors are logged to the console error stream but
        /// do not stop execution as would be the case when using <see cref="Directory.EnumerateFiles()"/>.
        /// </summary>
        /// <param name="dir">The directory to search.</param>
        /// <returns>A list of absolute paths.</returns>
        private IEnumerable<string> GetFilesRecursive(DirectoryInfo dir)
        {
            var files = new List<string>();

            try
            {
                foreach (var subDir in dir.EnumerateDirectories())
                {
                    files.AddRange(GetFilesRecursive(subDir));
                }

                files.AddRange(dir.EnumerateFiles().Select(f => f.FullName));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return files;
        }
    }
}
