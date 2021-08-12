extends Node

var this

var state = false
var use_text:String

func _ready():
	use_text
	this.SetUsableState(true)
	pass

func _on_use(invoker):
	state
	this.SetUsableState(false)
	pass
