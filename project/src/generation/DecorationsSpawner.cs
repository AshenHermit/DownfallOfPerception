using Godot;
using System;

namespace Game 
{
    public class DecorationsSpawner : Node
    {
        [Export]
        public NodePath MeshInstancePath;
        MeshInstance _meshInstance;
        Godot.Collections.Array<MultiMeshInstance> _multiMeshInstances = new Godot.Collections.Array<MultiMeshInstance>();
        Godot.Collections.Array<MultiMesh> _multiMeshes = new Godot.Collections.Array<MultiMesh>();

        [Export]
        public Godot.Collections.Array<Godot.Collections.Dictionary<string, object>> Decorations = 
            new Godot.Collections.Array<Godot.Collections.Dictionary<string, object>>();

        [Export]
        public bool Enabled = true;


        public override void _Ready()
        {
            if (!MeshInstancePath.IsEmpty()) _meshInstance = GetNode<MeshInstance>(MeshInstancePath);
            SetupMultimeshes();
            if(Enabled) SpawnDecorations();
        }
        void SetupMultimeshes()
        {
            for(int i=0; i<Decorations.Count; ++i)
            {
                if (!Decorations[i].ContainsKey("mesh") || !Decorations[i].ContainsKey("material")) { 
                    GD.PrintErr("mesh or material is not set"); AddMultiMeshInstance(null, null); continue; 
                }

                if (!Decorations[i].ContainsKey("density")) Decorations[i]["density"] = 0.5f;
                AddMultiMeshInstance((Mesh)Decorations[i]["mesh"], (Material)Decorations[i]["material"]);
            }
        }

        public void AddMultiMeshInstance(Mesh mesh, Material material)
        {
            if(mesh==null || material == null)
            {
                _multiMeshes.Add(null);
                _multiMeshInstances.Add(null);
                return;
            }

            MultiMesh multiMesh = new MultiMesh();
            multiMesh.Mesh = mesh;
            multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
            multiMesh.VisibleInstanceCount = 0;
            multiMesh.InstanceCount = 10000;
            _multiMeshes.Add(multiMesh);
            MultiMeshInstance multiMeshInstance  = new MultiMeshInstance();
            multiMeshInstance.MaterialOverride = material;
            multiMeshInstance.Multimesh = multiMesh;
            _multiMeshInstances.Add(multiMeshInstance);

            _meshInstance.CallDeferred("add_child", multiMeshInstance);
        }

        public void SpawnDecorations()
        {
            if (_meshInstance == null) return;
            MeshDataTool meshDataTool = new MeshDataTool();
            ArrayMesh mesh = new ArrayMesh();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _meshInstance.Mesh.SurfaceGetArrays(0));
            meshDataTool.CreateFromSurface(mesh, 0);

            for (int f = 0; f < meshDataTool.GetFaceCount(); ++f)
            {
                int vertId = meshDataTool.GetFaceVertex(f, 0);
                Transform t = Transform.Identity;
                Vector3 pos = meshDataTool.GetVertex(vertId);

                Vector3 normal = meshDataTool.GetFaceNormal(f);
                Plane plane = new Plane(normal, 0.0f);
                Vector3 dir = new Vector3(GD.Randf() - 0.5f, GD.Randf() - 0.5f, GD.Randf() - 0.5f);
                dir = (plane.Project(dir)).Normalized();
                t.origin = Vector3.Zero;
                t = t.LookingAt(normal, dir);
                t = t.Rotated(t.basis.x, -Mathf.Pi / 2.0f);
                t.origin = pos;

                for (int i = 0; i < _multiMeshInstances.Count; ++i)
                {
                    if (_multiMeshInstances[i] != null)
                    {
                        if (GD.Randf() < (float)Decorations[i]["density"])
                        {
                            if (_multiMeshes[i].VisibleInstanceCount > _multiMeshes[i].InstanceCount) continue;

                            _multiMeshes[i].SetInstanceTransform(_multiMeshes[i].VisibleInstanceCount, t);
                            _multiMeshes[i].VisibleInstanceCount += 1;
                        }
                    }
                }
            }
        }
    }
}
