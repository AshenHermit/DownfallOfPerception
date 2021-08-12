extends Node

export var key:String = ""
export var is_array:bool = false

func _ready():
	call_deferred("register")
func register():
	if get_child_count() == 0:
		Global.GetGenerationManager().RegisterNode(key, self, false)
	else:
		if is_array:
			for child in get_children():
				Global.GetGenerationManager().RegisterNode(key, child, true)
		else:
			Global.GetGenerationManager().RegisterNode(key, get_child(0), false)
		
	
