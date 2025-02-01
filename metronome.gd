extends GridContainer

var active_tempobox = 0

# Called when the node enters the scene tree for the first time.
func _ready():
	pass
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass

func update_tempoboxes():
	$TempoBox1.modulate = Global.metronome_off_color
	$TempoBox2.modulate = Global.metronome_off_color
	$TempoBox3.modulate = Global.metronome_off_color
	$TempoBox4.modulate = Global.metronome_off_color
	
	var tempobox = "TempoBox%s" % [str(active_tempobox)]
	get_child(active_tempobox - 1).modulate = Global.metronome_on_color
	Global.tick.emit(active_tempobox)
	

func _on_timer_timeout():
	if active_tempobox + 1 > Global.time_signature:
		active_tempobox = 1
	else:
		active_tempobox += 1
		
	update_tempoboxes()
