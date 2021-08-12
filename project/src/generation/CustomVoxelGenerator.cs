using Godot;
using System;

namespace Game { 
    public class CustomVoxelGenerator : VoxelGeneratorScript
    {
        FastNoiseLite noise = new FastNoiseLite();

        public CustomVoxelGenerator()
        {
            noise.NoiseType = FastNoiseLite.NoiseTypeEnum.OpenSimplex2s;
            noise.Period = 30.0f;
            noise.FractalOctaves = 2;
            noise.FractalLacunarity = 0.001f;
        }

        public override void _GenerateBlock(VoxelBuffer outBuffer, Vector3 originInVoxels, int lod)
        {
            int stride = 1 << lod;
            Vector3 size = outBuffer.GetSize();
            for(int z=0; z<size.z; ++z) 
            {
                for (int x = 0; x < size.x; ++x)
                {
                    for (int y = 0; y < size.y; ++y)
                    {
                        Vector3 worldPos = new Vector3(originInVoxels.x + x, originInVoxels.y + y, originInVoxels.z + z);
                        float sdf = noise.GetNoise3dv(worldPos);
                        float dist = worldPos.DistanceTo(Vector3.Zero);
                        if(dist < 16.0){
                            sdf = 0.0f;
                        }
                        if (dist > 100.0){
                            sdf = -1.0f;
                        }
                        outBuffer.SetVoxelF(sdf, x, y, z, 1);
                    }
                }
            }
        }
    }
}

