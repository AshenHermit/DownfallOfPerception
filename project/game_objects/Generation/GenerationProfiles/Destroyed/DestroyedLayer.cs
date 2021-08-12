using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class DestroyedLayer : GenerationProfile
    {
        float musicTimer = 60.0f;


        public DestroyedLayer()
        {

        }

        public override bool SceneIsValid()
        {
            return Global.Instance.CurrentSceneName == "Destroyed";
        }

        public Spatial SpawnPoint;

        public override VoxelGeneratorScript GetGeneratorScript()
        {
            return GeneratorScript;
        }

        public override void _Ready()
        {
            base._Ready();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (musicTimer > 0.0f)
            {
                musicTimer -= delta;
                if (musicTimer < 0.0f)
                {
                    Global.Instance.GetAudioManager().PlayMusic("res://resources/sounds/music/poison_tree.ogg", 20.0f);
                    musicTimer = 0.0f;
                }
            }
        }


        public override void ObjectiveAchieved(string objectiveId)
        {
            
        }

        async public override void ActionHappened(string actionId)
        {
            if(actionId == "Player_entered_RestartTrigger")
            {
                Global.Instance.GetPlayer().Velocity = Vector3.Zero;
                await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(0.1f), "timeout");
                if (!SceneIsValid()) return;
                Global.Instance.GetPlayer().GlobalTransform = SpawnPoint.GlobalTransform;
            }
        }

        public override void ProcessSurfacePoint(Transform transform)
        {
            
        }
    }
}
