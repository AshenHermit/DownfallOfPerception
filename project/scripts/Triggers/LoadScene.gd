extends Node

export(String) var scene_name:String
export(int) var spawn_point_id:int = 0
export(NodePath) var screen_fade_path:NodePath

func _on_Area_body_entered(body):
	if body == Global.GetPlayer():
		if scene_name!="":
			remove_screen_fade()
			if Global.GetGenerationManager()!=null: Global.GetGenerationManager().ActionHappened("going_to_scene_"+scene_name)
			Global.LoadScene("res://scenes/"+scene_name+".tscn", spawn_point_id, true)

func remove_screen_fade():
	if not screen_fade_path.is_empty():
		var screen_fade = get_node(screen_fade_path)
		screen_fade.get_parent().remove_child(screen_fade)
		screen_fade.free()
			


