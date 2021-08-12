using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class MemoryStorageLayer : GenerationProfile
    {
        [Export]
        public VoxelGeneratorScript ClosedLayerGeneratorScript;

        int _memoryCardsCount = 0;

        static int _maxTumorsCount = 3;
        int _tumorsCount = 0;
        int _diedTumorsCount = 0;

        Gate _gate;
        Spatial _terminal;

        public MemoryStorageLayer()
        {
            
        }

        public override bool SceneIsValid()
        {
            return Global.Instance.CurrentSceneName == "Memory Storage";
        }

        public override VoxelGeneratorScript GetGeneratorScript()
        {
            GDE.Print(Global.Instance.GetObjectivesManager().IsObjectiveAchieved("memory_storage_activated"));
            if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("memory_storage_activated"))
                return ClosedLayerGeneratorScript;
            return GeneratorScript;
        }

        public override void _Ready()
        {
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("memory_storage_activated"))
            {
                SpawnTerminal();
                SpawnGate();
            }
        }

        public void SpawnTerminal()
        {
            _terminal = GD.Load<PackedScene>("res://game_objects/Layers/MemoryStorage/Terminal/Terminal.tscn").Instance<Spatial>();
            Global.Instance.CurrentSceneInstance.AddChild(_terminal);
            _terminal.Translate(new Vector3(120.0f, -120.0f, -90.0f));
            _terminal.RotateObjectLocal(Vector3.Up, -Mathf.Pi / 2.0f);
        }

        public void SpawnGate()
        {
            _gate = GD.Load<PackedScene>("res://game_objects/Layers/MemoryStorage/Objectives/Gate/Gate.tscn").Instance<Gate>();
            Global.Instance.CurrentSceneInstance.AddChild(_gate);
            _gate.GlobalTransform = Transform.Identity.Translated(new Vector3(0.0f, -50.0f, -130.0f));
        }

        async public override void ActionHappened(string actionId)
        {
            if (actionId == "tumor_died")
            {
                _diedTumorsCount += 1;
                if (_diedTumorsCount >= _maxTumorsCount)
                {
                    
                }
            }

            if(actionId == "try_open_gate")
            {
                if (_diedTumorsCount >= _maxTumorsCount)
                {
                    _gate.Open();
                }
                else
                {
                    Global.Instance.PopupInfo(Global.Translate("gate.blocked_message"));
                    if(Global.Instance.DEBUG) _gate.Open();
                }
            }

            if(actionId == "layer_system_destroy_activated")
            {
                Global.Instance.GetAudioManager().PlayNonSpatialSound("res://resources/sounds/deep_rumble.ogg");
                Global.Instance.GetObjectivesManager().AchieveObjective("destroy_stage_1_passed");
            }
        }

        //TODO: too long method
        public override void ProcessSurfacePoint(Transform transform)
        {
            if (SpawnFungus(0.0025f, transform)) return;
            if (SpawnContainer(0.01f, transform)) return;
            if (SpawnLamp(0.0015f, transform)) return;


            if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("memory_storage_activated"))
            {
                if (transform.origin.y > -41.0f)
                {
                    if (_memoryCardsCount < 2) {
                        if (SpawnMemoryCard(0.01f, transform))
                        {
                            _memoryCardsCount += 1; return;
                        }
                    }
                }
            }
            else
            {
                if (transform.origin.y > -51.0f)
                {
                    // distance from elevator 
                    // TODO: why shoud I comment this? not cool, generation like this should be more mobile, will try to describe them in some special files and write an editor for this.
                    // Also voxel generation scripts gave me conniptions.
                    if (transform.origin.z > -130+10)
                    {
                        if (SpawnTurret(0.035f, transform)) return;

                        if (_tumorsCount < _maxTumorsCount)
                        {
                            if(SpawnTumor(0.001f, transform))
                            {
                                _tumorsCount += 1; return;
                            }
                        }
                    }
                }
                else
                {
                    if (SpawnRope(0.04f, transform)) return;
                }
            }
        }

        public bool SpawnTumor(float chance, Transform transform)
        {
            if (GD.Randf() < chance)
            {
                Spatial instance = GD.Load<PackedScene>("res://game_objects/Enemies/Tumor.tscn").Instance<Spatial>();
                Global.Instance.CurrentSceneInstance.AddChild(instance);
                GDE.Print(transform);
                instance.Transform = transform * instance.Transform;
                return true;
            }
            return false;
        }
        public bool SpawnFungus(float chance, Transform transform)
        {
            if (Math.Abs(transform.basis.y.y) < 0.1f && GD.Randf() < chance)
            {
                //y
                if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                    transform.origin + transform.basis.y * 0.2f,
                    transform.origin + transform.basis.y * 0.2f + Vector3.Up*9.0f, null, 2))
                {
                    Spatial instance = GD.Load<PackedScene>("res://game_objects/Layers/MemoryStorage/Fungus/Fungus.tscn").Instance<Spatial>();
                    Global.Instance.CurrentSceneInstance.AddChild(instance);
                    //instance.RotateObjectLocal(Vector3.Up, Mathf.Pi);
                    instance.RotateObjectLocal(Vector3.Right, -Mathf.Pi / 2.0f);
                    instance.Transform = transform * instance.Transform;
                    instance.LookAt(instance.GlobalTransform.origin + transform.basis.y * 1.0f, Vector3.Up);
                    return true;
                }
            }
            return false;
        }
        public bool SpawnTurret(float chance, Transform transform)
        {
            if (Math.Abs(transform.basis.y.y) < 0.1f && GD.Randf() < chance)
            {
                //y
                if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                    transform.origin + transform.basis.y * 0.1f,
                    transform.origin + transform.basis.y * 2.0f, null, 2))
                {
                    Spatial instance = GD.Load<PackedScene>("res://game_objects/Enemies/Turret/Turret.tscn").Instance<Spatial>();
                    Global.Instance.CurrentSceneInstance.AddChild(instance);
                    //instance.RotateObjectLocal(Vector3.Up, (GD.Randf()-0.5f) * 2.0f * Mathf.Pi * 0.2f);
                    instance.RotateObjectLocal(Vector3.Right, -Mathf.Pi / 2.0f);
                    instance.Transform = transform * instance.Transform;
                    return true;
                }
            }
            return false;
        }
        public bool SpawnRope(float chance, Transform transform)
        {
            if (GD.Randf() < chance)
            {
                Spatial instance = GD.Load<PackedScene>("res://game_objects/Layers/MemoryStorage/TumorRope.tscn").Instance<Spatial>();
                Global.Instance.CurrentSceneInstance.AddChild(instance);
                instance.Transform = transform * instance.Transform;
                return true;
            }
            return false;
        }
        public bool SpawnMemoryCard(float chance, Transform transform)
        {
            if (transform.basis.y.y < -0.9f && GD.Randf() < chance)
            {
                //y
                if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                    transform.origin + Vector3.Down * 0.2f,
                    transform.origin + Vector3.Down * 8.0f, null, 1))
                {
                    //x
                    if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                        transform.origin + Vector3.Down * 0.5f - Vector3.Right * 2.5f,
                        transform.origin + Vector3.Down * 0.5f + Vector3.Right * 2.5f, null, 1))
                    {
                        //z
                        if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                           transform.origin + Vector3.Down * 0.5f - Vector3.Forward * 2.5f,
                           transform.origin + Vector3.Down * 0.5f + Vector3.Forward * 2.5f, null, 1))
                        {
                            Spatial instance = GD.Load<PackedScene>("res://game_objects/Layers/MemoryStorage/Objectives/MemoryCard/MemoryCardSocket.tscn").Instance<Spatial>();
                            Global.Instance.CurrentSceneInstance.AddChild(instance);
                            instance.RotateObjectLocal(Vector3.Up, GD.Randf() * Mathf.Pi * 2);
                            //instance.RotateObjectLocal(Vector3.Right, Mathf.Pi);
                            transform.basis = Basis.Identity;
                            instance.Transform = transform * instance.Transform;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool SpawnContainer(float chance, Transform transform)
        {
            if (transform.basis.y.y > 0.98f && GD.Randf() < chance)
            {
                TumorContainer instance = GD.Load<PackedScene>("res://game_objects/Layers/MemoryStorage/CrateContainer.tscn").Instance<TumorContainer>();
                Global.Instance.CurrentSceneInstance.AddChild(instance);
                instance.Transform = transform * instance.Transform;
                return true;
            }
            return false;
        }
        public bool SpawnLamp(float chance, Transform transform)
        {
            if (GD.Randf() < chance)
            {
                Spatial instance = GD.Load<PackedScene>("res://game_objects/Layers/MemoryStorage/StickLamp.tscn").Instance<Spatial>();
                Global.Instance.CurrentSceneInstance.AddChild(instance);
                instance.Transform = transform * instance.Transform;
                return true;
            }
            return false;
        }
    }
}
