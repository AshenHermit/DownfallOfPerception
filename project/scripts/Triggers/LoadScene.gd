extends Node

export(String) var scene_name:String
export(int) var spawn_point_id:int = 0

func _on_Area_body_entered(body):
	if body == Global.GetPlayer():
		if scene_name!="":
			if Global.GetGenerationManager()!=null: Global.GetGenerationManager().ActionHappened("going_to_scene_"+scene_name)
			Global.LoadScene("res://scenes/"+scene_name+".tscn", spawn_point_id, true)
			


