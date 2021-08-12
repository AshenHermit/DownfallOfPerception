extends Control

export(String) var string_id = "label"

func _ready():
	set("text", Global.TranslateWithInstance(string_id))

