extends Spatial

export(int) var damage
export(NodePath) var animation_player_path
onready var animation_player:AnimationPlayer = get_node(animation_player_path)

var ended = false
var frozen = false

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func _on_placed():
	var pos = global_transform.origin
	global_transform.origin = Vector3.ZERO
	global_transform = global_transform.rotated(global_transform.basis.y, randf()*PI*2)
	global_transform.origin = pos
	pass

func play_attack_animation():
	animation_player.play("attack")

func push_body(body:Object):
	var velocity_key = ""
	if "linear_velocity" in body:
		velocity_key = "linear_velocity"
	if "velocity" in body:
		velocity_key = "velocity"
	if "Velocity" in body:
		velocity_key = "Velocity"
		
	if velocity_key!="":
		body.set(velocity_key, global_transform.basis.y*40.0)
		
		

func _on_Area_body_entered(body):
	if not frozen:
		if not ended:
			if body.has_method("TakeDamage"):
				var can = true
				if body.has_method("GetHealth"):
					can = body.GetHealth()>0
				if can:
					play_attack_animation()
					body.call("TakeDamage", damage)
					push_body(body)
					ended = true
	
