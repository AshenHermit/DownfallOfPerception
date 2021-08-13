using Godot;
using Godot.Collections;
using System;
using Game.Utils;

namespace Game
{
    class PickableItem : Usable, IPickableItem
    {
        [Export]
        public string ItemId;
        [Export]
        public int Amount;
        [Export]
        public Godot.Collections.Dictionary Properties = new Godot.Collections.Dictionary();

        public override void Use(Node invoker)
        {
            Global.Instance.GetInventory().AddItem(this);
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/layers/withering/impact/pick_up.ogg", GlobalTransform.origin);
            if (Global.Instance.GetGenerationManager() != null) Global.Instance.GetGenerationManager().ActionHappened("item_picked_" + Filename.GetFileNameFromPath());
            QueueFree();
        }
        public override string GetUseText()
        {
            return Global.Translate("use_text.TAKE") + " " + Inventory.Item.GetLocalizedName(ItemId);
        }

        // IPickableItem interface implementation
        public int GetAmount() => Amount;
        public string GetItemId() => ItemId;
        public Dictionary<string, object> GetProperties() => new Dictionary<string, object>(Properties);
        public PackedScene GetItemScene()
        {
            return GD.Load<PackedScene>(this.Filename);
        }
        public Mesh GetItemMesh()
        {
            Godot.Collections.Array<MeshInstance> meshes = this.GetAllChildren<MeshInstance>();
            if (meshes.Count > 0)
            {
                return meshes[0].Mesh;
            }
            return null;
        }
        public Texture GetIcon()
        {
            if (Properties.Contains("icon")){
                return (Texture)Properties["icon"];
            }
            return Utils.ResourceUtils.GetIconOfSpatial(GetChild<Spatial>(0));
            //return Utils.ResourceUtils.GetIconOfMesh(GetItemMesh());
        }

        public void FitInventoryItem(Inventory.Item item)
        {
            ItemId = item.ID;
            Amount = item.Amount;
            Properties = (Dictionary)item.Properties.Duplicate();
            GD.Print(Properties);
        }

    }
}
