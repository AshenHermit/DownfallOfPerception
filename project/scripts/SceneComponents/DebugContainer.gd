extends Node

func _ready():
	if not Global.DEBUG:
		queue_free()
