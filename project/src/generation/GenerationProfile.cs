using Godot;
using System;

namespace Game
{
    /// <summary>
    /// In this game Geneneration profiles are used as Layer profiles and contain rough logic for events happening in layer and objects spawning on world surface.
    /// </summary>
    public class GenerationProfile : Resource
    {
        [Export]
        public VoxelGeneratorScript GeneratorScript;
        [Export]
        public int WaitAfterGenerated = 0;

        Godot.Collections.Dictionary<string, int> _objectsCounter = new Godot.Collections.Dictionary<string, int>();

        [Export]
        public string SoundBankId = "withering";

        public string GetSoundIdFromBank(string soundId)
        {
            return SoundBankId + "_" + soundId;
        }

        public virtual void _Ready()
        {
            Global.Instance.GetObjectivesManager().OnObjectiveAchieved += ObjectiveAchieved;
        }
        public virtual void Clear()
        {
            _objectsCounter = new Godot.Collections.Dictionary<string, int>();
        }
        public virtual void _Process(float delta)
        {

        }

        public virtual bool SceneIsValid()
        {
            return true;
        }

        public virtual VoxelGeneratorScript GetGeneratorScript()
        {
            return GeneratorScript;
        }

        /// <summary>
        /// When spawn point is given to GenerationManager, GenerationManager gives spawn point transform to GenerationProfile with this method
        /// </summary>
        /// <param name="transform"></param>
        public virtual void ProcessSurfacePoint(Transform transform)
        {

        }

        public virtual void ObjectiveAchieved(string objectiveId)
        {

        }

        public virtual void ActionHappened(string actionId)
        {

        }

        //TODO: this is not in use
        public virtual float GetVoxelValue(Vector3 pos)
        {
            return 0.0f;
        }

        public void InstantiateObject(string objectName, int maxCount)
        {
            if (_objectsCounter.ContainsKey(objectName)) _objectsCounter[objectName] = 0;
            if (_objectsCounter[objectName] < maxCount)
            {

            }
        }
    }
}
