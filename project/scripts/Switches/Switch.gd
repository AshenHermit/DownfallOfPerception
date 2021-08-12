extends Object

var this:Spatial

# exports
var connection

onready var animation_player:AnimationPlayer = this.get_child(0).get_node("./AnimationPlayer")

var state = false
var ready_to_call = false

func play_state_animation():
	if state:
		animation_player.play("turn_on")
	else:
		animation_player.play("turn_off")

func _ready():
	set_state(state)
	
func _process(delta):
	if animation_player.current_animation_position >= 0.5 and ready_to_call:
		ready_to_call = false
		process_connection()


func _on_use(invoker):
	set_state(not state)
	
func set_state(new_state):
	state = new_state
	play_state_animation()
	ready_to_call = true
	

func process_connection():
	if connection is NodePath:
		update_connected_object_state(this.get_node(connection))
	elif connection is Array:
		for path in connection:
			update_connected_object_state(this.get_node(path))


func update_connected_object_state(object):
	if object is Light:
		object.visible = state
