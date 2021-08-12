using Godot;
using System;
using Game.Utils;

namespace Game{
    public class Teleporter : ConsumingMachine
    {
        [Export]
        public string SceneName;
        [Export]
        public int SpawnPointId = 0;

        [Export]
        public bool CanDamage = true;

        public override void _Ready()
        {
            base._Ready();

            ItemsCount = NeedItemsCount.Duplicate();
            if (CanDamage) Damage();
            UpdateWorkingState();
            UpdateInfo();
        }

        public void Damage()
        {
            foreach (string itemId in ItemsCount.Keys)
            {
                if (!NeedItemsCount.ContainsKey(itemId)) continue;
                if (GD.Randf() < 0.99f)
                {
                    ItemsCount[itemId] = Math.Max(0, ItemsCount[itemId]-(int)Math.Floor(
                        ((GD.Randf()*0.8f+0.2f)*(float)ItemsCount[itemId])));
                }
            }
        }

        async public override void WorkingUse()
        {
            Global.Instance.GetPlayerCamera().StartShakingConstantly(0.1f);
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/teleporter/teleporter.mp3", GlobalTransform.origin, this);
            await ToSignal(Global.Instance.CurrentSceneInstance.GetTree().CreateTimer(4.0f), "timeout");
            //TODO: extract this pattern from other places to one method
            Global.Instance.GetGenerationManager().ActionHappened("teleporter_used");
            Global.Instance.LoadScene("res://scenes/" + SceneName + ".tscn", SpawnPointId);
        }
    }
}

