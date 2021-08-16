extends Control

export(NodePath) var modal_window_path

func _ready():
	set("text", Global.TranslateWithInstance("ui_button.CLOSE"))
	
func _on_pressed():
	get_node(modal_window_path).queue_free()
