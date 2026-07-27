using System;
using System.Text;
using System.Text.Json;

namespace RoseNG.Core.Services
{
    public static class EncodingService
    {
        public static string Base64Encode(string input) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

        public static string Base64Decode(string input)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(input));
            }
            catch (Exception ex)
            {
                return $"Invalid base64: {ex.Message}";
            }
        }

        public static string HexEncode(string input) =>
            Convert.ToHexString(Encoding.UTF8.GetBytes(input)).ToLowerInvariant();

        public static string HexDecode(string input)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromHexString(input));
            }
            catch (Exception ex)
            {
                return $"Invalid hex: {ex.Message}";
            }
        }

        public static string UrlEncode(string input) => Uri.EscapeDataString(input);

        public static string UrlDecode(string input)
        {
            try { return Uri.UnescapeDataString(input); }
            catch (Exception ex) { return $"Invalid URL-encoded input: {ex.Message}"; }
        }

        public static string Rot13(string input)
        {
            var chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (c >= 'a' && c <= 'z') chars[i] = (char)('a' + (c - 'a' + 13) % 26);
                else if (c >= 'A' && c <= 'Z') chars[i] = (char)('A' + (c - 'A' + 13) % 26);
            }
            return new string(chars);
        }

        public static string XorCipher(string input, string key)
        {
            if (string.IsNullOrEmpty(key)) return "Key cannot be empty";
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var result = new byte[inputBytes.Length];
            for (int i = 0; i < inputBytes.Length; i++)
                result[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
            return Convert.ToHexString(result).ToLowerInvariant();
        }

        public static string XorDecipherHex(string hexInput, string key)
        {
            if (string.IsNullOrEmpty(key)) return "Key cannot be empty";
            try
            {
                var inputBytes = Convert.FromHexString(hexInput);
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var result = new byte[inputBytes.Length];
                for (int i = 0; i < inputBytes.Length; i++)
                    result[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
                return Encoding.UTF8.GetString(result);
            }
            catch (Exception ex)
            {
                return $"Invalid hex input: {ex.Message}";
            }
        }

        public static string DecodeJwt(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return "Invalid JWT (expected header.payload.signature)";

            string DecodeSegment(string segment)
            {
                segment = segment.Replace('-', '+').Replace('_', '/');
                switch (segment.Length % 4)
                {
                    case 2: segment += "=="; break;
                    case 3: segment += "="; break;
                }
                var bytes = Convert.FromBase64String(segment);
                var json = Encoding.UTF8.GetString(bytes);
                using var doc = JsonDocument.Parse(json);
                return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            }

            try
            {
                var header = DecodeSegment(parts[0]);
                var payload = DecodeSegment(parts[1]);
                return $"HEADER:\n{header}\n\nPAYLOAD:\n{payload}\n\n(signature not verified)";
            }
            catch (Exception ex)
            {
                return $"Failed to decode JWT: {ex.Message}";
            }
        }
    }
}
