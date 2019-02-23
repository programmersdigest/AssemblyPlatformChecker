using System;
using System.IO;

namespace AssemblyPlatformChecker
{
    /// <summary>
    /// Reads various meta data from PE files to determine their <see cref="BinaryType"/>.
    /// </summary>
    internal class PeFileChecker
    {
        /// <summary>
        /// Tries to determine the <see cref="BinaryType"/> for the given binary file.
        /// All exceptions are logged to STD-Error.
        /// </summary>
        /// <param name="path">Path to the binary file to check.</param>
        /// <returns>The resulting <see cref="BinaryType"/>.</returns>
        public BinaryType GetBinaryType(string path)
        {
            try
            {
                return GetBinaryTypeInternal(path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                return BinaryType.Unknown;
            }
        }

        /// <summary>
        /// Reads the various PE headers to detect the <see cref="BinaryType"/> from the
        /// COFF header field Machine, the Optional header field Magic and the COR header
        /// field Flags.
        /// </summary>
        /// <param name="path">Path to the binary file to check.</param>
        /// <returns>The resulting <see cref="BinaryType"/>.</returns>
        private BinaryType GetBinaryTypeInternal(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream))
            {
                // Read offset of COFF header.
                stream.Seek(0x3c, SeekOrigin.Begin);
                var coffHeaderOffset = reader.ReadUInt16();

                var machineType = ReadCoffMachineType(reader, coffHeaderOffset);

                var optionalHeaderOffset = (uint)coffHeaderOffset + 24;   // COFF header has a fixed size.
                var magic = ReadOptionalHeaderMagic(reader, optionalHeaderOffset);

                var nativeBinaryType = GetNativeBinaryType(machineType, magic);

                // Read "NumberOfRvaAndSizes" from windows specific fields in optional header.
                var numberOfRvaAndSizes = ReadNumberOfRvaAndSizes(reader, nativeBinaryType, optionalHeaderOffset);
                if (numberOfRvaAndSizes < 15)
                {
                    return nativeBinaryType;      // Not a CLR assembly.
                }

                // Read CLR directory entry.
                var clrDirectoryVirtualAddress = ReadClrDirectoryVirtualAddress(reader, nativeBinaryType, optionalHeaderOffset);
                if (clrDirectoryVirtualAddress == 0)
                {
                    return nativeBinaryType;      // Not a CLR assembly.
                }

                if (nativeBinaryType == BinaryType.Native64Bit)
                {
                    return BinaryType.Dotnet64Bit;  // We are a CLR assembly forced to 64 Bit.
                }

                var corHeaderOffset = ComputeCorHeaderOffset(reader, optionalHeaderOffset, numberOfRvaAndSizes, clrDirectoryVirtualAddress);
                return ReadCorFlags(reader, corHeaderOffset);
            }
        }

        /// <summary>
        /// Reads the machine type from the COFF header.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="coffHeaderOffset">Offset of the first byte of the COFF header.</param>
        /// <returns>The machine type.</returns>
        /// <exception cref="BadImageFormatException">In case the machine type is neither i386 nor AMD64.</exception>
        private ushort ReadCoffMachineType(BinaryReader reader, uint coffHeaderOffset)
        {
            reader.BaseStream.Seek(coffHeaderOffset + 4, SeekOrigin.Begin);
            var machineType = reader.ReadUInt16();
            if (machineType != 0x14c && machineType != 0x8664)  // i386 or AMD64
            {
                throw new BadImageFormatException($"Unknoen \"Machine\" in COFF header: {machineType:X}");
            }

            return machineType;
        }

        /// <summary>
        /// Reads the field magic from the optional header.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="optionalHeaderOffset">Offset of the first byte of the optional header.</param>
        /// <returns>The optional header magic.</returns>
        private ushort ReadOptionalHeaderMagic(BinaryReader reader, uint optionalHeaderOffset)
        {
            reader.BaseStream.Seek(optionalHeaderOffset, SeekOrigin.Begin);
            var magic = reader.ReadUInt16();
            if (magic != 0x10b && magic != 0x20b)   // PE32 or PE32+
            {
                throw new BadImageFormatException($"Unknown \"Magic\" in optional header: {magic:X}");
            }

            return magic;
        }

        /// <summary>
        /// Computes the native binary type of the PE file.
        /// </summary>
        /// <param name="machineType">The machine type as read from the COFF header.</param>
        /// <param name="magic">The magic as read from the optional header.</param>
        /// <returns>The native <see cref="BinaryType"/>; either Native32Bit or Native64Bit.</returns>
        /// <exception cref="BadImageFormatException">In case the machineType and magic state differing architectures.</exception>
        private BinaryType GetNativeBinaryType(ushort machineType, ushort magic)
        {
            if (machineType == 0x14c && magic == 0x10b)
            {
                return BinaryType.Native32Bit;
            }
            else if (machineType == 0x8664 && magic == 0x20b)
            {
                return BinaryType.Native64Bit;
            }
            else
            {
                throw new BadImageFormatException($"COFF header ({machineType:X}) and optional header ({magic:X}) state differing architectures.");
            }
        }

        /// <summary>
        /// Reads the number of directory entries from the optional header.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="nativeBinaryType">The native binary type of the PE file.</param>
        /// <param name="optionalHeaderOffset">Offset off the first byte of the optional header.</param>
        /// <returns>The number of directory entries.</returns>
        private uint ReadNumberOfRvaAndSizes(BinaryReader reader, BinaryType nativeBinaryType, uint optionalHeaderOffset)
        {
            if (nativeBinaryType == BinaryType.Native32Bit)
            {
                reader.BaseStream.Seek(optionalHeaderOffset + 92, SeekOrigin.Begin);
            }
            else
            {
                reader.BaseStream.Seek(optionalHeaderOffset + 108, SeekOrigin.Begin);
            }
            return reader.ReadUInt32();
        }

        /// <summary>
        /// Reads the relative virtual address of the COR header from the CLR directory in the optional header..
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="nativeBinaryType">The native binary type of the PE file.</param>
        /// <param name="optionalHeaderOffset">Offset off the first byte of the optional header.</param>
        /// <returns>The relative virtual address of the COR header.</returns>
        private uint ReadClrDirectoryVirtualAddress(BinaryReader reader, BinaryType nativeBinaryType, uint optionalHeaderOffset)
        {
            if (nativeBinaryType == BinaryType.Native32Bit)
            {
                reader.BaseStream.Seek(optionalHeaderOffset + 208, SeekOrigin.Begin);
            }
            else
            {
                reader.BaseStream.Seek(optionalHeaderOffset + 224, SeekOrigin.Begin);
            }
            return reader.ReadUInt32();
        }

        /// <summary>
        /// Finds the section in which the COR header is located and computes the COR header offset.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="optionalHeaderOffset">Offset of the first byte of the optional header.</param>
        /// <param name="numberOfRvaAndSizes">Number of directory data entries as read from the optional header.</param>
        /// <param name="clrDirectoryVirtualAddress">The virtual address read from the CLR directory entry in the optional header.</param>
        /// <returns>The offset to the first byte of the COR header.</returns>
        private uint ComputeCorHeaderOffset(BinaryReader reader, uint optionalHeaderOffset, uint numberOfRvaAndSizes, uint clrDirectoryVirtualAddress)
        {
            reader.BaseStream.Seek(optionalHeaderOffset + 96 + numberOfRvaAndSizes * 8, SeekOrigin.Begin);

            uint corHeaderOffset = 0;
            do
            {
                reader.BaseStream.Seek(8, SeekOrigin.Current);
                var virtualSize = reader.ReadUInt32();
                var virtualAddress = reader.ReadUInt32();
                reader.BaseStream.Seek(4, SeekOrigin.Current);
                var pointerToRawData = reader.ReadUInt32();

                if (clrDirectoryVirtualAddress >= virtualAddress &&
                    clrDirectoryVirtualAddress < virtualAddress + virtualSize)
                {
                    corHeaderOffset = pointerToRawData + (clrDirectoryVirtualAddress - virtualAddress);
                }
            } while (corHeaderOffset == 0);

            return corHeaderOffset;
        }

        /// <summary>
        /// Determines whether the CLR assembly is x86, x64, AnyCPU or AnyCPU preferring 32 Bit.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="corHeaderoffset">Offset to the first byte of the COR header.</param>
        /// <returns>The resulting <see cref="BinaryType"/>.</returns>
        private BinaryType ReadCorFlags(BinaryReader reader, uint corHeaderoffset)
        {
            reader.BaseStream.Seek(corHeaderoffset + 16, SeekOrigin.Begin);
            var corFlags = reader.ReadUInt32();

            if ((corFlags & 0x00020000) == 0x00020000)
            {
                return BinaryType.DotnetAnyCpuPrefer32Bit;
            }
            else if ((corFlags & 0x00000002) == 0x00000002)
            {
                return BinaryType.Dotnet32Bit;
            }
            else
            {
                return BinaryType.DotnetAnyCpu;
            }
        }
    }
}
