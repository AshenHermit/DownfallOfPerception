extends Spatial

export var hightlight_material:Material
export var model_path:NodePath
export var wire_end_path:NodePath
export var wire_id:String

onready var wire_end:MeshInstance = get_node(wire_end_path)

signal wire_connected(wire)

var wire
var highlighted = false

func _ready():
	wire_end.visible = false
	wire_end.material_override = hightlight_material
	pass
	
func _process(delta):
	if wire == null:
		if highlighted:
			wire_end.visible = true
			highlighted = false
		else:
			wire_end.visible = false
	
func set_material(mat):
	pass

func connect_wire(wire_to_connect):
	wire = wire_to_connect
	wire.SetEndPoint(global_transform.origin)
	wire_end.visible = true
	wire_end.material_override = null
	emit_signal("wire_connected", wire)
	
func highlight():
	highlighted = true
	
	
