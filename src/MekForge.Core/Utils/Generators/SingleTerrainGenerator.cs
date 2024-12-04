using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Terrains;
using Sanet.MekForge.Core.Exceptions;

namespace Sanet.MekForge.Core.Utils.Generators;

public class SingleTerrainGenerator : ITerrainGenerator
{
    private readonly int _width;
    private readonly int _height;
    private readonly Terrain _terrain;

    public SingleTerrainGenerator(int width, int height, Terrain terrain)
    {
        _width = width;
        _height = height;
        _terrain = terrain;
    }

    public Hex Generate(HexCoordinates coordinates)
    {
        if (coordinates.Q < 0 || coordinates.Q >= _width ||
            coordinates.R < 0 || coordinates.R >= _height)
        {
            throw new HexOutsideOfMapBoundariesException(coordinates, _width, _height);
        }
        
        return new Hex(coordinates).WithTerrain(_terrain);
    }
}
