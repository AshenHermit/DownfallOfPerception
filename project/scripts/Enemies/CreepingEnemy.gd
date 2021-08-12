extends Object

enum StateEnum {IDLE, WANDERING, CHASING_TARGET, ATTACKING}

var this:KinematicBody

var velocity:Vector3
var movement:Vector3

var noise_timer = 0.1
var noise_movement_target:Vector3
var noise_movement:Vector3

onready var up_direction = this.global_transform.basis.y
onready var ray_cast:RayCast = this.get_node("./RayCast")

# base methods
func _physics_process(delta):
	velocity += movement*10.0
	velocity = this.move_and_slide(velocity, up_direction, true)
	velocity -= up_direction*3.0
	#velocity += (this.global_transform.basis.x * movement.x + this.global_transform.basis.z * movement.y).normalized()
	velocity /= 2.0
	movement = Vector3.ZERO

func _process(delta):
	if this.IsAlive():
		if noise_timer>0.0:
			noise_timer -= delta
		else:
			noise_timer = 0.5
			noise_movement_target = (Vector3(randf(), randf(), randf())-Vector3.ONE/2)*2*800.0
			
		noise_movement += (noise_movement_target-noise_movement)/40.0
		
		if ray_cast.is_colliding():
			up_direction = ray_cast.get_collision_normal()
			
			var trans = this.global_transform

			trans.origin = Vector3.ZERO
			#TODO: move this in some utiliy to use in other places
			trans = trans.looking_at(trans.origin + up_direction, this.global_transform.basis.z)
			trans = trans.rotated(trans.basis.x, -PI / 2.0)
			trans.origin = ray_cast.get_collision_point() + up_direction*1.2+velocity*0.1
			this.global_transform = trans

	else:
		velocity.y -= 500

func _on_died():
	pass

# methods
func chase_player():
	#movement += (Global.GetPlayer().global_transform.origin-this.global_transform.origin).normalized()*400.0
	movement = this.global_transform.basis.z*0.1
	pass


# state methods
func _idle_state(delta):
	this.SetState(StateEnum.CHASING_TARGET)
	
func _wandering_state(delta):
	pass

func _chasing_target_state(delta):
	chase_player()

func _attacking_state(delta):
	pass

	
