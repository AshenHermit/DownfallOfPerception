extends Container

#TODO: this shit is not automated, settings should be automaticly generated

export var language_menu_path:NodePath
export var fullscreen_button_path:NodePath
export var fxaa_path:NodePath
export var debug_path:NodePath
export var volume_path:NodePath
export var volume_num_path:NodePath

func _ready():
	var lang_menu:OptionButton = get_node(language_menu_path)
	var fullscreen:CheckButton = get_node(fullscreen_button_path)
	var debug:CheckButton = get_node(debug_path)
	var fxaa:CheckButton = get_node(fxaa_path)
	var volume:HSlider = get_node(volume_path)
	
	#TODO: make this automated
	debug.pressed = Global.GetGameStorage().Settings["debug"]
	fullscreen.pressed = Global.GetGameStorage().Settings["fullscreen"]
	fxaa.pressed = Global.GetGameStorage().Settings["fxaa"]
	volume.value = Global.GetGameStorage().Settings["volume"]
	update_volume_num()
	
	for locale in Global.Locales:
		lang_menu.add_item(locale)
	lang_menu.select(Global.Locales.find(Global.GetGameStorage().Settings["locale"]))
	pass

func update_volume_num():
	get_node(volume_num_path).text = str(Global.GetGameStorage().Settings["volume"]) + "db"

func _process(delta):
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
