using System;
using System.Reflection;

namespace AssemblyPlatformChecker
{
    /// <summary>
    /// Provides methods to check an assembly for platform compatibility.
    /// To prevent assembly collisions, each assembly has to be loaded in a seperate
    /// AppDomain which is unloaded once the assembly has been checked.
    /// </summary>
    [Serializable]
    internal class AssemblyChecker
    {
        /// <summary>
        /// Path to the assembly to load and check.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Denotes whether to print verbose output or not.
        /// </summary>
        private readonly bool _verbose;

        /// <summary>
        /// Creates a new instance of the <see cref="AssemblyChecker"/> class.
        /// </summary>
        /// <param name="path">Path to the assembly to load and check.</param>
        /// <param name="verbose">Denotes whether to print verbose output or not.</param>
        public AssemblyChecker(string path, bool verbose)
        {
            _path = path;
            _verbose = verbose;
        }

        /// <summary>
        /// Checks whether the given assembly is built as x86, x64 or AnyCPU and outputs the result
        /// to the command line (STD-out).
        /// </summary>
        /// <seealso cref="For more info: https://malvinly.com/2016/11/16/check-whether-a-net-dll-is-built-for-any-cpu-x86-or-x64/"/>
        public void CheckAssembly()
        {
            var appDomain = AppDomain.CreateDomain("TempAssemblyCheckAppDomain");
            appDomain.DoCallBack(AssemblyLoadCallback);
            AppDomain.Unload(appDomain);
        }

        /// <summary>
        /// Callback to be executed in the temporarly created AppDomain.
        /// Performs a reflection only load of the assembly at <see cref="_path"/> and analyses
        /// the PEKind.
        /// </summary>
        private void AssemblyLoadCallback()
        {
            try
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(_path);
                assembly.ManifestModule.GetPEKind(out var peKind, out var machine);

                if (_verbose)
                {
                    PrintAssemblyInfoVerbose(peKind);
                }
                else
                {
                    PrintAssemblyInfo(peKind);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Prints platform information to STD-out based on the given <paramref name="peKind"/>.
        /// Only assemblies direcly compiled for x86 or x64 are printed.
        /// </summary>
        /// <param name="peKind">The <see cref="PortableExecutableKinds"/> of the loaded assembly.</param>
        private void PrintAssemblyInfo(PortableExecutableKinds peKind)
        {
            var message = "";

            if ((peKind & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly)
            {
                if ((peKind & PortableExecutableKinds.Required32Bit) == PortableExecutableKinds.Required32Bit)
                {
                    message = "a x86 assembly";
                }
                else if ((peKind & PortableExecutableKinds.PE32Plus) == PortableExecutableKinds.PE32Plus)
                {
                    message = "a x64 assembly";
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine($"{_path} is {message}.");
            }
        }

        /// <summary>
        /// Prints platform information to STD-out based on the given <paramref name="peKind"/>.
        /// </summary>
        /// <param name="peKind">The <see cref="PortableExecutableKinds"/> of the loaded assembly.</param>
        private void PrintAssemblyInfoVerbose(PortableExecutableKinds peKind)
        {
            string message;
            if ((peKind & PortableExecutableKinds.Unmanaged32Bit) == PortableExecutableKinds.Unmanaged32Bit)
            {
                message = "an unmanaged executable";
            }
            else if ((peKind & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly)
            {
                if ((peKind & PortableExecutableKinds.Required32Bit) == PortableExecutableKinds.Required32Bit)
                {
                    message = "a x86 assembly";
                }
                else if ((peKind & PortableExecutableKinds.PE32Plus) == PortableExecutableKinds.PE32Plus)
                {
                    message = "a x64 assembly";
                }
                else if ((peKind & PortableExecutableKinds.Preferred32Bit) == PortableExecutableKinds.Preferred32Bit)
                {
                    message = "an AnyCPU assembly preferring x86";
                }
                else
                {
                    message = "an AnyCPU assembly";
                }
            }
            else
            {
                message = "not a portable executable";
            }

            Console.WriteLine($"{_path} is {message} ({peKind}).");
        }
    }
}
