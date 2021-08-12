extends VoxelGeneratorScript

var noise:FastNoiseLite = FastNoiseLite.new()

func _init():
	noise.seed = floor(randf()*10000000)
	noise.noise_type = FastNoiseLite.TYPE_OPEN_SIMPLEX_2S
	noise.period = 30.0
	noise.fractal_octaves = 2
	noise.fractal_lacunarity = 0.001
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
				var world_pos = Vector3(origin.x+x, origin.y+y, origin.z+z)*2.0
				var sdf = noise.get_noise_3dv(world_pos)
				#var sdf = 0.0
				
				
				# pipe
				var columnDist = Vector2(59.0, 0.0).distance_to(Vector2(world_pos.x, world_pos.z))
				var dist = world_pos.distance_to(Vector3(40,-50,0))
				if columnDist < 8.0:
					sdf = -1.0
				if dist < 12.0:
					sdf = 0
				
				dist = world_pos.distance_to(Vector3(0,0,0))
				var aiDist = world_pos.distance_to(Vector3(-80,-80,45))
				if dist < 16.0:
					sdf = 0
					
				if aiDist < 12.0:
					sdf = 0
				if world_pos.distance_to(Vector3(-80,-80,55)) < 12.0:
					sdf = 0
				
				if dist > 100.0 and aiDist > 12.0 and world_pos.distance_to(Vector3(-80,-80,0)) > 40.0:
					if world_pos.distance_to(Vector3(-80,-80,55)) > 12.0:
						sdf = -1.0
					
				
				out_buffer.set_voxel_f(sdf, x, y, z, 1)
