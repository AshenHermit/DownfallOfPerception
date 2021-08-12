extends VoxelGeneratorScript

var noise:FastNoiseLite = FastNoiseLite.new()

func _init():
	noise.seed = floor(randf()*10000000)
	noise.noise_type = FastNoiseLite.TYPE_OPEN_SIMPLEX_2S
	noise.period = 25.0
	noise.fractal_octaves = 10
	noise.fractal_lacunarity = 0.0005
	#noise.octaves = 2
	#noise.persistence = 0.001

func _generate_block(out_buffer: VoxelBuffer, origin: Vector3, lod: int):
	# Draw a flat plane at origin.y == 0
	#if origin.y < 0:
	#	out_buffer.fill(1, channel)
	#	return

	var stride:int = 1 << lod
	var size:Vector3 = out_buffer.get_size()
	
	for z in range(0, size.z):
		for x in range(0, size.x):
			for y in range(0, size.z):
				var orig_world_pos = Vector3(origin.x+x, origin.y+y, origin.z+z)*2.0
				var sdf = 0.0 if orig_world_pos.y>-50.0 else noise.get_noise_3dv(orig_world_pos)
				#var sdf = 0.0
				
				# terminal
				var dist = orig_world_pos.distance_to(Vector3(120,-140+20,-90))
				if dist < 28.0:
					sdf = 0
				
				# cross
				var columnDist = Vector2(orig_world_pos.y, orig_world_pos.z).length()
				if(orig_world_pos.x < -130.0 or orig_world_pos.x > 130.0): sdf = -1.0
				if(orig_world_pos.z < -130.0 or orig_world_pos.z > 130.0): sdf = -1.0
				
				var elevator = Vector2(0.0, -130).distance_to(Vector2(orig_world_pos.x, orig_world_pos.z))
				if columnDist > 50.0:
					columnDist = Vector2(orig_world_pos.y, orig_world_pos.x).length()
					if columnDist > 50.0:
						columnDist = Vector2(-120.0, -90).distance_to(Vector2(orig_world_pos.y, orig_world_pos.z))
						if columnDist > 40.0:
							if elevator > 6.0:
								sdf = -1.0
				if sdf != -1:
					if orig_world_pos.y>-50.0:
						var world_pos = orig_world_pos
						if(world_pos.x>-40.0 and world_pos.x<40.0): world_pos = (world_pos/18.0).floor()*18.0
						else: world_pos = (world_pos/12.0).floor()*12.0
						sdf = noise.get_noise_3dv(world_pos if orig_world_pos.y>-50.0 else orig_world_pos)
							
				if elevator <= 10.0:
					sdf = 0.0
					
				if(orig_world_pos.y > 40.0 or orig_world_pos.y < -120.0): sdf = -1.0
				
				dist = orig_world_pos.distance_to(Vector3(0,0,0))
				if dist < 16.0:
					sdf = 0
					
				
				out_buffer.set_voxel_f(sdf, x, y, z, 1)
