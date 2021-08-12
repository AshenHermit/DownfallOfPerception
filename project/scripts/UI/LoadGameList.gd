extends VBoxContainer

export(PackedScene) var button_scene



func _ready():
	
	
	for game_save in Global.GetGameStorage().Stats["game_saves"]:
		var button = button_scene.instance()
		button.game_save = game_save
		add_child(button)
