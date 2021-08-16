using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class ProxyLayer : GenerationProfile
    {
        public Area ScreenFade;
        public Spatial ProxySystem;

        public ProxyLayer()
        {

        }

        public override Godot.Collections.Dictionary<string, object> GetDefaultUserData()
        {
            return new Godot.Collections.Dictionary<string, object> {
                { "current_layer_index", 0 }
            };
        }

        public override VoxelGeneratorScript GetGeneratorScript()
        {
            return null;
        }

        public override void _Ready()
        {
            base._Ready();

            if (Global.Instance.SpawnPointId != -1)
            {
                UserData["current_layer_index"] = Global.Instance.SpawnPointId;
            }
            ProxySystem.Call("setup");
        }


        public override void ObjectiveAchieved(string objectiveId)
        {
            
        }

        public override void ActionHappened(string actionId)
        {
            //TODO: bad
            if(actionId == "going_to_scene_Withering" || actionId == "going_to_scene_Begining" || actionId == "going_to_scene_Memory Storage")
            {
                ScreenFade.Monitoring = false;
                //ScreenFade.GetParent().RemoveChild(ScreenFade);
                //ScreenFade.Free();
            }
            if(actionId == "going_to_scene_Begining")
            {
                Global.Instance.GetObjectivesManager().AchieveObjective("begining_teleporter_activated");
            }
        }

        public override void ProcessSurfacePoint(Transform transform)
        {
            
        }
    }
}
