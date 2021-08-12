extends Button

enum ButtonType{
	CONTINUE,
	NEW_GAME,
	SETTINGS,
	EXIT,
}

export(ButtonType) var button_type
export(NodePath) var screen_fade_path

var new_game_sound = preload("res://resources/sounds/misc/start_new_game_sound.mp3")

func _ready():
	var key = "main_menu_button."+ButtonType.keys()[button_type]
	text = Global.TranslateWithInstance(key)
	margin_right = 120

	
	if button_type == ButtonType.CONTINUE:
		if Global.GetGameStorage().Stats["game_saves"].size()==0 or Global.GetGameStorage().Stats["last_game_save_id"] == -1:
			disabled = true

func continue_game():
	pass

func load_game():
	pass

func open_scene(scene):
	get_node("/root/MainMenu").call_deferred("free")
	var instance = scene.instance()
	get_node("/root").add_child(instance)

func _on_pressed():
	if button_type == ButtonType.CONTINUE:
		Global.GetGameStorage().ContinueGame()
	if button_type == ButtonType.NEW_GAME:
		Global.GetAudioManager().PlayNonSpatialSound(new_game_sound, "master")
		get_node(screen_fade_path).start_fading(1.0/3.0)
		yield(get_tree().create_timer(3.0), "timeout")
		Global.GetGameStorage().StartNewGame()
	if button_type == ButtonType.SETTINGS:
		var settings = load("res://game_objects/UI/Settings.tscn").instance()
		Global.CurrentSceneInstance.get_node("MainMenu").add_child(settings)
	if button_type == ButtonType.EXIT:
		Global.ExitGame()
		
