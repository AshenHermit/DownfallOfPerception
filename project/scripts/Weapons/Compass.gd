extends Object


# exports
var model_path:NodePath

var this:Spatial
var item_properties

var armature:Spatial
var text_container:Spatial
var text3d:Spatial

enum ModeEnum{NWES, CENTER, ROUTER_RADAR}
var modes_count = 3
var mode = 0

var rand_timer = 0.0
var noise = FastNoiseLite.new()

var has_router = false
var distance_to_router = 0.0
var radar_click_timer = 1.0
var rotation_y = 0.0

var click_sound = preload("res://resources/sounds/compass/radar_click.wav")

func _ready():
	noise.period = 0.1
	
	mode = item_properties.mode
	var animation_player:AnimationPlayer = this.get_node(model_path).get_node("AnimationPlayer")
	this.CanBeRotated = false
	animation_player.get_animation("working").loop = true
	animation_player.play("working")
	armature = this.get_node(model_path).get_node("Armature")
	text_container = this.get_node(model_path).get_node("text_container")
	text3d = text_container.get_node("Text3D")
	update_text()
	
func _process(delta):
	# nwes
	if mode == ModeEnum.NWES:
		armature.rotation.y = -this.global_transform.basis.get_euler().y
	
	# center
	elif mode == ModeEnum.CENTER:
		if can_find_center():
			armature.rotation.y = this.global_transform.looking_at(Vector3.ZERO, Vector3.UP).basis.get_euler().y - this.global_transform.basis.get_euler().y-PI/2
		else:
			rand_timer+=delta*0.1
			armature.rotation.y = -PI/2.0 + (noise.get_noise_2d(0.0, rand_timer)*0.8)
		
	# router radar
	elif mode==ModeEnum.ROUTER_RADAR:
		update_distance_to_router()
		rand_timer+=delta
		var factor = max(0.0, 2.0-distance_to_router*0.01)
		factor = (factor*factor*factor)/8.0
		if has_router:
			factor = 2.0
		rotation_y += factor*2.0*PI*delta
		armature.rotation.y = rotation_y
		radar_click_timer -= factor*delta
		if radar_click_timer<=0.0:
			radar_click_timer = 1.0
			Global.GetAudioManager().PlaySoundAtPosition(
				click_sound, this.global_transform.origin-this.global_transform.basis.z, this, "world")
			
	text_container.global_transform = armature.global_transform

func can_find_center():
	return (Global.CurrentSceneName == "Withering" or Global.CurrentSceneName == "Memory Storage")
	
func update_distance_to_router():
	var dist = 0.0
	if Global.CurrentSceneName == "Withering":
		dist = Global.GetGenerationManager().CurrentGenerationProfile.GetDistanceToNearestRouter(
			this.global_transform.origin
		)
	else:
		dist = 9.0+(noise.get_noise_2d(0.0, rand_timer)+1.0)*99.0
	
	distance_to_router = dist
	has_router = distance_to_router<10.0

func update_text():
	if mode==ModeEnum.NWES: text3d.SetText(Global.Translate("compass_info.nwes"))
	if mode==ModeEnum.CENTER:
		if can_find_center():
			text3d.SetText(Global.Translate("compass_info.center"))
		else:
			text3d.SetText(Global.Translate("compass_info.center")+"\n"+Global.Translate("compass_info.not_found"))
	if mode==ModeEnum.ROUTER_RADAR:
		text3d.SetText(Global.Translate("compass_info.router_radar"))

func _make_shot(primary):
	rotation_y = 0.0
	mode = int(mode+1) % int(modes_count)
	item_properties.mode = mode
	update_text()
	
