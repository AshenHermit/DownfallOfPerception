extends Control

#TODO:KLUDGE:

var fade_amount = 0.0
var fade_speed = 1.0
var fading = false

func _ready():
	visible = true
	update_modulation()

func start_fading(new_fade_speed):
	fading = true
	fade_speed = new_fade_speed
	
func update_modulation():
	modulate = Color(0,0,0,fade_amount)
	
func _process(delta):
	if fading:
		fade_amount = min(fade_amount + delta * fade_speed, 1.0)
	update_modulation()
	
