using Godot;
using System;

namespace Game.UI
{
    public class ItemPopupDriver : Container
    {
        [Export]
        public PackedScene ItemPopupScene;

        public override void _Ready()
        {
            Global.Instance.GetInventory().OnItemAddEvent += OnItemManipulation;
            Global.Instance.GetInventory().OnItemRemoveEvent += OnItemManipulation;
        }
        public override void _Process(float delta)
        {
            MarginTop = 0;
        }

        public void OnItemManipulation(Game.Inventory.Item item, int amountChange)
        {
            StartItemPopup(item, amountChange);
        }

        public void StartItemPopup(Game.Inventory.Item item, int amountChange)
        {
            ItemPopup itemPopup = ItemPopupScene.Instance<ItemPopup>();
            AddChild(itemPopup);
            string message = "";
            if (amountChange > 0) message = Global.Translate("message.added_item");
            if (amountChange < 0) message = Global.Translate("message.removed_item");
            itemPopup.Start(message, item, Math.Abs(amountChange));
        }
    }
}
