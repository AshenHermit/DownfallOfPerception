extends VoxelInstanceGenerator


func _process_surface_point(transform):
	Global.GetGenerationManager().ProcessSurfacePoint(transform)
