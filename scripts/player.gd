extends CharacterBody2D

@export var speed = 360
@export var interact_cursor: ColorRect

signal hit
var screen_size
# 0 - up, 1 - right, 2 - down, 3 - left
var facing = 0

@export var INTERACT_TIMER: Timer

# Called when the node enters the scene tree for the first time.
func _ready():
	screen_size = get_viewport_rect().size
	Global.return_tile.connect(_return_tile)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	velocity = Vector2.ZERO # The player's movement vector.
	if Input.is_action_pressed("move_right"):
		facing = 1
		velocity.x += 1
	if Input.is_action_pressed("move_left"):
		facing = 3
		velocity.x -= 1
	if Input.is_action_pressed("move_down"):
		facing = 2
		velocity.y += 1
	if Input.is_action_pressed("move_up"):
		facing = 0
		velocity.y -= 1

	if velocity.length() > 0:
		velocity = velocity.normalized() * speed
		$AnimatedSprite2D.play()
	else:
		$AnimatedSprite2D.stop()

	move_and_slide()
	_move_cursor()
	Global.get_tile_xy.emit(position.x, position.y)
	
	if Input.is_action_pressed("interact") and INTERACT_TIMER.time_left == 0:
		Global.interact_tile.emit(interact_cursor.position.x, interact_cursor.position.y)
		INTERACT_TIMER.start()
	
func _move_cursor():
	var xy = Global.get_rounded_xy(position.x, position.y)
	var x = xy[0] * 16
	var y = xy[1] * 16
	if (facing == 0):
		interact_cursor.position = Vector2i(x, y - 16)
	if (facing == 1):
		interact_cursor.position = Vector2i(x + 16, y)
	if (facing == 2):
		interact_cursor.position = Vector2i(x, y + 16)
	if (facing == 3):
		interact_cursor.position = Vector2i(x - 16, y)
	
func _return_tile(x, y, tile):
	pass
