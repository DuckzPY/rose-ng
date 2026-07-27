using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RoseNG.Core.Services
{
    public static class SecurityService
    {
        public static string GeneratePassword(int length, bool symbols, bool digits, bool upper)
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digitChars = "0123456789";
            const string symbolChars = "!@#$%^&*()-_=+[]{}";

            var pool = lower
                + (upper ? upperChars : "")
                + (digits ? digitChars : "")
                + (symbols ? symbolChars : "");

            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(pool[b % pool.Length]);
            return sb.ToString();
        }

        public static string CheckStrength(string password)
        {
            int score = 0;
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(c => !char.IsLetterOrDigit(c))) score++;

            var verdict = score switch
            {
                <= 1 => "Very weak",
                2 => "Weak",
                3 => "Moderate",
                4 => "Strong",
                _ => "Very strong"
            };
            return $"{verdict} ({score}/5)";
        }

        public static string HashString(string input, string algorithm)
        {
            byte[] hash = algorithm.ToUpperInvariant() switch
            {
                "MD5" => MD5.HashData(Encoding.UTF8.GetBytes(input)),
                "SHA1" => SHA1.HashData(Encoding.UTF8.GetBytes(input)),
                "SHA256" => SHA256.HashData(Encoding.UTF8.GetBytes(input)),
                "SHA512" => SHA512.HashData(Encoding.UTF8.GetBytes(input)),
                _ => throw new ArgumentException("Unknown algorithm")
            };
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public static string IdentifyHash(string hash)
        {
            var len = hash.Trim().Length;
            return len switch
            {
                32 => "Likely MD5 (128-bit)",
                40 => "Likely SHA-1 (160-bit)",
                64 => "Likely SHA-256 (256-bit)",
                128 => "Likely SHA-512 (512-bit)",
                _ => $"Unrecognized length ({len} chars) — not a common hex hash"
            };
        }

        // Dictionary/wordlist based cracking against a hash. Loads a user-supplied
        // wordlist file line by line and compares hashes.
        public static string? CrackHash(string targetHash, string algorithm, string wordlistPath)
        {
            if (!File.Exists(wordlistPath)) return null;

            foreach (var word in File.ReadLines(wordlistPath))
            {
                var candidate = HashString(word, algorithm);
                if (string.Equals(candidate, targetHash.Trim(), StringComparison.OrdinalIgnoreCase))
                    return word;
            }
            return null;
        }

        public static string FileChecksum(string path, string algorithm = "SHA256")
        {
            using var stream = File.OpenRead(path);
            byte[] hash = algorithm.ToUpperInvariant() switch
            {
                "MD5" => MD5.HashData(stream),
                "SHA1" => SHA1.HashData(stream),
                "SHA256" => SHA256.HashData(stream),
                "SHA512" => SHA512.HashData(stream),
                _ => throw new ArgumentException("Unknown algorithm")
            };
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
