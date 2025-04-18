using Godot;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPckReader;

/// <summary>
/// One entry in the PCK file table.
/// </summary>
public sealed class PckFileEntry
{
    public string Path { get; }
    public ulong Offset { get; }
    public ulong Size { get; }
    public byte[] Md5 { get; } // 16 bytes
    public uint Flags { get; }

    internal PckFileEntry(string path, ulong offset, ulong size, byte[] md5, uint flags)
    {
        Path = path;
        Offset = offset;
        Size = size;
        Md5 = md5;
        Flags = flags;
    }
}

/// <summary>
/// Holds all parsed PCK metadata. If <paramref name="IsValid"/> is false, <paramref name="Status"/> returns why.
/// </summary>
public sealed class PckData
{
    public bool IsValid { get; }
    public ReadStatus Status { get; }
    public uint Format { get; }
    public uint EngineVersionMajor { get; }
    public uint EngineVersionMinor { get; }
    public uint EngineVersionPatch { get; }
    public bool Encrypted { get; }
    public bool RelativeFileBase { get; }
    public ulong FileBase { get; }

    public IReadOnlyList<string> Directories { get; }
    public IReadOnlyList<PckFileEntry> Files { get; }

    public enum ReadStatus
    {
        /// <summary>
        /// The pck file was successfully read.
        /// </summary>
        Success,
        /// <summary>
        /// The read file's header 'magic' was not correct.
        /// </summary>
        BadMagic,
        /// <summary>
        /// Unsupported pck format version.
        /// </summary>
        BadFormat,
        /// <summary>
        /// The pck is encrypted.
        /// </summary>
        Encrypted,
        /// <summary>
        /// An entry inside the pck was corrupt.
        /// </summary>
        CorruptEntry,
        /// <summary>
        /// The pck file was not found or able to be loaded.
        /// </summary>
        LoadFailed
    }

    internal PckData(ReadStatus readStatus)
    {
        IsValid = false;
        Status = readStatus;
        Directories = null;
        Files = null;
    }

    internal PckData(
        uint format,
        uint verMaj, uint verMin, uint verPat,
        bool encrypted,
        bool relativeFb,
        ulong fileBase,
        List<string> dirs,
        List<PckFileEntry> files
    )
    {
        IsValid = true;
        Status = ReadStatus.Success;
        Format = format;
        EngineVersionMajor = verMaj;
        EngineVersionMinor = verMin;
        EngineVersionPatch = verPat;
        Encrypted = encrypted;
        RelativeFileBase = relativeFb;
        FileBase = fileBase;
        Directories = dirs;
        Files = files;
    }
}

/// <summary>
/// Static entry point for reading a .pck and producing a <see cref="PckData"/>.
/// </summary>
public static class PckReader
{
    // From core/io/file_access_pack.cpp
    const int PACK_HEADER_MAGIC = 0x43504447; // "GDPC"
    const uint PACK_FORMAT_VERSION = 2;
    const uint PACK_DIR_ENCRYPTED = 1 << 0;
    const uint PACK_REL_FILEBASE = 1 << 1;

    /// <summary>
    /// Parses the pck at <paramref name="pckPath"/>.
    /// </summary>
    public static PckData Read(string pckPath)
    {
        try
        {
            var dirs = new HashSet<string>(StringComparer.Ordinal);
            var files = new List<PckFileEntry>();

            using var file = FileAccess.Open(pckPath, FileAccess.ModeFlags.Read);

            // --- Header ---
            if (file.Get32() != PACK_HEADER_MAGIC)
                return new(PckData.ReadStatus.BadMagic);

            uint format = file.Get32();
            if (format != PACK_FORMAT_VERSION)
                return new(PckData.ReadStatus.BadFormat);

            uint verMaj = file.Get32();
            uint verMin = file.Get32();
            uint verPat = file.Get32();

            uint packFlags = file.Get32();
            bool encDirs = (packFlags & PACK_DIR_ENCRYPTED) != 0;
            bool relFB = (packFlags & PACK_REL_FILEBASE) != 0;

            ulong fileBase = file.Get64();

            // skip reserved (16 × 4 bytes)
            for (int i = 0; i < 16; i++)
                file.Get32();

            uint fileCount = file.Get32();

            if (encDirs)
                return new(PckData.ReadStatus.Encrypted);

            // --- Entries ---
            for (uint i = 0; i < fileCount; i++)
            {
                uint nameLen = file.Get32();
                if (nameLen == 0)
                    return new(PckData.ReadStatus.CorruptEntry);

                var nameBytes = file.GetBuffer((int)nameLen);
                string path = Encoding.UTF8.GetString(nameBytes);

                // collect directory
                int slash = path.LastIndexOf('/');
                if (slash >= 0)
                    dirs.Add(path.Substring(0, slash));

                ulong offset = file.Get64();
                ulong size = file.Get64();
                byte[] md5 = file.GetBuffer(16);
                uint flags = file.Get32();

                files.Add(new PckFileEntry(path, offset, size, md5, flags));
            }

            return new PckData(
                format,
                verMaj, verMin, verPat,
                encDirs, relFB, fileBase,
                new(dirs),
                files
            );
        }
        catch
        {
            // any I/O errors or unexpected data
            return new(PckData.ReadStatus.LoadFailed);
        }
    }
}
