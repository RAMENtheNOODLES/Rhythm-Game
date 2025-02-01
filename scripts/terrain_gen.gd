extends Node2D

@export_group("Layers")
@export var tile_map_layer: TileMapLayer
@export var placeables_layer: TileMapLayer

var screen_size
var _noise     = FastNoiseLite.new()
var RANGE: int = 128


# Called when the node enters the scene tree for the first time.
func _ready():
	screen_size = get_viewport_rect().size
	gen_terrain()
	Global.get_tile_xy.connect(get_tile_from_xy)
	Globals.interact_tile.connect(_interact_tile)

func gen_terrain():
	_noise.seed = randi()
	_noise.fractal_octaves = 2
	_noise.fractal_lacunarity = 1.575
	_noise.frequency = 0.05
	_noise.noise_type = 3

	var _k = 0
	
	for _i in range(-RANGE, RANGE):
		for _j in range(-RANGE, RANGE):
			var _dist = Vector2(_i, _j).distance_to(Vector2(0, 0)) / RANGE
			_k = _noise.get_noise_2d(_i, _j) - _dist

			if _k < -0.3:
				tile_map_layer.set_cell(Vector2i(_i, _j), 1, Global.TILES.GRASS)
			elif _k > -0.3 && _k <= -0.1:
				tile_map_layer.set_cell(Vector2i(_i, _j), 1, Global.TILES.GRASS)
			elif _k > -0.1 && _k <= 0:
				tile_map_layer.set_cell(Vector2i(_i, _j), 1, Global.TILES.GRASS)
			elif _k > 0 && _k <= 0.1:
				tile_map_layer.set_cell(Vector2i(_i, _j), 1, Global.TILES.DIRT)
			elif _k > 0.1:
				tile_map_layer.set_cell(Vector2i(_i, _j), 1, Global.TILES.DIRT)
				placeables_layer.set_cell(Vector2i(_i, _j), 1, Global.PLACEABLES.TREE)
				var data = placeables_layer.get_cell_tile_data(Vector2i(_i, _j))
				
				if (data):
					data.set_custom_data_by_layer_id(1, 5)
					print("setting data")
					print(data.get_custom_data_by_layer_id(1))
				

func get_tile_from_xy(x, y):
	var _x = round(x / Global.tile_length)
	var _y = round(y / Global.tile_length)
	if (placeables_layer.get_cell_atlas_coords(Vector2i(x, y)) == Vector2i(-1, -1)):
		return tile_map_layer.get_cell_atlas_coords(Vector2i(x, y))
	return placeables_layer.get_cell_atlas_coords(Vector2i(x, y))
	
func _interact_tile(x, y):
	var _x = x / Global.tile_length
	var _y = y / Global.tile_length
	if (placeables_layer.get_cell_atlas_coords(Vector2i(_x, _y)) == Global.PLACEABLES.TREE):
		print("Mining Tree...")
		var data = placeables_layer.get_cell_tile_data(Vector2i(_x, _y))
		if data:
			if (data.get_custom_data("health") <= 0):
				Global.placeable_destroyed.emit(Global.PLACEABLES.PLACEABLES.TREE)
				placeables_layer.erase_cell(Vector2i(_x, _y))
			else:
				data.set_custom_data("health", data.get_custom_data("health") - 1)
	else:
		print("Nothing found...")
		return -1

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
