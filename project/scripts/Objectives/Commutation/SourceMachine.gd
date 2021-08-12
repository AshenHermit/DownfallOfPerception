extends Spatial



func _on_WireInput_wire_connected(wire):
	get_child(0).get_node("AnimationPlayer").play("activate")
	Global.GetAudioManager().PlaySoundAtPosition(load("res://resources/sounds/source_machine/source_machine_turn_on.ogg"), 
				global_transform.origin, self, "world");
	get_node("SparksSound").stop()
	get_node("WorkingSound").play()
	Global.GetGenerationManager().ActionHappened("source_machine_connected")
	
