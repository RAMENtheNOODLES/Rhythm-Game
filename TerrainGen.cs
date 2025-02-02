using System;
using System.Collections.Generic;
using Godot;

namespace RhythmGame;

public partial class TerrainGen : Node
{
    [ExportGroup("layers")]
    [Export]
    public TileMapLayer TileMapLayer;
    [Export]
    public TileMapLayer PlaceablesLayer;
    
    private Vector2 _screenSize;
    private FastNoiseLite _noise = new();
    private const int Range = 128;

    private Dictionary<Vector2I, Globals.ITile> _placeables = new();

    public override void _Ready()
    {
        _screenSize = GetViewport().GetVisibleRect().Size;
        Globals.GetTileEvent += get_tile_from_xy;
        Globals.InteractTileEvent += interact_tile;
        Globals.DestroyTileEvent += _DestroyTile;
        gen_terrain();
    }
    
    public override void _Process(double delta)
    {
        // Called every frame. Delta is time since the last frame.
        // Update game logic here.
    }

    private void gen_terrain()
    {
        var random = new Random();
        _noise.Seed = (int) random.NextInt64();
        _noise.FractalOctaves = 2;
        _noise.FractalLacunarity = 1.575f;
        _noise.Frequency = 0.05f;
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

        double k = 0;

        for (var i = -Range; i < Range; i++)
        {
            for (var j = -Range; j < Range; j++)
            {
                double dist = new Vector2(i, j).DistanceTo(new Vector2()) / Range;

                k = _noise.GetNoise2D(i, j) - dist;

                var coord = new Vector2I(i, j);

                switch (k)
                {
                    case < -0.3:
                        TileMapLayer.SetCell(coord, 1, Globals.Tiles.Grass.AtlasCoords);
                        break;
                    case > -0.3 and <= -0.1:
                        TileMapLayer.SetCell(coord, 1, Globals.Tiles.Grass.AtlasCoords);
                        break;
                    case > -0.1 and <= 0:
                        TileMapLayer.SetCell(coord, 1, Globals.Tiles.Grass.AtlasCoords);
                        break;
                    case > 0 and <= 0.1:
                        TileMapLayer.SetCell(coord, 1, Globals.Tiles.Dirt.AtlasCoords);
                        break;
                    case > 0.1:
                        TileMapLayer.SetCell(coord, 1 , Globals.Tiles.Dirt.AtlasCoords);
                        PlaceablesLayer.SetCell(coord, 1, Globals.Placeables.Tree.AtlasCoords);
                        _placeables.Add(coord, Globals.Placeables.Tree);
                        break;
                };
            }
        }
    }

    private Vector2I get_tile_from_xy(double x, double y)
    {
        var xRound = (int) Mathf.Round(x / Globals.TileLength);
        var yRound = (int)Mathf.Round(y / Globals.TileLength);

        var atlasCoords = PlaceablesLayer.GetCellAtlasCoords(new Vector2I(xRound, yRound));

        return atlasCoords == new Vector2I(-1, -1) ? TileMapLayer.GetCellAtlasCoords(new Vector2I(xRound, yRound)) : atlasCoords;
    }

    private int interact_tile(double x, double y)
    {
        var xRound = (int) Mathf.Round(x / Globals.TileLength);
        var yRound = (int)Mathf.Round(y / Globals.TileLength);
        
        var atlasCoords = PlaceablesLayer.GetCellAtlasCoords(new Vector2I(xRound, yRound));

        if (atlasCoords == Globals.Placeables.Tree.AtlasCoords)
        {
            GD.Print("Mining Tree...");
            var placeable = (Globals.Placeable) _placeables[new Vector2I(xRound, yRound)];
            placeable.DecrementHealth();
            GD.Print("Tree health: " + placeable.Health);
            _placeables[new Vector2I(xRound, yRound)] = placeable;
        }
        else
        {
            GD.Print("Nothing found...");
            return -1;
        }

        return 1;
    }

    private void _DestroyTile(Vector2I tile)
    {
        PlaceablesLayer.EraseCell(tile);
        _placeables.Remove(tile);
    }
}