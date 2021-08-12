extends Node

export var environment:Environment

var is_set = false

func _ready():
	pass

func _process(delta):
	Global.SetEnvironment(environment)
	is_set = true
