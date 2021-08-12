using Godot;
using Game.Utils;
using System;

namespace Game
{
    public class EnemyCorpse : RigidBody, IUsable
    {
        Godot.Collections.Dictionary _itemToDropDict = null;
        float _timer = 40.0f;
        public bool CanBeDisassembled = true;

        public override void _Ready()
        {
            
        }
        public override void _Process(float delta)
        {
            _timer -= delta;
            if (_timer <= 0.0f)
            {
                QueueFree();
            }
        }

        public void SetItemToDrop(Godot.Collections.Dictionary itemDict)
        {
            if (itemDict == null) return;
            if (!itemDict.Contains("scene")) return;
            if (!itemDict.Contains("chance")) return;

            _itemToDropDict = itemDict.Duplicate();
        }

        public void DropItem()
        {
            if (_itemToDropDict == null) return;

            // chance
            if (GD.Randf() >= _itemToDropDict.GetFloat("chance")) return;

            PickableItem item = _itemToDropDict.Get<PackedScene>("scene").Instance<PickableItem>();
            if (_itemToDropDict.Contains("amount"))
            {
                item.Amount = 1 + (int)Mathf.Max(0.0f, Mathf.Floor(GD.Randf() * _itemToDropDict.GetInt("amount")-1));
            }
            Global.Instance.CurrentSceneInstance.AddChild(item);
            item.GlobalTransform = GlobalTransform;
        }

        public bool IsUsable()
        {
            return true;
        }

        public void SetUsableState(bool usable)
        {
            return;
        }

        public void Use(Node invoker)
        {
            DropItem();
            QueueFree();
        }

        public string GetUseText()
        {
            if (!CanBeDisassembled) return "";
            return Global.Translate("use_text.DISASSEMBLE");
        }

        public Spatial GetUseInfoPoint()
        {
            return this;
        }
    }
}
