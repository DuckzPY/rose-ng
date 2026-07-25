using System.Text;

namespace RoseNG.Core.Osint;

/// <summary>
/// V1 scope: filesystem-level metadata for any file, plus raw JPEG/PNG
/// header parsing for dimensions. Full EXIF GPS/camera extraction is
/// listed on the roadmap once an imaging library is pulled in.
/// </summary>
public static class MetadataTool
{
    public static ToolResult Inspect(string path)
    {
        if (!File.Exists(path))
            return ToolResult.Fail("File not found.");

        var info = new FileInfo(path);
        var sb = new StringBuilder();
        sb.AppendLine($"Name:      {info.Name}");
        sb.AppendLine($"Size:      {info.Length:N0} bytes");
        sb.AppendLine($"Created:   {info.CreationTimeUtc:u}");
        sb.AppendLine($"Modified:  {info.LastWriteTimeUtc:u}");
        sb.AppendLine($"Extension: {info.Extension}");
        sb.AppendLine($"Read-only: {info.IsReadOnly}");

        if (info.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            var dims = TryReadPngDimensions(path);
            if (dims is not null) sb.AppendLine($"Dimensions: {dims.Value.w} x {dims.Value.h}");
        }
        else if (info.Extension is ".jpg" or ".jpeg")
        {
            var dims = TryReadJpegDimensions(path);
            if (dims is not null) sb.AppendLine($"Dimensions: {dims.Value.w} x {dims.Value.h}");
        }

        return ToolResult.Ok(sb.ToString().TrimEnd());
    }

    private static (int w, int h)? TryReadPngDimensions(string path)
    {
        using var fs = File.OpenRead(path);
        Span<byte> header = stackalloc byte[24];
        if (fs.Read(header) < 24) return null;
        // Width/height live at bytes 16-23, big-endian.
        int w = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
        int h = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
        return (w, h);
    }

    private static (int w, int h)? TryReadJpegDimensions(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        if (br.ReadUInt16() != 0xD8FF) return null; // big-endian 0xFFD8 marker

        while (fs.Position < fs.Length - 8)
        {
            byte marker = br.ReadByte();
            if (marker != 0xFF) continue;
            byte type = br.ReadByte();
            if (type is 0xC0 or 0xC2) // SOF0 / SOF2
            {
                br.ReadUInt16(); // segment length
                br.ReadByte();   // precision
                int h = ReadBigEndianUInt16(br);
                int w = ReadBigEndianUInt16(br);
                return (w, h);
            }
            if (type == 0xD9 || type == 0x01) continue;
            int len = ReadBigEndianUInt16(br);
            if (len < 2) break;
            fs.Seek(len - 2, SeekOrigin.Current);
        }
        return null;
    }

    private static int ReadBigEndianUInt16(BinaryReader br)
    {
        var b1 = br.ReadByte();
        var b2 = br.ReadByte();
        return (b1 << 8) | b2;
    }
}
