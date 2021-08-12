extends Particles

onready var timer = lifetime

func _process(delta):
	timer-=delta
	if timer<=0:
		queue_free()
