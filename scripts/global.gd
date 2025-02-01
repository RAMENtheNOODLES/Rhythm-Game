extends Node2D

static var time_signature := 4

static var metronome_off_color = Color.BLACK
static var metronome_on_color = Color.WHITE
static var tile_length = 16

class PLACEABLES:
	enum PLACEABLES {TREE}
	const TREE = Vector2i(13, 9)
	
class TILES:
	const GRASS = Vector2i(5, 0)
	const DIRT = Vector2i(6, 0)

signal get_tile_xy(x, y)
signal return_tile(x, y, tile)
signal placeable_destroyed(placeable: PLACEABLES.PLACEABLES)
signal interact_tile(x, y)
signal tick(beat_on: int)

static func get_rounded_xy(x, y):
	return [floor(x / tile_length), floor(y / tile_length)]
