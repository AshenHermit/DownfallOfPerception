extends Spatial

export var indicators_path:NodePath
export var teleport_trigger_path:NodePath


var layers = ["Begining", "Withering", "Memory Storage"]
onready var indicators_node:Node = get_node(indicators_path)
onready var teleport_trigger:Area = get_node(teleport_trigger_path)
onready var chosen_layer_id = Global.SpawnPointId
onready var chosen_layer = layers[chosen_layer_id]


var entered_pos:Vector3

func _ready():
	prepare_chosen_layer()
	
func prepare_chosen_layer():
	chosen_layer_id = clamp(chosen_layer_id, 0, layers.size()-1)
	chosen_layer = layers[chosen_layer_id]
	
	for indicator in indicators_node.get_children():
		indicator.visible = false
	indicators_node.get_node(chosen_layer).visible = true
	
	teleport_trigger.scene_name = chosen_layer

func scroll_layers(scroll):
	chosen_layer_id = clamp(chosen_layer_id+scroll, 0, layers.size()-1)
	prepare_chosen_layer()


func _on_LayerChangeTrigger_body_entered(body):
	if body == Global.GetPlayer():
		entered_pos = body.global_transform.origin
func _on_LayerChangeTrigger_body_exited(body):
	if body == Global.GetPlayer():
		if body.global_transform.origin.z-entered_pos.z > 0.0:
			scroll_layers(1)
		else:
			scroll_layers(-1)
			
			
