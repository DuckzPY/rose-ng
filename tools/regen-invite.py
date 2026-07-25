#!/usr/bin/env python3
"""
Regenerates the obfuscated byte array used by RoseNG.Core.Security.Links.

Usage:
    python3 tools/regen-invite.py "https://discord.gg/yourNewInvite"

Paste the printed array into Links.cs, replacing DiscordInviteObfuscated.
Must match the Key array in ObfuscatedString.cs exactly, or update both together.
"""
import sys

KEY = [0x9A, 0x3F, 0xE1, 0x77, 0x5C, 0x22, 0xB4, 0x0D]

def obfuscate(plaintext: str) -> list[int]:
    data = plaintext.encode("utf-8")
    return [b ^ KEY[i % len(KEY)] for i, b in enumerate(data)]

def main() -> None:
    if len(sys.argv) != 2:
        print("Usage: python3 regen-invite.py <new-invite-url>")
        sys.exit(1)

    encoded = obfuscate(sys.argv[1])
    formatted = ", ".join(f"0x{b:02X}" for b in encoded)
    print("\nPaste this into Links.cs as DiscordInviteObfuscated:\n")
    print(f"    {{ {formatted} }}\n")

if __name__ == "__main__":
    main()
