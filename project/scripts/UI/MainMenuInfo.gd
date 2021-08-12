extends Label


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	text = Global.TranslateWithInstance("main_menu_info").format([
		Global.Version,
		OS.get_user_data_dir()
	], "{}")
