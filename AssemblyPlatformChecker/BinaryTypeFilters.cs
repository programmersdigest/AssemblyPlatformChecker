using System;

namespace AssemblyPlatformChecker
{
    [Flags]
    internal enum BinaryTypeFilters
    {
        All = 0x00000000,
        Native32 = 0x00000001,
        Native64 = 0x00000002,
        Dotnet32 = 0x00000004,
        Dotnet64 = 0x00000008,
        AnyCPU = 0x00000010,
        AnyCPU32Preferred = 0x00000020,
        Unknown = 0x00000040
    }
}
