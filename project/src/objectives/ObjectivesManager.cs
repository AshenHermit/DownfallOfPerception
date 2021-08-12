using Godot;
using System;

namespace Game
{
    public class ObjectivesManager : Godot.Object
    {
        Godot.Collections.Dictionary<string, bool> objectives = new Godot.Collections.Dictionary<string, bool>();

        public delegate void AchievedObjectiveHandler(string objectiveId);
        public event AchievedObjectiveHandler OnObjectiveAchieved;

        public bool IsObjectiveAchieved(string objectiveId)
        {
            if (!objectives.ContainsKey(objectiveId)) return false;
            return objectives[objectiveId];
        }
        public void AchieveObjective(string objectiveId)
        {
            GDE.Print("objective achieved:", objectiveId);
            objectives[objectiveId] = true;
            if (OnObjectiveAchieved != null) OnObjectiveAchieved.Invoke(objectiveId);
        }

        public Godot.Collections.Dictionary ExportObjectives()
        {
            Godot.Collections.Dictionary dict = (Godot.Collections.Dictionary)objectives.Duplicate();
            
            return dict;
        }
        public void ImportObjectives(Godot.Collections.Dictionary dict)
        {
            objectives = new Godot.Collections.Dictionary<string, bool>(dict.Duplicate());
        }
    }
}
