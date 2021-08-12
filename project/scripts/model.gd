extends Spatial

onready var animation_player:AnimationPlayer = get_node("./AnimationPlayer")
export var moving_animation_name="moving"
export var steps_sound_bank="creeping_enemy_step"
export(Array, float) var step_frames

func _ready():
	setup_step_keys()
	pass
	
func setup_step_keys():
	if moving_animation_name=="": return
	animation_player
	
	var anim = animation_player.get_animation(moving_animation_name)
	if anim.find_track("../"+name)!=-1: return
	var track_id = anim.add_track(Animation.TYPE_METHOD)
	print("add_track")
	anim.track_set_path(track_id, "../"+name)
	for frame in step_frames:
		anim.track_insert_key(track_id, frame, {"method": "play_sound_from_bank", "args": [steps_sound_bank]})

func play_sound_from_bank(id:String):
	Global.GetAudioManager().PlaySoundFromBankAtPosition(id, global_transform.origin, self, "world")
	pass
