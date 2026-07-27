using System;
using System.Collections.Generic;
using System.IO;

namespace RoseNG.Core.Services
{
    // Bundled IEEE OUI (Organizationally Unique Identifier) vendor database.
    //
    // The full IEEE registry (https://standards-oui.ieee.org/oui/oui.csv) has 40,000+
    // entries and changes over time, so rather than freezing a huge snapshot into the
    // binary, RoseNg ships a curated subset covering common consumer/enterprise vendors
    // (networking gear, major device manufacturers, virtualization platforms). This
    // covers the majority of MACs seen on a typical home/office network out of the box.
    //
    // For full coverage, download the official CSV and drop it at OuiDatabase.ExtraDatabasePath
    // (one "AABBCC,Vendor Name" pair per line - the official oui.csv format works as-is).
    public static class OuiDatabase
    {
        private static readonly Dictionary<string, string> Vendors = BuildBaseDatabase();
        private static bool _extraLoaded;
        private static readonly object Lock = new();

        public static string ExtraDatabasePath => Path.Combine(SettingsService.AppDataDir, "oui-extra.csv");

        public static string? Lookup(string ouiHex)
        {
            EnsureExtraLoaded();
            return Vendors.TryGetValue(ouiHex.ToUpperInvariant(), out var vendor) ? vendor : null;
        }

        public static int EntryCount
        {
            get { EnsureExtraLoaded(); return Vendors.Count; }
        }

        private static void EnsureExtraLoaded()
        {
            if (_extraLoaded) return;
            lock (Lock)
            {
                if (_extraLoaded) return;
                _extraLoaded = true;
                try
                {
                    var path = ExtraDatabasePath;
                    if (!File.Exists(path)) return;

                    foreach (var rawLine in File.ReadAllLines(path))
                    {
                        var line = rawLine.Trim();
                        if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("Registry")) continue;

                        // Accept both the official IEEE CSV columns (Registry,Assignment,Organization Name,...)
                        // and a simple "AABBCC,Vendor" format.
                        var parts = line.Split(',');
                        if (parts.Length < 2) continue;

                        string ouiField = parts[0].Trim('"', ' ');
                        string vendorField = parts[1].Trim('"', ' ');

                        // Official CSV: Registry column is first ("MA-L"), Assignment (the OUI) is second.
                        if (ouiField.Equals("MA-L", StringComparison.OrdinalIgnoreCase) ||
                            ouiField.Equals("MA-M", StringComparison.OrdinalIgnoreCase) ||
                            ouiField.Equals("MA-S", StringComparison.OrdinalIgnoreCase))
                        {
                            if (parts.Length < 3) continue;
                            ouiField = vendorField;
                            vendorField = parts[2].Trim('"', ' ');
                        }

                        var key = ouiField.Replace(":", "").Replace("-", "").ToUpperInvariant();
                        if (key.Length < 6 || vendorField.Length == 0) continue;

                        Vendors[key.Substring(0, 6)] = vendorField;
                    }
                }
                catch
                {
                    // Malformed/unreadable extension file shouldn't crash the app
                }
            }
        }

        private static Dictionary<string, string> BuildBaseDatabase()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Cisco
                ["00000C"] = "Cisco Systems",
                ["FCFBFB"] = "Cisco Systems",
                ["00179A"] = "Cisco Systems",
                ["0007EB"] = "Cisco Systems",

                // Apple
                ["001B63"] = "Apple, Inc.",
                ["3C0754"] = "Apple, Inc.",
                ["F01898"] = "Apple, Inc.",
                ["E0699A"] = "Apple, Inc.",
                ["A483E7"] = "Apple, Inc.",
                ["68D93C"] = "Apple, Inc.",
                ["9027E4"] = "Apple, Inc.",
                ["7CD1C3"] = "Apple, Inc.",
                ["D89E3F"] = "Apple, Inc.",
                ["ACBC32"] = "Apple, Inc.",

                // Google
                ["3C5AB4"] = "Google, Inc.",
                ["5460AA"] = "Google, Inc.",
                ["F4F5D8"] = "Google, Inc.",

                // Raspberry Pi
                ["B827EB"] = "Raspberry Pi Foundation",
                ["DCA632"] = "Raspberry Pi Trading Ltd",
                ["E45F01"] = "Raspberry Pi Trading Ltd",
                ["28CDC1"] = "Raspberry Pi Trading Ltd",

                // Virtualization
                ["005056"] = "VMware, Inc.",
                ["000C29"] = "VMware, Inc.",
                ["000569"] = "VMware, Inc.",
                ["080027"] = "PCS Systemtechnik / Oracle VirtualBox",
                ["001C42"] = "Parallels, Inc.",
                ["00155D"] = "Microsoft Hyper-V",
                ["525400"] = "QEMU / KVM (virtual NIC)",

                // Microsoft
                ["0003FF"] = "Microsoft Corporation",
                ["00125A"] = "Microsoft Corporation",
                ["7C1E52"] = "Microsoft Corporation",

                // Realtek
                ["00E04C"] = "Realtek Semiconductor",
                ["52540A"] = "Realtek Semiconductor",

                // Netgear
                ["001F33"] = "Netgear",
                ["204E7F"] = "Netgear",
                ["841B5E"] = "Netgear",
                ["A040A0"] = "Netgear",
                ["00146C"] = "Netgear",
                ["0024B2"] = "Netgear",

                // Linksys / Cisco-Linksys
                ["001D7E"] = "Cisco-Linksys",
                ["00226B"] = "Cisco-Linksys",
                ["002369"] = "Cisco-Linksys",
                ["001B2F"] = "Cisco-Linksys",
                ["000C41"] = "Linksys",
                ["001310"] = "Linksys",

                // TP-Link
                ["C0C1C0"] = "TP-Link Technologies",
                ["50C7BF"] = "TP-Link Technologies",
                ["98DAC4"] = "TP-Link Technologies",
                ["EC086B"] = "TP-Link Technologies",
                ["F4F26D"] = "TP-Link Technologies",
                ["001D0F"] = "TP-Link Technologies",

                // ASUS
                ["246968"] = "ASUSTek Computer",
                ["08606E"] = "ASUSTek Computer",
                ["1C872C"] = "ASUSTek Computer",
                ["2C56DC"] = "ASUSTek Computer",
                ["000C6E"] = "ASUSTek Computer",

                // D-Link
                ["002618"] = "D-Link Corporation",
                ["14D64D"] = "D-Link Corporation",
                ["1C7EE5"] = "D-Link Corporation",
                ["00055D"] = "D-Link Corporation",
                ["001B11"] = "D-Link Corporation",

                // Ubiquiti
                ["E8CC18"] = "Ubiquiti Networks",
                ["24A43C"] = "Ubiquiti Networks",
                ["687251"] = "Ubiquiti Networks",
                ["7483C2"] = "Ubiquiti Networks",
                ["788A20"] = "Ubiquiti Networks",
                ["FCECDA"] = "Ubiquiti Networks",
                ["00156D"] = "Ubiquiti Networks",
                ["B4FBE4"] = "Ubiquiti Networks",

                // Huawei
                ["0023AB"] = "Huawei Technologies",
                ["00E0FC"] = "Huawei Technologies",
                ["101B54"] = "Huawei Technologies",
                ["4C1FCC"] = "Huawei Technologies",

                // Sony
                ["001A6B"] = "Sony Corporation",
                ["00041F"] = "Sony Corporation",
                ["0024BE"] = "Sony Corporation",
                ["30F9ED"] = "Sony Corporation",

                // LG
                ["0016FE"] = "LG Electronics",
                ["001C62"] = "LG Electronics",
                ["001E75"] = "LG Electronics",

                // Samsung
                ["001247"] = "Samsung Electronics",
                ["001599"] = "Samsung Electronics",
                ["001D25"] = "Samsung Electronics",
                ["5C0A5B"] = "Samsung Electronics",
                ["8C7712"] = "Samsung Electronics",
                ["BC1485"] = "Samsung Electronics",

                // Dell
                ["001EC9"] = "Dell Inc.",
                ["001422"] = "Dell Inc.",
                ["00219B"] = "Dell Inc.",
                ["0026B9"] = "Dell Inc.",
                ["180373"] = "Dell Inc.",
                ["B82A72"] = "Dell Inc.",
                ["000F1F"] = "Dell Inc.",
                ["001C23"] = "Dell Inc.",

                // HP
                ["F48E38"] = "Hewlett Packard",
                ["000E7F"] = "Hewlett Packard",
                ["001F29"] = "Hewlett Packard",
                ["00237D"] = "Hewlett Packard",
                ["3CD92B"] = "Hewlett Packard",
                ["000BCD"] = "Hewlett Packard",

                // Lenovo
                ["001B78"] = "Lenovo",
                ["54EE75"] = "Lenovo",
                ["002655"] = "Lenovo",
                ["E454E8"] = "Lenovo",

                // Intel
                ["0021CC"] = "Intel Corporation",
                ["001517"] = "Intel Corporation",
                ["001B21"] = "Intel Corporation",
                ["001E64"] = "Intel Corporation",
                ["0024D6"] = "Intel Corporation",
                ["3CA9F4"] = "Intel Corporation",
                ["A434D9"] = "Intel Corporation",
                ["F81654"] = "Intel Corporation",

                // Amazon
                ["340286"] = "Amazon Technologies",
                ["44650D"] = "Amazon Technologies",
                ["6837E9"] = "Amazon Technologies",
                ["74C246"] = "Amazon Technologies",
                ["AC63BE"] = "Amazon Technologies",
                ["FC65DE"] = "Amazon Technologies",
                ["40B4CD"] = "Amazon Technologies",
                ["18742E"] = "Amazon Technologies",

                // Philips (Hue)
                ["001788"] = "Philips Lighting",
                ["ECB5FA"] = "Philips Lighting",

                // Nokia
                ["001A22"] = "Nokia",
                ["0002EE"] = "Nokia",

                // Belkin
                ["0026CC"] = "Belkin International",
                ["94103E"] = "Belkin International",
                ["C05627"] = "Belkin International",

                // Sonos
                ["44D9E7"] = "Sonos, Inc.",
                ["5CAAFD"] = "Sonos, Inc.",
                ["949F3E"] = "Sonos, Inc.",
                ["B8E937"] = "Sonos, Inc.",

                // Roku
                ["B0C554"] = "Roku, Inc.",
                ["CC6DA0"] = "Roku, Inc.",
                ["DC3A5E"] = "Roku, Inc.",
                ["AC3A7A"] = "Roku, Inc.",

                // Nest / Google Nest Labs
                ["44D32D"] = "Nest Labs",
                ["18B430"] = "Nest Labs",
                ["641666"] = "Nest Labs",
            };
        }
    }
}
