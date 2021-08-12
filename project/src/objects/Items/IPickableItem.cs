using Godot;
using System;

namespace Game
{
    public interface IPickableItem
    {
        string GetItemId();
        int GetAmount();
        Texture GetIcon();
        PackedScene GetItemScene();
        Godot.Collections.Dictionary<string, object> GetProperties();

        void FitInventoryItem(Inventory.Item item);
    }
}
