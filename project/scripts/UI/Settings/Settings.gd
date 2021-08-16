extends Container

#TODO: this shit is not automated, settings should be automaticly generated

export var container_path:NodePath
export var language_menu_path:NodePath
export var fullscreen_button_path:NodePath
export var fxaa_path:NodePath
export var debug_path:NodePath
export var volume_path:NodePath
export var volume_num_path:NodePath
export var mouse_sensivity_path:NodePath
export var mouse_sensivity_num_path:NodePath

func _ready():
	var container:MarginContainer = get_node(container_path)
	container.margin_right = 800
	var lang_menu:OptionButton = get_node(language_menu_path)
	var fullscreen:CheckButton = get_node(fullscreen_button_path)
	var debug:CheckButton = get_node(debug_path)
	var fxaa:CheckButton = get_node(fxaa_path)
	var volume:HSlider = get_node(volume_path)
	var mouse_sensivity:HSlider = get_node(mouse_sensivity_path)
	
	#TODO: make this automated
	debug.pressed = Global.GetGameStorage().Settings["debug"]
	fullscreen.pressed = Global.GetGameStorage().Settings["fullscreen"]
	fxaa.pressed = Global.GetGameStorage().Settings["fxaa"]
	volume.value = Global.GetGameStorage().Settings["volume"]
	mouse_sensivity.value = Global.GetGameStorage().Settings["mouse_sensivity"]
	update_volume_num()
	update_mouse_sensivity_num()
	
	for locale in Global.Locales:
		lang_menu.add_item(locale)
	lang_menu.select(Global.Locales.find(Global.GetGameStorage().Settings["locale"]))
	pass

func update_volume_num():
	get_node(volume_num_path).text = str(Global.GetGameStorage().Settings["volume"]) + "db"
	
func update_mouse_sensivity_num():
	get_node(mouse_sensivity_num_path).text = str(int(Global.GetGameStorage().Settings["mouse_sensivity"])) + "%"

func _process(delta):
	#get_node(container_path).margin_left = get_viewport().size.x-800
	#get_node(container_path).margin_right = 800
	if Input.is_action_just_pressed("show_game_menu"):
		_on_GoBackButton_pressed()

func _on_GoBackButton_pressed():
	Global.GetGameStorage().SaveSettings()
	queue_free()

func _on_Language_item_selected(index):
	Global.GetGameStorage().Settings["locale"] = Global.Locales[index]
	Global.GetGameStorage().ApplySettings()

func _on_Fullscreen_toggled(button_pressed):
	Global.GetGameStorage().Settings["fullscreen"] = button_pressed
	Global.GetGameStorage().ApplySettings()

func _on_FXAA_toggled(button_pressed):
	Global.GetGameStorage().Settings["fxaa"] = button_pressed
	Global.GetGameStorage().ApplySettings()

func _on_Debug_toggled(button_pressed):
	Global.GetGameStorage().Settings["debug"] = button_pressed
	Global.GetGameStorage().ApplySettings()

func _on_Volume_value_changed(value):
	Global.GetGameStorage().Settings["volume"] = value
	update_volume_num()
	Global.GetGameStorage().ApplySettings()

func _on_MouseSensivity_value_changed(value):
	Global.GetGameStorage().Settings["mouse_sensivity"] = value
	update_mouse_sensivity_num()
	Global.GetGameStorage().ApplySettings()
