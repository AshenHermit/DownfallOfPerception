using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class ProxyLayer : GenerationProfile
    {

        public Spatial ScreenFade;

        public ProxyLayer()
        {

        }



        public override VoxelGeneratorScript GetGeneratorScript()
        {
            return null;
        }

        public override void _Ready()
        {
            
        }

        public override void ObjectiveAchieved(string objectiveId)
        {
            
        }

        public override void ActionHappened(string actionId)
        {
            //TODO: bad
            if(actionId == "going_to_scene_Withering" || actionId == "going_to_scene_Begining" || actionId == "going_to_scene_Memory Storage")
            {
                ScreenFade.GetParent().RemoveChild(ScreenFade);
                ScreenFade.Free();
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
