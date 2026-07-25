using System.Text;

namespace RoseNG.Core.Security;

/// <summary>
/// IMPORTANT - set expectations correctly: this is obfuscation, not
/// encryption. A string baked into a shipped binary can always be
/// recovered by a determined reverse engineer (a debugger or
/// disassembler will find it eventually) - there is no way around that
/// for a plain, unauthenticated support-server invite. What this class
/// *does* buy you:
///   - the literal invite URL does not appear in source control, in
///     `strings` output, or in a naive `grep -r discord.gg` sweep
///   - a bot/scraper crawling GitHub for "discord.gg/xxxx" text won't
///     find it
///   - you can rotate the invite by changing ONE plaintext line
///     (see Links.cs) and rebuilding - no need to re-derive any byte
///     arrays by hand
///
/// How it works: the plaintext lives only in Links.cs (git-ignored by
/// default, see README) as a single readable line. A build step (or you,
/// once) runs Obfuscate() on it and pastes the resulting byte array into
/// this file's ObfuscatedInvite field. At runtime Reveal() XORs it back.
/// </summary>
public static class ObfuscatedString
{
    // Rotating XOR key - change this too if you want a fresh encoding.
    private static readonly byte[] Key = { 0x9A, 0x3F, 0xE1, 0x77, 0x5C, 0x22, 0xB4, 0x0D };

    public static byte[] Obfuscate(string plaintext)
    {
        var bytes = Encoding.UTF8.GetBytes(plaintext);
        var output = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
            output[i] = (byte)(bytes[i] ^ Key[i % Key.Length]);
        return output;
    }

    public static string Reveal(byte[] obfuscated)
    {
        var output = new byte[obfuscated.Length];
        for (int i = 0; i < obfuscated.Length; i++)
            output[i] = (byte)(obfuscated[i] ^ Key[i % Key.Length]);
        return Encoding.UTF8.GetString(output);
    }
}
