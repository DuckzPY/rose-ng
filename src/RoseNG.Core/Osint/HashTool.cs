using System.Security.Cryptography;
using System.Text;

namespace RoseNG.Core.Osint;

public enum HashAlgorithmKind { Md5, Sha1, Sha256, Sha512 }

public static class HashTool
{
    public static ToolResult HashText(string text, HashAlgorithmKind kind)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var digest = Compute(bytes, kind);
        return ToolResult.Ok(Convert.ToHexString(digest).ToLowerInvariant());
    }

    public static async Task<ToolResult> HashFileAsync(string path, HashAlgorithmKind kind, CancellationToken ct = default)
    {
        if (!File.Exists(path))
            return ToolResult.Fail("File not found.");

        try
        {
            using var stream = File.OpenRead(path);
            using var algo = Create(kind);
            var digest = await algo.ComputeHashAsync(stream, ct);
            return ToolResult.Ok(Convert.ToHexString(digest).ToLowerInvariant());
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Hashing failed: {ex.Message}");
        }
    }

    public static ToolResult Verify(string computedHex, string expectedHex)
    {
        var match = string.Equals(computedHex.Trim(), expectedHex.Trim(), StringComparison.OrdinalIgnoreCase);
        return match
            ? ToolResult.Ok("Match: the hashes are identical.")
            : ToolResult.Fail("Mismatch: the hashes do not match.");
    }

    private static byte[] Compute(byte[] data, HashAlgorithmKind kind)
    {
        using var algo = Create(kind);
        return algo.ComputeHash(data);
    }

    private static HashAlgorithm Create(HashAlgorithmKind kind) => kind switch
    {
        HashAlgorithmKind.Md5 => MD5.Create(),
        HashAlgorithmKind.Sha1 => SHA1.Create(),
        HashAlgorithmKind.Sha256 => SHA256.Create(),
        HashAlgorithmKind.Sha512 => SHA512.Create(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
