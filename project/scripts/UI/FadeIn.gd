extends Control

export var wait:float = 1.0
export var fade_time:float = 1.0
export var if_game_not_booted:bool = false

var fading = false
var timer = 1.0

func _init():
	visible = true
	modulate = Color(1.0, 1.0, 1.0, 0.0)

func _ready():
	timer = wait

func _process(delta):
	if !fading:
		timer-=delta
		if timer <= 0:
			timer = fade_time
			fading = true
	else:
		timer-=delta
		modulate = Color(1.0, 1.0, 1.0, min(1.0, 1.0-timer/fade_time))
	
