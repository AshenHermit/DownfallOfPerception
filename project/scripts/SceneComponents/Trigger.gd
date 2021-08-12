extends Node


func _on_Area_body_entered(body):
	if Global.GetGenerationManager()==null: return
	Global.GetGenerationManager().ActionHappened(body.name+"_entered_"+name)


func _on_Area_body_exited(body):
	if Global.GetGenerationManager()==null: return
	Global.GetGenerationManager().ActionHappened(body.name+"_exited_"+name)
