using Godot;
using System;

[GlobalClass]
public partial class Globals : Resource
{
	public interface ITile
	{
		public static Vector2I AtlasCoords { get; set; }
		String Name;
	}
	public readonly struct Placeable : Tile
	{
		public static Vector2I AtlasCoords { get; set; }
		public readonly String Name;

		public Placeable(String name, Vector2I atlasCoords)
		{
			Name = name;
			AtlasCoords = atlasCoords;
		}
	}

	public class Placeables
	{
		public readonly Placeable Grass = new ("GRASS", new Vector2I(5, 0));
	}
}
