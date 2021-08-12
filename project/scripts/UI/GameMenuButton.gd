extends Button

enum ButtonType{
	RETURN,
	LOAD_LAST_SAVE,
	SETTINGS,
	EXIT_TO_MAIN_MENU,
	EXIT_TO_DESKTOP,
	DEBUG_SAVE,
}

export(ButtonType) var button_type

func _ready():
	var key = "game_menu_button."+ButtonType.keys()[button_type]
	text = Global.TranslateWithInstance(key)

func _on_pressed():
	if button_type == ButtonType.RETURN:
		Global.GetUIManager().ShowOverlay(0)
	if button_type == ButtonType.LOAD_LAST_SAVE:
		Global.GetGameStorage().ContinueGame()
	if button_type == ButtonType.SETTINGS:
		var settings = load("res://game_objects/UI/Settings.tscn").instance()
		Global.CurrentSceneInstance.get_node("UI").add_child(settings)
	if button_type == ButtonType.EXIT_TO_MAIN_MENU:
		Global.LoadScene("res://scenes/MainMenu.tscn",0,false)
	if button_type == ButtonType.EXIT_TO_DESKTOP:
		Global.ExitGame()
	if button_type == ButtonType.DEBUG_SAVE:
		Global.GetGameStorage().SaveGame()
