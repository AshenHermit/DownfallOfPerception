extends Label

func _init():
	if not Global.DEBUG:
		visible = false
		queue_free()
		

func _process(delta):
	text = str(Global.GetPlayer().global_transform.origin.round())
	
