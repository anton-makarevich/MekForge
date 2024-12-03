using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Terrains;

namespace Sanet.MekForge.Core.Utils.Generators;

public class AlternatingRowsGenerator : ITerrainGenerator
{
    private readonly Terrain _evenTerrain;
    private readonly Terrain _oddTerrain;
    private readonly int _width;
    private readonly int _height;

    public AlternatingRowsGenerator(int width, int height, Terrain evenTerrain, Terrain oddTerrain)
    {
        _width = width;
        _height = height;
        _evenTerrain = evenTerrain;
        _oddTerrain = oddTerrain;
    }

    public Hex Generate(HexCoordinates coordinates)
    {
        if (coordinates.Q < 0 || coordinates.Q >= _width ||
            coordinates.R < 0 || coordinates.R >= _height)
        {
            return new Hex(coordinates);
        }
        
        return new Hex(coordinates)
            .WithTerrain(coordinates.R % 2 == 0 ? _evenTerrain : _oddTerrain);
    }
}
