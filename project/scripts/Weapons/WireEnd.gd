extends Spatial

var this:Spatial
var item_properties

#properties

onready var particles:Particles = this.get_node("./Particles")
onready var hand:Spatial = this.get_parent().get_parent()
onready var ray_cast:RayCast = this.get_node("./RayCast")
onready var player:KinematicBody = Global.GetPlayer()

var wire

func _ready():
	particles.emitting = false

func _process(delta):
	if wire!=null:
		wire.SetEndPoint(this.MuzzlePoint.global_transform.origin)
		if ray_cast.is_colliding():
			var collider = ray_cast.get_collider()
			if collider.has_method("highlight") and collider.has_method("connect_wire"):
				if collider.wire_id == wire.GetId():
					collider.highlight()
					if Input.is_action_just_pressed("use"):
						this.StopFiring()
			
	pass

func _make_shot(primary):
	if ray_cast.is_colliding():
		var collider = ray_cast.get_collider()
		if collider.has_method("RequestWire"):
			particles.emitting = true
			var tmpwire = collider.RequestWire()
			
			if tmpwire.ConnectedInput != null:
				#wire.Disconnect()
				pass
			elif tmpwire.IsFolded and tmpwire.ConnectedInput == null:
				#this.CanBeRotated = false
				this.transform = Transform.IDENTITY
				wire = tmpwire
				wire.Unfold()


func _on_started_firing(primary):
	pass


func _on_stopped_firing():
	this.CanBeRotated = true
	particles.emitting = false
	if wire!=null:
		var fold_wire = true
		if ray_cast.is_colliding():
			var collider = ray_cast.get_collider()
			if collider.has_method("connect_wire"):
				if collider.wire_id == wire.GetId():
					collider.connect_wire(wire)
					wire.Connect(collider)
					fold_wire = false
					Global.GetInventory().TakeItem("wire_end_weapon", 1)
				
		if fold_wire: wire.Fold()
		wire = null
