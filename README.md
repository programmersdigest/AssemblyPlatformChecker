# AssemblyPlatformChecker
Checks assemblies for platform compatibility (x86, x64, AnyCPU).

# Usage
AssemblyPlatformChecker.exe [-v] <searchPath>
  
| Parameter | Description |
| --------- | ----------- |
| searchPath | The path to search for portable executable (PE) files. If the path points to a file, this file is checked. If the path points to a directory, all executables contained in this directory and al its sub directories are checked. In case a relative path is supplied, it is considered relative to the AssemblyPlatformChecker executable. The path may be ommited, in which case the directory containing the AssemblyPlatformChecker executable is searched. |
| -v | Activates verbose output. The default output only shows executables which are compiled for 32 bit or 64 bit. With verbose output, all executables are dispayed with their respective target platform. |
| -h | Prints this help. |

# Example output (verbose)
```
Found 4 executables in total. Checking...
...\AssemblyPlatformChecker.exe is an AnyCPU assembly (ILOnly).
...\AssemblyPlatformCheckerPrefer32Bit.exe is an AnyCPU assembly preferring x86 (ILOnly, Preferred32Bit).
...\AssemblyPlatformCheckerx64.exe is a x64 assembly (ILOnly, PE32Plus).
...\AssemblyPlatformCheckerx86.exe is a x86 assembly (ILOnly, Required32Bit).
Done!
```

# Example output (non-verbose)
```
Found 4 executables in total. Checking...
...\AssemblyPlatformCheckerx64.exe is a x64 assembly.
...\AssemblyPlatformCheckerx86.exe is a x86 assembly.
Done!
```

# Relevant Materials
https://malvinly.com/2016/11/16/check-whether-a-net-dll-is-built-for-any-cpu-x86-or-x64/
