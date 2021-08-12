extends Button

export var game_save:String

var test_scene_name = ""

func _ready():
	if game_save.begins_with("_debug_load_test."):
		test_scene_name = game_save.substr(game_save.find(".")+1)
		text = "[DEBUG] Load Test: " + test_scene_name
	else:
		text = game_save
		



func _on_pressed():
	if test_scene_name != "":
		Global.LoadScene("res://scenes/tests/"+test_scene_name+".tscn", 0, false)
		Global.GetPlayer().SetMaxHealth(100.0)
		Global.GetPlayer().SetHealth(100.0)
	else:
		Global.GetGameStorage().LoadGameByName(game_save)
