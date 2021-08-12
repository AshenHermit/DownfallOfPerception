extends Spatial

var this:Spatial
var item_properties

#properties

onready var animation_player:AnimationPlayer = this.get_node("./Model/AnimationPlayer")
onready var particles:Particles = this.get_node("./Particles")
onready var particles_burst:Particles = this.get_node("./ParticlesBurst")
onready var hand:Spatial = this.get_parent().get_parent()
onready var hand_default_transform:Transform = hand.transform
onready var ray_cast:RayCast = this.get_node("./RayCast")
onready var player:KinematicBody = Global.GetPlayer()
onready var sound_player:AudioStreamPlayer3D = this.get_node("./Sound")

var scan_sound_player:AudioStreamPlayer3D

var scan_sound:AudioStream = preload("res://resources/sounds/weapons/grappling_gun/grappling_gun_scan.mp3")
var grab_sound:AudioStream = preload("res://resources/sounds/weapons/grappling_gun/grappling_gun_grab.mp3")
var release_sound:AudioStream = preload("res://resources/sounds/weapons/grappling_gun/grappling_gun_release.mp3")

var hook_point:Vector3
var hook_local_point:Vector3
var hooked_body:Spatial
var hooked_body_wr:WeakRef
var is_hooked = false
var distance = 0.0
var distance_target

var move_along_ray_speed = 90.0

onready var last_player_pos = player.global_transform.origin
onready var player_pos_diff:Vector3

func _ready():
	sound_player.unit_db = -80.0
	sound_player.play()
	particles.emitting = false
	animation_player.get_animation("init").loop = true
	if not this.OneShot: animation_player.get_animation("fire").loop = true
	animation_player.play("init")
	ray_cast.add_exception(player)

func _process(delta):
	if is_hooked:
		if not hooked_body_wr.get_ref():
			this.StopFiring()
			return
			
		sound_player.unit_db += (-6-sound_player.unit_db)/2.0
			
		hook_point = hooked_body.to_global(hook_local_point)
		distance += (distance_target-distance)/6.0
		
		#hand.global_transform = hand.global_transform.looking_at(hook_point, Vector3.UP)
		hand.look_at(hook_point, Vector3.UP)
		this.transform = Transform(Basis.IDENTITY, Vector3.ZERO)
		if ray_cast.is_colliding() and ray_cast.get_collision_point().distance_to(this.global_transform.origin)<hook_point.distance_to(this.global_transform.origin)-4.0:
			hook_point = ray_cast.get_collision_point()
			set_hooked_body(ray_cast.get_collider())
			distance_target = hook_point.distance_to(player.global_transform.origin)
			distance = distance_target
			
		var dir:Vector3 = player.global_transform.origin-hook_point
		var norm = dir.normalized()
		if dir.length() >= distance:
			if hooked_body.has_method("Detach") and player.Velocity.y<0.0 and player.Velocity.length()>18.0: hooked_body.Detach()
			player.global_transform.origin = hook_point + norm * distance
			player.Velocity.y *= min(norm.dot(Vector3.UP)/2+0.5+0.5, 1)
			player.Velocity.x += -norm.x*1.0
			player.Velocity.z += -norm.z*1.0
			
		if Input.is_action_pressed("jump"):
			distance_target = min(distance, hook_point.distance_to(player.global_transform.origin))
			distance = distance_target
			distance_target = max(6.0, distance_target-move_along_ray_speed*delta)
			sound_player.unit_db += (12-sound_player.unit_db)/2.0
		elif Input.is_action_pressed("crouch"):
			distance_target = min(distance, hook_point.distance_to(player.global_transform.origin))
			distance = distance_target
			distance_target += move_along_ray_speed*delta
			sound_player.unit_db += (12-sound_player.unit_db)/2.0
			
	else:
		sound_player.unit_db += (-80-sound_player.unit_db)/100.0
	
	player_pos_diff = player.global_transform.origin-last_player_pos
	last_player_pos = player.global_transform.origin
			
func hook():
	if this!=null:
		if Input.is_action_pressed("fire") and ray_cast.is_colliding():
			hook_point = ray_cast.get_collision_point()
			set_hooked_body(ray_cast.get_collider())
			particles.emitting = true
			is_hooked = true
			distance_target = hook_point.distance_to(player.global_transform.origin)
			distance = distance_target
			this.CanBeRotated = false
			animation_player.play("fire")
			
			var sound:AudioStreamPlayer3D = Global.GetAudioManager().PlaySoundAtPosition(grab_sound, sound_player.global_transform.origin, this, "world")

func set_hooked_body(body:Node):
	hooked_body = body
	hooked_body_wr = weakref(body)
	hook_local_point = hooked_body.to_local(hook_point)

func _make_shot(primary):
	if this!=null:
		#particles_burst.process_material.direction = this.transform.basis.z
		particles_burst.emitting = true
		
		scan_sound_player = Global.GetAudioManager().PlaySoundAtPosition(scan_sound, sound_player.global_transform.origin, this, "world")
		scan_sound_player.max_db = 15.0
		
		if ray_cast.is_colliding():
			hook_point = ray_cast.get_collision_point()
			var wait = hook_point.distance_to(this.global_transform.origin)/80.0
			
			yield(this.get_tree().create_timer(wait), "timeout")
			if weakref(scan_sound_player).get_ref()!=null: scan_sound_player.stop()
			if this!=null: hook()

func _on_started_firing(primary):
	pass


func _on_stopped_firing():
	particles.emitting = false
	hand.transform = hand_default_transform
	if is_hooked:
		animation_player.play("stop_firing")
		var add = player_pos_diff*40.0
		if add.y>0: player.Velocity += Vector3(0.0, add.y, 0.0)
		Global.GetAudioManager().PlaySoundAtPosition(release_sound, sound_player.global_transform.origin, this, "world")
	is_hooked = false
	this.CanBeRotated = true
