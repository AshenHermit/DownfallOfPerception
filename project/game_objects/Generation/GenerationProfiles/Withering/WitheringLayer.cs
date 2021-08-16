using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class WitheringLayer : GenerationProfile
    {
        [Export]
        public VoxelGeneratorScript DestroyedLayerGenerationScript;

        [Export]
        public PackedScene ContainerScene;

        [Export]
        public PackedScene CommutatorScene;
        [Export]
        public PackedScene SourceMachineScene;

        FastNoiseLite _noise = new FastNoiseLite();

        bool commutatorGenerated = false;
        Spatial commutator;
        bool sourceMachineGenerated = false;

        Godot.Collections.Array<EnemySpawner> enemySpawners = new Godot.Collections.Array<EnemySpawner>();
        Godot.Collections.Array<Rope> ropes = new Godot.Collections.Array<Rope>();

        Godot.Collections.Array<Vector3> _routersPositions = new Godot.Collections.Array<Vector3>();

        AI aiInstance;
        public DestroyControl AIDestroyControl;

        float _kingUtilizersTimer = 2.0f;
        int _remainingKingUtilizersCount = 5;

        public WitheringLayer()
        {
            _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.OpenSimplex2s;
            _noise.Period = 30.0f;
            _noise.FractalOctaves = 2;
            _noise.FractalLacunarity = 0.001f;
        }
        public override bool SceneIsValid()
        {
            return Global.Instance.CurrentSceneName == "Withering";
        }

        public override VoxelGeneratorScript GetGeneratorScript()
        {
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("destroy_stage_1_passed"))
                return DestroyedLayerGenerationScript;
            return base.GetGeneratorScript();
        }

        public override void _Ready()
        {
            enemySpawners = new Godot.Collections.Array<EnemySpawner>();
            ropes = new Godot.Collections.Array<Rope>();

            base._Ready();
            CreateSpawnPipe();
            SpawnAI();

            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("player_escaped_order"))
            {
                _remainingKingUtilizersCount = 6 + (int)Mathf.Floor(GD.Randf() * 8.0f);
            }
            else
            {
                _remainingKingUtilizersCount = 0;
            }
        }
        public override void _Process(float delta)
        {
            base._Process(delta);
            SpawnKingUtilizers(delta);
        }

        void SpawnKingUtilizers(float delta)
        {
            if (_remainingKingUtilizersCount > 0 && Global.Instance.GetGenerationManager().Generated)
            {
                _kingUtilizersTimer -= delta;
                if (_kingUtilizersTimer < 0.0f)
                {
                    KingUtilizer enemy = GD.Load<PackedScene>("res://game_objects/Enemies/KingUtilizer/KingUtilizer.tscn").Instance<KingUtilizer>();
                    Global.Instance.CurrentSceneInstance.AddChild(enemy);
                    enemy.GlobalTransform = Transform.Identity.Translated(MathUtils.Randv()*(30.0f+GD.Randf()*60.0f));
                    _kingUtilizersTimer = 6.0f;
                    _remainingKingUtilizersCount -= 1;
                }
            }
        }

        void CreateSpawnPipe()
        {
            StaticBody instance = GD.Load<PackedScene>("res://game_objects/Layers/Withering/TrashPipe.tscn").Instance<StaticBody>();
            Global.Instance.CurrentSceneInstance.AddChild(instance);
            Transform trans = instance.GlobalTransform;
            trans = trans.Rotated(Vector3.Up, Mathf.Pi / 2.0f);
            trans.origin = new Vector3(51.0f, -50.0f, 0.0f);
            instance.GlobalTransform = trans;
        }
        void SpawnAI()
        {
            aiInstance = GD.Load<PackedScene>("res://game_objects/Layers/Withering/AI/AI.tscn").Instance<AI>();

            aiInstance.SetWiresOutputsCount(3);
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_1_wire_connected"))
                aiInstance.SetWiresOutputsCount(2);
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_2_wire_connected"))
                aiInstance.SetWiresOutputsCount(1);
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_3_wire_connected"))
                aiInstance.SetWiresOutputsCount(0);

            Global.Instance.CurrentSceneInstance.AddChild(aiInstance);
            Transform trans = aiInstance.GlobalTransform;
            trans = trans.Rotated(Vector3.Up, Mathf.Pi);
            trans.origin = new Vector3(-80.0f, -80.0f + 8.0f, 45.0f+14.0f);
            aiInstance.GlobalTransform = trans;
            CallDeferred("UpdateAi");
        }

        //TODO: too long method
        void UpdateAi()
        {
            aiInstance.SetMonologue("");
            if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("withering_power_activated"))
            {
                aiInstance.Discharge();
                aiInstance.SetMonologue("lack_of_power");
            }
            else
            {
                aiInstance.Charge();
            }
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("withering_power_activated") &&
                !Global.Instance.GetObjectivesManager().IsObjectiveAchieved("talked_about_looks_horrible"))
            {
                aiInstance.SetMonologue("looks_horrible");
            }
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("ai_got_memory_card"))
            {
                aiInstance.SetMonologue("need_router_calibration");
                aiInstance.SetMonologuePartIndex(5);
            }
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("unfolded_wires"))
            {
                aiInstance.Unfold();
                aiInstance.SetMonologue("");
            }
            if(Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_1_wire_connected"))
            {
                aiInstance.TalkWhenPlayerArrived();
                aiInstance.SetMonologue("ill_1");
            }
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_2_wire_connected"))
            {
                aiInstance.MakeSick();
                aiInstance.TalkWhenPlayerArrived();
                aiInstance.SetMonologue("ill_2");
            }
            //TODO: what if i will need more than 3 routers?
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_3_wire_connected"))
            {
                aiInstance.MakeSick();
                aiInstance.SetMonologue("");
            }

            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("destroy_stage_1_passed"))
            {
                aiInstance.MakeSick();
                aiInstance.Discharge(false);
                aiInstance.TalkWhenPlayerArrived();
                aiInstance.npcId = "tumor_king";
                aiInstance.SetMonologue("come_with_me");
            }

            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("last_order_started"))
            {
                aiInstance.MakeSick();
                aiInstance.Discharge(false);
                aiInstance.SetMonologue("");
                AIDestroyControl.MakeReadyToDestroy();
            }
        }

        public override void ObjectiveAchieved(string objectiveId)
        {
            if (objectiveId == "withering_power_activated")
            {
                foreach(EnemySpawner enemySpawner in enemySpawners)
                {
                    enemySpawner.Enabled = true;
                }
                foreach (Rope rope in ropes)
                {
                    rope.Enabled = true;
                }
                aiInstance.SetMonologue("looks_horrible");
                aiInstance.Charge();
            }
        }

        //TODO: too long method
        async public override void ActionHappened(string actionId)
        {
            if(actionId == "teleporter_used")
            {
                Global.Instance.GetObjectivesManager().AchieveObjective("booth_opened");
            }

            if (actionId == "source_machine_connected")
            {
                commutator.GetNode<AudioStreamPlayer3D>("Sound").Play();
                Global.Instance.GetObjectivesManager().AchieveObjective("withering_power_activated");
            }
            if (actionId == "router_digger_calibrated")
            {
                //TODO: this is stupid
                if(!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_1_calibrated")) 
                    Global.Instance.GetObjectivesManager().AchieveObjective("router_1_calibrated");
                else if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_2_calibrated"))
                    Global.Instance.GetObjectivesManager().AchieveObjective("router_2_calibrated");
                else if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_3_calibrated"))
                    Global.Instance.GetObjectivesManager().AchieveObjective("router_3_calibrated");
            }
            if (actionId == "router_wire_connected")
            {
                //TODO: this is stupid
                if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_1_wire_connected"))
                {
                    Global.Instance.GetObjectivesManager().AchieveObjective("router_1_wire_connected");
                    UpdateAi();
                }
                else if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_2_wire_connected"))
                {
                    Global.Instance.GetObjectivesManager().AchieveObjective("router_2_wire_connected");
                    UpdateAi();
                }
                else if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("router_3_wire_connected"))
                {
                    Global.Instance.GetObjectivesManager().AchieveObjective("router_3_wire_connected");
                    Global.Instance.GetObjectivesManager().AchieveObjective("memory_storage_activated");
                    Global.Instance.GetAudioManager().PlayNonSpatialSound("res://resources/sounds/rumble.ogg");

                    UpdateAi();
                }
            }
            if (actionId == "router_digger_correctly_placed")
            {
                //if (_routersPositions.Count > 0) _routersPositions.RemoveAt(0);
            }
            if (actionId == "ai_unfold_wires")
            {
                aiInstance.Unfold();
            }
            if (actionId == "ai_activate_memory_storage")
            {
                
            }
            if (actionId == "ai_got_MemoryCardItem")
            {
                Global.Instance.GetObjectivesManager().AchieveObjective("ai_god_memory_card");
                if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("talked_about_looks_horrible"))
                {
                    if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("unfolded_wires"))
                    {
                        aiInstance.SetMonologue("need_router_calibration");
                        aiInstance.Talk();
                        aiInstance.TakeBodyInArea();
                    }
                }
            }
            if (actionId == "start_last_order")
            {
                Global.Instance.GetAudioManager().PlayMusic("res://resources/sounds/music/fear.ogg", 0.0f);
                Global.Instance.GetObjectivesManager().AchieveObjective("last_order_started");
                Global.Instance.GetPlayerCamera().PlayCutscene("Interruption");
                await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(0.05f), "timeout");
                //TODO: this is bad solution for safe timeout
                if (!SceneIsValid()) return;
                //TODO: events in cutscene animation must happen using animation keys
                Global.Instance.GetPlayer().SetHealth(1.0f);
                await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(3.20f), "timeout");
                if (!SceneIsValid()) return;
                Global.Instance.LoadScene("res://scenes/Begining.tscn", 2);
            }
            if (actionId == "destroy_activated")
            {
                Global.Instance.GetAudioManager().StopMusic(0.0f);
                Global.Instance.GetAudioManager().PlayNonSpatialSound("res://resources/sounds/destroy_layers.ogg");
                Global.Instance.GetObjectivesManager().AchieveObjective("layers_destroyed");
                Global.Instance.GetPlayerCamera().StartShakingConstantly(0.2f);
                await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(4.0f), "timeout");
                if (!SceneIsValid()) return;
                Global.Instance.GetGenerationManager().StopAmbience();
                Global.Instance.GetPlayerCamera().StartShakingConstantly(2.0f);
                await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(6.5f), "timeout");
                if (!SceneIsValid()) return;
                Global.Instance.GetUIManager().SetScreenFadeAmount(1.0f);
                Global.Instance.GetPlayer().Disable();
                await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(6.0f), "timeout");
                if (!SceneIsValid()) return;

                Global.Instance.LoadScene("res://scenes/Destroyed.tscn", 0);
                
            }
        }

        public override void ProcessSurfacePoint(Transform transform)
        {
            // basic
            if (SpawnLight(0.002f, transform)) return;
            if (SpawnRope(0.001f, transform)) return;
            if (SpawnContainer(0.002f, transform)) return;

            // power activation
            if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("withering_power_activated"))
            {
                if (SpawnCommutator(0.04f, transform)) return;
                if (SpawnSourceMachine(0.04f, transform)) return;
            }

            // routers
            if (AddRouter(0.005f, 3, transform)) return;

            // enemy spawners
            if (SpawnFlyingSpawner(0.001f, transform)) return;
            if (SpawnCreepingSpawner(0.001f, transform)) return;
        }
        public bool AddRouter(float chance, int maxCount, Transform transform)
        {
            if(GD.Randf()<chance && _routersPositions.Count<maxCount && transform.basis.y.y > 0.9f && transform.origin.y>10.0f)
            {
                _routersPositions.Add(transform.origin);
                return true;
            }
            return false;
        }

        public bool SpawnRope(float chance, Transform transform)
        {
            if (GD.Randf() < chance)
            {
                Rope instance = GD.Load<PackedScene>("res://game_objects/Layers/Withering/Objectives/Fruits/Rope.tscn").Instance<Rope>();
                Global.Instance.CurrentSceneInstance.AddChild(instance);
                ropes.Add(instance);
                instance.Enabled = false;
                if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("withering_power_activated")) instance.Enabled = true;

                //instance.RotateObjectLocal(Vector3.Right, -Mathf.Pi / 2.0f);
                instance.Transform = transform * instance.Transform;
                return true;
            }
            return false;
        }
        public bool SpawnLight(float chance, Transform transform)
        {
            if (transform.basis.y.y < -0.7f && GD.Randf() < chance)
            {
                //y
                if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                    transform.origin + Vector3.Down * 0.5f,
                    transform.origin + Vector3.Down * 14.0f, null, 1))
                {
                    //x
                    if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                        transform.origin + Vector3.Down * 12.0f - Vector3.Right * 1.0f,
                        transform.origin + Vector3.Down * 12.0f + Vector3.Right * 1.0f, null, 1))
                    {
                        //z
                        if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                           transform.origin + Vector3.Down * 12.0f - Vector3.Forward * 1.0f,
                           transform.origin + Vector3.Down * 12.0f + Vector3.Forward * 1.0f, null, 1))
                        {
                            Spatial instance = GD.Load<PackedScene>("res://game_objects/Layers/Withering/Decorations/Jar.tscn").Instance<Spatial>();
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
            if (GD.Randf() < chance)
            {
                ScriptedUsable instance = ContainerScene.Instance<ScriptedUsable>();
                instance.Exports = instance.Exports.Duplicate();
                instance.Exports["timeout"] = 0.8f+GD.Randf()*4.0f;
                if (GD.Randf() < 0.5f)
                {
                    instance.Exports["scenes_to_spawn"] = new Godot.Collections.Array {
                        GD.Load<PackedScene>("res://game_objects/Items/Building/stairs_item.tscn"),
                        GD.Load<PackedScene>("res://game_objects/Items/Food/lasagna.tscn")
                    };
                }
                Global.Instance.CurrentSceneInstance.AddChild(instance);
                instance.RotateObjectLocal(Vector3.Right, Mathf.Pi / 2.0f);
                instance.Transform = transform * instance.Transform;
                return true;
            }
            return false;
        }

        // commutation
        public bool SpawnCommutator(float chance, Transform transform)
        {
            if (transform.origin.y < -70.0f && transform.basis.y.y > 0.8f && !commutatorGenerated && GD.Randf() < chance)
            {
                if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                    transform.origin + transform.basis.y * 0.1f,
                    transform.origin + transform.basis.y * 8.0f,
                    null, 1))
                {
                    if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                        transform.origin + transform.basis.y * 8.0f - transform.basis.x * 4.5f,
                        transform.origin + transform.basis.y * 8.0f + transform.basis.x * 4.5f,
                        null, 1))
                    {
                        GDE.Print("commutator");
                        GDE.Print(transform);
                        Spatial instance = CommutatorScene.Instance<Spatial>();
                        commutator = instance;
                        Global.Instance.CurrentSceneInstance.AddChild(instance);
                        instance.Transform = transform * instance.Transform;
                        commutatorGenerated = true;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool SpawnSourceMachine(float chance, Transform transform)
        {
            if (transform.origin.y > 70.0f && Mathf.Abs(transform.basis.y.y) < 0.5f && !sourceMachineGenerated && GD.Randf() < chance)
            {
                GDE.Print("source machine");
                GDE.Print(transform);
                Spatial instance = SourceMachineScene.Instance<Spatial>();
                Global.Instance.CurrentSceneInstance.AddChild(instance);
                instance.RotateObjectLocal(Vector3.Right, Mathf.Pi / 2.0f);
                instance.Transform = transform * instance.Transform;
                sourceMachineGenerated = true;
                return true;
            }
            return false;
        }

        // enemy spawners
        public bool SpawnFlyingSpawner(float chance, Transform transform)
        {
            if (transform.basis.y.y < -0.6f && GD.Randf() < chance)
            {
                if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                    transform.origin + transform.basis.y * 0.1f, transform.origin + transform.basis.y * 11.0f, null, 1))
                {
                    string res = "res://game_objects/Enemies/Spawners/FlyingSpawner.tscn";
                    EnemySpawner instance = GD.Load<PackedScene>(res).Instance<EnemySpawner>();
                    instance.Enabled = false;

                    // if objective achieved: withering_power_activated
                    if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("withering_power_activated")
                        && !Global.Instance.GetObjectivesManager().IsObjectiveAchieved("last_order_started"))
                        instance.Enabled = true;
                    enemySpawners.Add(instance);

                    Global.Instance.CurrentSceneInstance.AddChild(instance);
                    instance.Transform = transform * instance.Transform;
                    return true;
                }
                else
                {

                }
            }
            return false;
        }
        public bool SpawnCreepingSpawner(float chance, Transform transform)
        {
            if (transform.basis.y.y > 0.0f && GD.Randf() < chance)
            {
                if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                    transform.origin + transform.basis.y * 0.1f,
                    transform.origin + transform.basis.y * 4.0f, null, 1))
                {
                    if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                        transform.origin + transform.basis.y * 2.0f - transform.basis.x * 4.0f,
                        transform.origin + transform.basis.y * 2.0f + transform.basis.x * 4.0f, null, 1))
                    {
                        if (!Global.Instance.GetDirectSpaceState().RayIsIntersecting(
                        transform.origin + transform.basis.y * 2.0f - transform.basis.z * 4.0f,
                        transform.origin + transform.basis.y * 2.0f + transform.basis.z * 4.0f, null, 1))
                        {
                            string res = "res://game_objects/Enemies/Spawners/CreepingSpawner.tscn";
                            EnemySpawner instance = GD.Load<PackedScene>(res).Instance<EnemySpawner>();
                            instance.Enabled = false;

                            // if objective achieved: withering_power_activated
                            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("withering_power_activated")
                                && !Global.Instance.GetObjectivesManager().IsObjectiveAchieved("last_order_started")) 
                                instance.Enabled = true;
                            enemySpawners.Add(instance);

                            Global.Instance.CurrentSceneInstance.AddChild(instance);
                            instance.Transform = transform * instance.Transform;
                            return true;
                        }
                    }
                }
            }
            return false;
        }




        // router methods
        public bool HasRouterAtPosition(Vector3 position)
        {
            return GetDistanceToNearestRouter(position) < 10.0f;
        }
        public int GetNearestRouterIndex(Vector3 searchPosition)
        {
            if (_routersPositions.Count == 0) return -1;

            float minDist = -1.0f;
            int nearestIndex = -1;
            for (int i = 0; i < _routersPositions.Count; ++i)
            {
                float dist = searchPosition.DistanceTo(_routersPositions[i]);
                if (dist < minDist || minDist == -1.0f) {
                    minDist = dist;
                    nearestIndex = i;
                }
            }
            return nearestIndex;
        }
        public float GetDistanceToNearestRouter(Vector3 searchPosition)
        {
            int nearestIndex = GetNearestRouterIndex(searchPosition);
            if (nearestIndex == -1) return 9999.0f;

            return searchPosition.DistanceTo(_routersPositions[nearestIndex]);
        }

        public void RemoveRouterAtPosition(Vector3 searchPosition)
        {
            int nearestIndex = GetNearestRouterIndex(searchPosition);
            if (nearestIndex == -1) return;

            _routersPositions.RemoveAt(nearestIndex);
        }


        //TODO: this is not in use
        public override float GetVoxelValue(Vector3 pos)
        {
            float value = _noise.GetNoise3dv(pos);
            float dist = pos.DistanceTo(new Vector3(0, 0, 0));

            if (dist < 16.0){
                value = 0.0f;
            }
            if (dist > 100.0)
            {
                value = -1.0f;
            }

            return value;
        }
    }
}
