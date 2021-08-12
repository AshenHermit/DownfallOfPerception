extends Spatial

export var wirePath:NodePath

onready var wire = get_node(wirePath)

func _ready():
	pass

func request_wire():
	return wire
