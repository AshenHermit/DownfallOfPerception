extends Object

# exports
var model_path:NodePath
var ray_cast:RayCast

var this:Spatial
var item_properties

var text_node:Spatial

var modes_count = 2

var router_digger = null
var calibrated = false

func _ready():
	text_node = this.get_node("Text3D").RequestText3D()
	ray_cast = this.get_node("RayCast")
	update_text()
	
func _process(delta):
	if router_digger!=null:
		var result = router_digger.Calibrate(Global.GetPlayerCamera().GetMouseMotion().x)
		if result: 
			calibrated = true
			router_digger = null
			item_properties.remaining_firmware -= 1
			update_text()

func update_text():
	if text_node==null: return
	
	if router_digger!=null:
		if not calibrated:
			text_node.SetText(Global.Translate("router_controller.connection_established"))
		else:
			text_node.SetText(Global.Translate("router_controller.calibrated"))
	else:
		text_node.SetText(Global.Translate("router_controller.remaining_firmware")+" "+str(item_properties.remaining_firmware))
		

func _make_shot(primary):
	if ray_cast.is_colliding():
		var body = ray_cast.get_collider()
		if body.has_method("Calibrate"):
			if body.IsWorking() and not body.IsCalibrated():
				router_digger = ray_cast.get_collider()
				Global.GetPlayerCamera().SetRotationAbility(false)
				update_text()
			
func _on_stopped_firing():
	calibrated = false
	router_digger = null
	update_text()
	Global.GetPlayerCamera().SetRotationAbility(true)
		
		
	
