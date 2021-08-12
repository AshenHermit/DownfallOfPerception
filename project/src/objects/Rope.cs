using Godot;
using System;
using Game.Utils;


namespace Game
{
    public class Rope : Spatial
    {
        [Export]
        public PackedScene RopeEndScene;
        [Export]
        public NodePath RopeMeshPath;
        Spatial _ropeMesh;
        [Export]
        public Godot.Collections.Array<Godot.Collections.Dictionary> ScenesToSpawn;

        public bool Enabled = true;

        bool _stretched = false;
        float _length;

        public override void _Ready()
        {
            _ropeMesh = GetNode<Spatial>(RopeMeshPath);
        }

        public override void _Process(float delta)
        {
            if (!_stretched && Global.Instance.GetGenerationManager().Generated)
            {
                if (Global.Instance.GetPlayer().GlobalTransform.origin.DistanceTo(GlobalTransform.origin) < 50.0f)
                {
                    if(Enabled) Stretch();
                }
            }
        }

        public void Stretch()
        {
            _stretched = true;
            Godot.Collections.Dictionary collision = Global.Instance.GetDirectSpaceState().IntersectRay(
                GlobalTransform.origin + GlobalTransform.basis.y * 0.5f,
                GlobalTransform.origin + GlobalTransform.basis.y * 500.0f,
                null, 2 // world_surface
            );

            if (collision.Count <= 0) return;

            Vector3 pos = collision.Get<Vector3>("position");
            _length = GlobalTransform.origin.DistanceTo(pos);
            _ropeMesh.Scale = new Vector3(1.0f, _length, 1.0f);
            Spatial instance = RopeEndScene.Instance<Spatial>();
            AddChild(instance);
            instance.SetRotationAlongNormal(
                pos,
                collision.Get<Vector3>("normal"),
                GlobalTransform.basis.y);
            SpawnScenes();
        }

        public void SpawnScenes()
        {
            if (ScenesToSpawn == null) return;
            if (ScenesToSpawn.Count <= 0) return;

            foreach (Godot.Collections.Dictionary sceneDict in ScenesToSpawn)
            {
                if (!sceneDict.Contains("chance") || !sceneDict.Contains("scene")) continue;

                int count = 1;
                if(sceneDict.Contains("count")) count = sceneDict.GetInt("count");
                count = (int)Mathf.Round((float)count*(_length/20.0f));

                for (int i=0; i<count; ++i)
                {
                    if (GD.Randf() < sceneDict.GetFloat("chance"))
                    {
                        Spatial instance = sceneDict.Get<PackedScene>("scene").Instance<Spatial>();
                        AddChild(instance);
                        Vector3 pos = GlobalTransform.origin + GlobalTransform.basis.y * GD.Randf()*_length;
                        instance.SetRotationAlongNormal(pos, GlobalTransform.basis.z, GlobalTransform.basis.y);
                        instance.RotateObjectLocal(Vector3.Forward, GD.Randf() * Mathf.Pi * 2.0f);
                    }
                }
            }
        }
    }
}
