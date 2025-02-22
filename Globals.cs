using Godot;
using System;
using RhythmGame;

[GlobalClass]
public partial class Globals : Resource
{
	/** Events **/
	public delegate Vector2I GetTileEventHandler(double x, double y);
	public static event GetTileEventHandler GetTileEvent;
	public static void RaiseGetTileEvent(double x, double y) => GetTileEvent?.Invoke(x, y);
	
	public delegate int InteractTileEventHandler(double x, double y);

	public static event InteractTileEventHandler InteractTileEvent;

	public static void RaiseInteractTileEvent(double x, double y) => InteractTileEvent?.Invoke(x, y);

	public delegate void DestroyTileEventHandler(Vector2I tile);

	public static event DestroyTileEventHandler DestroyTileEvent;
	public static void DestroyTile(Vector2I tile) => DestroyTileEvent?.Invoke(tile);
	
	/** End Events **/
	
	public const int TileLength = 16;
	public static Color MetronomeOffColor = Colors.Black;
	public static Color MetronomeOnColor = Colors.White;
	public const int TimeSignature = 4;

	public static GDScript DebugConsole = GD.Load<GDScript>("res://addons/copper_dc/scripts/debug_console.gd");
	
	public enum Layers
	{
		TileLayer,
		PlaceableLayer,
		Null,
	}
	public interface ITile
	{
		public Vector2I AtlasCoords { get; set; }
		string Name { get; init; }
		const Layers Layer = Layers.Null;
	}
	public struct Placeable : ITile
	{
		public Vector2I AtlasCoords { get; set; }
		public string Name { get; init; }
		public const Layers Layer = Layers.PlaceableLayer;
		public int Health { get; private set; } = -1;
		public bool Unbreakable { private set; get; } = false;

		private void DestroyThisTile()
		{
			DestroyTile(AtlasCoords);
		}

		public void DecrementHealth()
		{
			if (Unbreakable)
				return;
			Health -= 1;

			if (Health <= 0)
			{
				DestroyThisTile();
			}
		}

		public Placeable(string name, Vector2I atlasCoords)
		{
			Name = name;
			AtlasCoords = atlasCoords;
		}
		
		public Placeable(string name, Vector2I atlasCoords, int health)
		{
			Name = name;
			AtlasCoords = atlasCoords;
			Health = health;
		}
		
		public Placeable(string name, Vector2I atlasCoords, bool unbreakable)
		{
			Name = name;
			AtlasCoords = atlasCoords;
			Unbreakable = unbreakable;
		}
	}

	public struct Tile : ITile
	{
		public Vector2I AtlasCoords { get; set; }
		public string Name { get; init; }
		public const Layers Layer = Layers.TileLayer;

		public Tile(string name, Vector2I atlasCoords)
		{
			Name = name;
			AtlasCoords = atlasCoords;
		}
	}

	public static class Placeables
	{
		public static Placeable Tree = new ("Tree", new Vector2I(13, 9), 5);
	}

	public static class Tiles
	{
		public static Tile Grass = new ("GRASS", new Vector2I(5, 0));
		public static Tile Dirt = new ("Dirt", new Vector2I(6, 0));
	}

	public static Vector2I GetRoundedXy(Vector2 xy)
	{
		return new Vector2I(Mathf.FloorToInt(xy.X / TileLength), Mathf.FloorToInt(xy.Y / TileLength));
	}
	
	public static Vector2I GetRoundedXy(float x, float y)
	{
		return new Vector2I(Mathf.FloorToInt(x / TileLength), Mathf.FloorToInt(y / TileLength));
	}

	public static bool IsAdjacent(TileMapLayer tileMapLayer, Vector2I coords, ITile cellToAvoid)
	{
		var atlasCoords = cellToAvoid.AtlasCoords;
		for (var i = 0L; i < 16L; i++)
		{
			if (tileMapLayer.GetNeighborCell(coords, (TileSet.CellNeighbor)i) == atlasCoords)
			{
				return true;
			}
		}

		return false;
	}
}
