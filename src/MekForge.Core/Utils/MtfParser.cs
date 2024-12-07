using System.Text.RegularExpressions;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Utils;

public class MtfParser
{
    private readonly Dictionary<string, string> _mechData = new();
    private readonly Dictionary<PartLocation, List<string>> _locationEquipment = new();
    private readonly Dictionary<PartLocation, ArmorValues> _armorValues = new();

    public MechData Parse(IEnumerable<string> lines)
    {
        ParseBasicData(lines);
        ParseLocationData(lines);
        
        return new MechData
        {
            Chassis = _mechData["chassis"],
            Model = _mechData["model"],
            Mass = int.Parse(_mechData["Mass"]),
            WalkMp = int.Parse(Regex.Match(_mechData["Walk MP"], @"\d+").Value),
            ArmorValues = _armorValues,
            LocationEquipment = _locationEquipment
        };
    }

    private void ParseBasicData(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Config:") || line.StartsWith("quirk:"))
                continue;

            var colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                var key = line[..colonIndex].Trim();
                var value = line[(colonIndex + 1)..].Trim();
                _mechData[key] = value;
            }
        }
    }

    private void ParseLocationData(IEnumerable<string> lines)
    {
        PartLocation? currentLocation = null;
        bool parsingArmor = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Start of armor section
            if (line.StartsWith("Armor:"))
            {
                parsingArmor = true;
                continue;
            }

            // End of armor section
            if (line.StartsWith("Weapons:"))
            {
                parsingArmor = false;
                continue;
            }

            // Parse armor values
            if (parsingArmor)
            {
                var match = Regex.Match(line, @"(\w+)\s+Armor:(\d+)");
                if (match.Success && TryParseLocation(match.Groups[1].Value, out var location))
                {
                    var value = int.Parse(match.Groups[2].Value);
                    if (!_armorValues.ContainsKey(location))
                        _armorValues[location] = new ArmorValues();

                    // Handle rear armor values
                    if (IsRearArmor(match.Groups[1].Value))
                    {
                        var mainLocation = GetMainLocationForRear(match.Groups[1].Value);
                        if (!_armorValues.ContainsKey(mainLocation))
                            _armorValues[mainLocation] = new ArmorValues();
                        _armorValues[mainLocation].RearArmor = value;
                    }
                    else
                    {
                        _armorValues[location].FrontArmor = value;
                    }
                }
                continue;
            }

            // Check for location headers
            if (line.EndsWith(":"))
            {
                var locationText = line[..^1].Trim();
                if (TryParseLocation(locationText, out var location))
                {
                    currentLocation = location;
                    if (!_locationEquipment.ContainsKey(location))
                        _locationEquipment[location] = new List<string>();
                }
                continue;
            }

            // Add equipment to current location
            if (currentLocation.HasValue && !line.Contains("-Empty-"))
            {
                _locationEquipment[currentLocation.Value].Add(line.Trim());
            }
        }
    }

    private static bool TryParseLocation(string locationText, out PartLocation location)
    {
        location = locationText switch
        {
            "Left Arm" or "LA" => PartLocation.LeftArm,
            "Right Arm" or "RA" => PartLocation.RightArm,
            "Left Torso" or "LT" => PartLocation.LeftTorso,
            "Right Torso" or "RT" => PartLocation.RightTorso,
            "Center Torso" or "CT" => PartLocation.CenterTorso,
            "Head" or "HD" => PartLocation.Head,
            "Left Leg" or "LL" => PartLocation.LeftLeg,
            "Right Leg" or "RL" => PartLocation.RightLeg,
            "RTL" or "RTR" or "RTC" => GetMainLocationForRear(locationText),
            _ => throw new ArgumentException($"Unknown location: {locationText}")
        };
        return true;
    }

    private static bool IsRearArmor(string locationText)
    {
        return locationText is "RTL" or "RTR" or "RTC";
    }

    private static PartLocation GetMainLocationForRear(string rearLocationText) => rearLocationText switch
    {
        "RTL" => PartLocation.LeftTorso,
        "RTR" => PartLocation.RightTorso,
        "RTC" => PartLocation.CenterTorso,
        _ => throw new ArgumentException($"Invalid rear location: {rearLocationText}")
    };
}

public class ArmorValues
{
    public int FrontArmor { get; set; }
    public int RearArmor { get; set; }
}

public class MechData
{
    public required string Chassis { get; init; }
    public required string Model { get; init; }
    public required int Mass { get; init; }
    public required int WalkMp { get; init; }
    public required Dictionary<PartLocation, ArmorValues> ArmorValues { get; init; }
    public required Dictionary<PartLocation, List<string>> LocationEquipment { get; init; }
}
