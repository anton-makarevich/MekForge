namespace Sanet.MekForge.Core.Models.Units.Components.Engines;

public class Engine : Component
{
    public int Rating { get; }
    public EngineType Type { get; }

    // Engine takes slots 1-3 and 8-10 in CT
    private static readonly int[] EngineSlots = [0, 1, 2, 7, 8, 9];

    public Engine(string name, int rating, EngineType type = EngineType.Fusion) 
        : base(name, EngineSlots)
    {
        Rating = rating;
        Type = type;
    }
}
