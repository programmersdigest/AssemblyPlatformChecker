# AssemblyPlatformChecker
Checks assemblies for platform compatibility (x86, x64, AnyCPU).

# Usage
AssemblyPlatformChecker.exe [-v] <searchPath>
  
| Parameter | Description |
| --------- | ----------- |
| searchPath | The path to search for portable executable (PE) files. If the path points to a file, this file is checked. If the path points to a directory, all executables contained in this directory and al its sub directories are checked. In case a relative path is supplied, it is considered relative to the AssemblyPlatformChecker executable. The path may be ommited, in which case the directory containing the AssemblyPlatformChecker executable is searched. |
| -v | Activates verbose output which prints more information on each assembly. |
| -h | Prints this help. |

# Example output
```
Found 4 executables in total. Checking...
Assembly [...]\AssemblyPlatformChecker.exe is a platform agnostic .NET assembly
Assembly [...]\AssemblyPlatformCheckerPrefer32Bit.exe is a platform agnostic .NET assembly preferring 32 bit
Assembly [...]\AssemblyPlatformCheckerx64.exe is a 64 bit .NET assembly
Assembly [...]\AssemblyPlatformCheckerx86.exe is a 32 bit .NET assembly
Done!
```

# Relevant Materials
Format of PE files: https://docs.microsoft.com/en-us/windows/desktop/Debug/pe-format
Building a disasembler: https://codingwithspike.wordpress.com/2012/08/12/building-a-net-disassembler-part-3-parsing-the-text-section/
System.Reflection.PortableExecutable: https://github.com/dotnet/corefx/tree/master/src/System.Reflection.Metadata/src/System/Reflection/PortableExecutable

# Other ways to get the platform of a .NET assembly 
How to read the PE kind of an assembly: https://malvinly.com/2016/11/16/check-whether-a-net-dll-is-built-for-any-cpu-x86-or-x64/