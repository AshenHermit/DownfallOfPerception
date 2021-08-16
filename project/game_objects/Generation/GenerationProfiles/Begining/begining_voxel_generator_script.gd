extends VoxelGeneratorScript

var noise:FastNoiseLite = FastNoiseLite.new()

func _init():
	setup()

func setup():
	noise.seed = floor(randf()*10000000)
	noise.noise_type = FastNoiseLite.TYPE_OPEN_SIMPLEX_2S
	noise.period = 18.0
	noise.fractal_octaves = 2
	noise.fractal_lacunarity = 0.02
	#noise.octaves = 2
	#noise.persistence = 0.001

func _generate_block(out_buffer: VoxelBuffer, origin: Vector3, lod: int):
	var stride:int = 1 << lod
	var size:Vector3 = out_buffer.get_size()
	
	for z in range(0, size.z):
		for x in range(0, size.x):
			for y in range(0, size.z):
				var world_pos = Vector3(origin.x+x, origin.y+y, origin.z+z)*2.0
				var sdf = noise.get_noise_3dv(world_pos)
				
				var columnDist = Vector2(world_pos.y, world_pos.z).length()
				if(world_pos.x < -5.0 or world_pos.x > 200.0): sdf = -1.0
				
				if columnDist>30.0:
					sdf = -1.0
				
				var dist = world_pos.distance_to(Vector3(0,0,0))
				if dist < 25.0:
					sdf = 0
					
				# teleporter
				dist = world_pos.distance_to(Vector3(200.0,0,0))
				if dist < 25.0:
					sdf = 0
				if world_pos.distance_to(Vector3(230.0,0,0)) < 15.0:
					sdf = 0
				
				out_buffer.set_voxel_f(sdf, x, y, z, 1)
