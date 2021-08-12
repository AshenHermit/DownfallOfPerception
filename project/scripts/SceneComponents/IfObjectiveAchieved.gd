extends Node
export var invert:bool
export var objective_id:String

func _ready():
	if not invert:
		if not Global.GetObjectivesManager().IsObjectiveAchieved(objective_id):
			queue_free()
	else:
		if Global.GetObjectivesManager().IsObjectiveAchieved(objective_id):
			queue_free()


	
