extends Spatial

export var speed = 1.0
export var amplitude = 1.0  #in degrees

var timer = 0.0

func _ready():
	timer = randf()*PI

func _process(delta):
	timer += delta*speed
	rotation.z = sin(timer*PI)*deg2rad(amplitude)
	
