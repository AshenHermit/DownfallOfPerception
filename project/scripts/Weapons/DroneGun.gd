extends Object

var this
var item_properties

#properties
var damage = 0

onready var particles:Particles = this.get_node("./NoiseMovement/Particles")
onready var area:Area = this.get_node("./NoiseMovement/Area")
onready var light:SpotLight = this.get_node("./NoiseMovement/Light")
onready var noise_movement:Spatial = this.get_node("./NoiseMovement")
onready var sound:AudioStreamPlayer3D = this.get_node("./Sound")
var start_firing_sound:AudioStream = preload("res://resources/sounds/weapons/drone_gun/drone_gun_start_firing.wav")
var stop_firing_sound:AudioStream = preload("res://resources/sounds/weapons/drone_gun/drone_gun_stop_firing.wav")

var bodies_in_area



func _ready():
	sound.unit_db = -80
	sound.play()
	particles.process_material = particles.process_material.duplicate()
	if item_properties != null:
		damage = item_properties["damage"] as float
	particles.emitting = false
	noise_movement.Strength = 0.4
	noise_movement.Speed = 0

func _process(delta):
	light.light_energy /= 1.2
	if this.IsFiring():
		sound.unit_db += (1-sound.unit_db)/15.0
		if can_do_fire(): 
			noise_movement.Speed += (10.0-noise_movement.Speed)/2.0
			particles.emitting = true
			light.light_energy += 1.0
		
		if bodies_in_area!=null:
			for body in bodies_in_area:
				if(weakref(body).get_ref()):
					if body!=Global.GetPlayer():
						if "Velocity" in body:
							body.Velocity += -this.global_transform.basis.z*4.0
	else:
		noise_movement.Speed += (0-noise_movement.Speed)/8.0
		sound.unit_db += (-80-sound.unit_db)/15.0
func can_do_fire():
	return sound.unit_db>-30
	
func _make_shot(primary):
	particles.process_material.gravity = -this.global_transform.basis.z*80
	
	if can_do_fire():
		bodies_in_area = area.get_overlapping_bodies()
		for body in bodies_in_area:
			# TODO: this is stupid as fuck
			if body.name != "Player":
				if body.has_method("TakeDamage"):
					var totalDmg = damage + max(0, 1.0 - (body.global_transform.origin.distance_to(this.global_transform.origin)/10.0))
					body.TakeDamage(totalDmg)
	
func _on_started_firing(primary):
	Global.GetAudioManager().PlaySoundAtPosition(start_firing_sound, this.global_transform.origin, this, "world")

func _on_stopped_firing():
	Global.GetAudioManager().PlaySoundAtPosition(stop_firing_sound, this.global_transform.origin, this, "world")
	particles.emitting = false
