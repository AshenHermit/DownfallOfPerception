extends Object

var this:Spatial

var animation_player:AnimationPlayer
var collision_shape:CollisionShape

var scenes_to_spawn:Array
var item_to_check_id = ""
var item_amount = -1
var can_spawn_grappling_gun = true
var use_timeout = false

var spawn_point:Spatial

var used = false
var spawned = false

var timeout = 3.0

var use_text

func _ready():
	animation_player = this.get_child(0).get_node("AnimationPlayer")
	collision_shape = this.get_child(1)
	spawn_point = this.get_node("./SpawnPoint")
	
	use_text = Global.Translate("use_text.OPEN")

func _on_use(invoker):
	if not used and not animation_player.is_playing():
		Global.GetAudioManager().PlaySoundAtPosition(load("res://resources/sounds/containers/container.ogg"), 
			this.global_transform.origin, this, "world");
		animation_player.play("open")
		collision_shape.disabled = true
		used = true
		broadcast_opening_this_container()
		
		if use_timeout:
			yield(this.get_tree().create_timer(timeout), "timeout")
			close()

func broadcast_opening_this_container():
	GlobalVarsStorage.last_opened_container_id = get_instance_id()
	
func can_close():
	return GlobalVarsStorage.last_opened_container_id != get_instance_id()

func close():
	if weakref(animation_player).get_ref()==null: return
	animation_player.play_backwards("open")
	used = false
	spawned = false

func spawn_item():
	spawned = true
	var scene:PackedScene = scenes_to_spawn[floor(randf()*scenes_to_spawn.size())]
	
	#TODO:KLUDGE: grapplng gun drop is story event
	if can_spawn_grappling_gun and not Global.GetObjectivesManager().IsObjectiveAchieved("found_grappling_gun"):
		scene = load("res://game_objects/Items/Weapons/GrapplingGunItem.tscn")
		Global.GetObjectivesManager().AchieveObjective("found_grappling_gun")
		
	var instance:RigidBody = scene.instance()
	if item_amount!=-1:
		instance.Amount = item_amount
	this.get_parent().add_child(instance)
	instance.global_transform.origin = spawn_point.global_transform.origin
	instance.apply_impulse(Vector3.ZERO, -spawn_point.global_transform.basis.z*10.0)

func _process(delta):
	if used:
		if not spawned:
			if animation_player.current_animation_position > 0.3:
				spawn_item()
		if spawned:
			if can_close():
				close()
	else:
		if collision_shape.disabled:
			if not animation_player.is_playing():
				collision_shape.disabled = false
