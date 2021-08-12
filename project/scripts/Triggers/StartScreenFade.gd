extends Spatial

var has_player = false
onready var collision_shape:CollisionShape = get_node("./CollisionShape")
var length

func _ready():
	var box:BoxShape = collision_shape.shape
	length = max(box.extents.x, max(box.extents.y, box.extents.z))*2.0

func _on_ScreenFade_body_entered(body):
	if body == Global.GetPlayer():
		has_player = true
		
func _process(delta):
	if has_player:
		var amount = global_transform.origin.distance_to(Global.GetPlayer().global_transform.origin)/length
		Global.GetPlayerCamera().Shake(max(0.0, (amount-0.15)*2.0))
		Global.GetUIManager().SetScreenFadeAmount(amount)

func _on_ScreenFade_body_exited(body):
	if body == Global.GetPlayer():
		has_player = false
		Global.GetUIManager().SetScreenFadeAmount(0.0)
