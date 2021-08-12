extends Object

enum StateEnum {IDLE, WANDERING, CHASING_TARGET, ATTACKING}

var this:KinematicBody

var velocity:Vector3
var movement:Vector3

var noise_timer = 0.1
var noise_movement_target:Vector3
var noise_movement:Vector3

# base methods
func _physics_process(delta):
	velocity = this.move_and_slide(velocity)
	velocity += movement
	velocity /= 2.0
	movement = Vector3.ZERO

func _process(delta):
	if this.GetHealth()>0:
		if noise_timer>0.0:
			noise_timer -= delta
		else:
			noise_timer = 0.5
			noise_movement_target = (Vector3(randf(), randf(), randf())-Vector3.ONE/2)*2*800.0
			
		noise_movement += (noise_movement_target-noise_movement)/40.0
		
		movement += noise_movement
		
		#this.global_transform.basis = Basis(-movement.normalized())
		this.global_transform = this.global_transform.looking_at(Global.GetPlayer().global_transform.origin, Vector3.UP)
	else:
		velocity.y -= 500
	
func _on_died():
	pass

# methods
func chase_player():
	var dif = Global.GetPlayer().global_transform.origin-this.global_transform.origin
	movement += dif.normalized()*400.0
	movement -= dif.normalized()*(max(8-dif.length(),0))*200.0


# state methods
func _idle_state(delta):
	this.SetState(StateEnum.CHASING_TARGET)
	
func _wandering_state(delta):
	pass

func _chasing_target_state(delta):
	chase_player()

func _attacking_state(delta):
	pass

	
