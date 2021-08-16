using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class BeginingLayer : GenerationProfile
    {
        [Export]
        public VoxelGeneratorScript DestroyedLayerGenerationScript;

        public BeginingLayer()
        {

        }

        public override bool SceneIsValid()
        {
            return Global.Instance.CurrentSceneName == "Begining";
        }

        public Godot.Collections.Array<Fungus> Funguses = new Godot.Collections.Array<Fungus>();
        public Godot.Collections.Array<StationDoor> StationDoors = new Godot.Collections.Array<StationDoor>();
        public Godot.Collections.Array<StationDoor> TrapDoors = new Godot.Collections.Array<StationDoor>();

        public Teleporter BeginingTeleporter;
        public TumorKing TumorKingNpc;
        public Spatial KingUtilizerSpawnPoint;

        public override VoxelGeneratorScript GetGeneratorScript()
        {
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("layers_destroyed"))
                return DestroyedLayerGenerationScript;
            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("last_order_started"))
                return GeneratorScript;
            return null;
        }

        public override void Clear()
        {
            Funguses = new Godot.Collections.Array<Fungus>();
            StationDoors = new Godot.Collections.Array<StationDoor>();
            TrapDoors = new Godot.Collections.Array<StationDoor>();
        }

        async public override void _Ready()
        {
            base._Ready();

            Global.Instance.GetGenerationManager().GetVoxelTerrain().CollisionLayer = 0;
            Global.Instance.GetGenerationManager().GetVoxelTerrain().CollisionMask = 0;

            if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("begining_teleporter_activated") 
                || Global.Instance.GetObjectivesManager().IsObjectiveAchieved("layers_destroyed"))
                BeginingTeleporter.Disable();

            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("got_rifle"))
            {
                foreach (StationDoor door in StationDoors)
                {
                    door.Open();
                }
            }

            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("last_order_started"))
            {
                Global.Instance.GetAudioManager().PlayMusic("res://resources/sounds/music/fear.ogg", 0.0f);

                if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("tumor_king_killed")
                && !Global.Instance.GetObjectivesManager().IsObjectiveAchieved("layers_destroyed"))
                {
                    TumorKingNpc.SetMonologue("last_order");
                    TumorKingNpc.Talk();
                }
                if ((Global.Instance.GetObjectivesManager().IsObjectiveAchieved("tumor_king_killed")
                || Global.Instance.GetObjectivesManager().IsObjectiveAchieved("layers_destroyed")))
                {
                    TumorKingNpc.DieWithSound = false;
                    TumorKingNpc.SetHealth(0.0f);
                }
            }

            if (Global.Instance.GetObjectivesManager().IsObjectiveAchieved("layers_destroyed"))
            {
                foreach (Fungus fungus in Funguses)
                {
                    fungus.Die();
                }
                Global.Instance.GetAudioManager().PlayMusic("res://resources/sounds/music/poison_tree.ogg", 10.0f);
                
                SpawnHomeTeleport();
            }
            else
            {
                SpawnFakeTeleport();
            }
        }

        public void SpawnFakeTeleport()
        {
            Spatial instance = GD.Load<PackedScene>("res://game_objects/Layers/FakeTeleport.tscn").Instance<Spatial>();
            Global.Instance.CurrentSceneInstance.AddChild(instance);
            instance.GlobalTransform = Transform.Identity
                .Translated(Global.Instance.GetGenerationManager().GetVoxelTerrain().GlobalTransform.origin + 
                    new Vector3(228.0f, 0.0f, 0.0f));
            instance.RotateObjectLocal(Vector3.Up, Mathf.Pi / 2.0f);
        }
        public void SpawnHomeTeleport()
        {
            Spatial instance = GD.Load<PackedScene>("res://game_objects/Layers/HomeTeleporter.tscn").Instance<Spatial>();
            Global.Instance.CurrentSceneInstance.AddChild(instance);
            instance.GlobalTransform = Transform.Identity
                .Translated(Global.Instance.GetGenerationManager().GetVoxelTerrain().GlobalTransform.origin +
                    new Vector3(228.0f, 0.0f, 0.0f));
            instance.RotateObjectLocal(Vector3.Up, Mathf.Pi / 2.0f);
        }

        public override void ObjectiveAchieved(string objectiveId)
        {
            
        }

        async public override void ActionHappened(string actionId)
        {
            if(actionId == "item_picked_RifleItem")
            {
                Global.Instance.GetObjectivesManager().AchieveObjective("got_rifle");
                await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(0.2f), "timeout");
                if (!SceneIsValid()) return;

                foreach (StationDoor door in StationDoors)
                {
                    door.Open();
                }
            }
            if (actionId == "Player_entered_TrapArea")
            {
                if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("layers_destroyed"))
                    Global.Instance.GetPlayer().MakeCrazy();

                foreach (StationDoor door in TrapDoors)
                {
                    door.Close();
                    Global.Instance.GetGenerationManager().GetVoxelTerrain().CollisionLayer = 1 + 2;
                    Global.Instance.GetGenerationManager().GetVoxelTerrain().CollisionMask = 1 + 2;
                }
            }
            if (actionId == "Player_entered_EscapeTrigger")
            {
                if (!Global.Instance.GetObjectivesManager().IsObjectiveAchieved("player_escaped_order"))
                {
                    Global.Instance.GetPlayerCamera().StartShakingConstantly(0.1f);
                    Global.Instance.GetObjectivesManager().AchieveObjective("player_escaped_order");
                    AudioManager.NonSpatialAudioUnit sound = Global.Instance.GetAudioManager().PlayNonSpatialSound("res://resources/sounds/small_destruction.ogg");
                    sound.VolumeDb = -4.0f;
                    KingUtilizer enemy = GD.Load<PackedScene>("res://game_objects/Enemies/KingUtilizer/KingUtilizer.tscn").Instance<KingUtilizer>();
                    enemy.Speed = 1.0f;
                    Global.Instance.CurrentSceneInstance.AddChild(enemy);
                    enemy.GlobalTransform = KingUtilizerSpawnPoint.GlobalTransform;
                    enemy.SetHealth(99999999999.0f);
                }
            }
            if (actionId == "player_used_fake_teleport")
            {
                Global.Instance.GetPlayer().MakeNormal();
                await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(5.0f+GD.Randf()*5.0f), "timeout");
                if (!SceneIsValid()) return;
                Global.Instance.GetPlayerCamera().PlayCutscene("Suicide", true);
                await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(5.2f), "timeout");
                if (!SceneIsValid()) return;
                Global.Instance.GetPlayerCamera().SetActiveState(false);
                Global.Instance.LoadScene("res://scenes/MainMenu.tscn");
                Global.Instance.GetAudioManager().StopMusic(0.0f);
            }
        }

        public override void ProcessSurfacePoint(Transform transform)
        {
            if (SpawnContainer(0.001f, transform)) return;
        }

        public bool SpawnContainer(float chance, Transform transform)
        {
            if (GD.Randf() < chance)
            {
                ScriptedUsable instance = GD.Load<PackedScene>("res://game_objects/Containers/container_usable.tscn").Instance<ScriptedUsable>();
                if (GD.Randf() < 0.5f)
                {
                    instance.Exports = instance.Exports.Duplicate();
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
    }
}
