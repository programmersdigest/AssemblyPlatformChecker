namespace AssemblyPlatformChecker
{
    /// <summary>
    /// Possible binary types for an assembly.
    /// </summary>
    internal enum BinaryType
    {
        Unknown,
        Native32Bit,
        Native64Bit,
        Dotnet32Bit,
        Dotnet64Bit,
        DotnetAnyCpu,
        DotnetAnyCpuPrefer32Bit
    }
}
