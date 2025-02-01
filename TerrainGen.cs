using System;
using Godot;

namespace RhythmGame;

public partial class TerrainGen : Node
{
    [ExportGroup("layers")]
    [Export]
    public TileMapLayer TileMapLayer;
    [Export]
    public TileMapLayer PlaceablesLayer;
    
    private Vector2 screenSize;
    private FastNoiseLite noise = new FastNoiseLite();
    private const int RANGE = 128;

    public override void _Ready()
    {
        screenSize = GetViewport().GetVisibleRect().Size;
    }
    
    public override void _Process(double delta)
    {
        // Called every frame. Delta is time since the last frame.
        // Update game logic here.
    }

    private void gen_terrain()
    {
        Random random = new Random();
        noise.Seed = (int) random.NextInt64();
        noise.FractalOctaves = 2;
        noise.FractalLacunarity = 1.575f;
        noise.Frequency = 0.05f;
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

        double k = 0;

        for (int i = -RANGE; i < RANGE; i++)
        {
            for (int j = -RANGE; j < RANGE; j++)
            {
                double dist = new Vector2(i, j).DistanceTo(new Vector2()) / RANGE;

                k = noise.GetNoise2D(i, j) - dist;

                if (k < -0.3)
                {
                    TileMapLayer.SetCell(new Vector2I(i, j), 1);
                }
                else if (k > -0.3 && k <= -0.1)
                {
                    TileMapLayer.SetCell(new Vector2I(i, j), 0, Globals.Placeable);
                }
            }
        }
    }
}