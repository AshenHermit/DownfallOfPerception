extends Spatial

export(int) var id:int = 0
export(Vector3) var local_velocity:Vector3 = Vector3.ZERO
export(String) var cutscene_name:String = ""
export(bool) var loading_screen:bool = false
export(float) var camera_shake_amount:float = 0.0
export(bool) var came_from_teleporter = false

func get_global_velocity():
	return global_transform.basis.x * local_velocity.x + global_transform.basis.y * local_velocity.y + global_transform.basis.z * local_velocity.z

func _ready():
	call_deferred("spawn")

func spawn():
	if Global.SpawnPointId == id:
		if came_from_teleporter:
			Global.GetAudioManager().PlaySoundAtPosition(load("res://resources/sounds/teleporter/teleporter_turn_off.mp3"), 
				global_transform.origin, self, "world");
		
		if loading_screen:
			Global.GetPlayer().Disable()
			Global.GetLoadingScreen().Show()
		Global.GetPlayer().global_transform = global_transform
		Global.GetPlayer().Velocity = get_global_velocity()
		Global.GetGenerationManager().GetVoxelViewer().global_transform = global_transform
		if cutscene_name!="":
			Global.GetPlayerCamera().PlayCutscene(cutscene_name, false)
		if camera_shake_amount>0.0:
			Global.GetPlayerCamera().Shake(camera_shake_amount)

