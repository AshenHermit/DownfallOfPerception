using Godot;
using System;

namespace Game.UI
{
    class QuickInventory : Container
    {
        [Export]
        public PackedScene UIItemPackedScene;

        [Export]
        public NodePath Inventory;

        [Export] 
        public int ItemsCount;

        bool _scroll_warp = false ;

        public override void _Ready()
        {
            base._Ready();

            Global.Instance.GetInventory().OnItemsArrayChanged += OnInventoryUpdate;
            Global.Instance.GetInventory().OnActiveItemChange += OnActiveItemChange;
            RenderItems();
        }

        public override void _Input(InputEvent e)
        {
            if (Global.Instance.GetPlayer() == null) return;
            if (!Global.Instance.GetPlayer().IsAlive()) return;
            if (Global.Instance.GetPlayer().Disabled) return;

            if (Global.Instance.GetUIManager().GetCurrentOverlay() == OverlayType.NONE) {
                if (e is InputEventMouseButton)
                {
                    InputEventMouseButton mbuttonEvent = (InputEventMouseButton)e;
                    if (mbuttonEvent.Pressed)
                    {
                        if (mbuttonEvent.ButtonIndex == (int)ButtonList.WheelUp || mbuttonEvent.ButtonIndex == (int)ButtonList.WheelDown)
                        {
                            int previousActiveItemIndex = Global.Instance.GetInventory().GetActiveItemIndex();
                            int nextActiveItemIndex = Global.Instance.GetInventory().GetActiveItemIndex();

                            if (mbuttonEvent.ButtonIndex == (int)ButtonList.WheelUp) { nextActiveItemIndex -= 1;  }
                            else if (mbuttonEvent.ButtonIndex == (int)ButtonList.WheelDown) { nextActiveItemIndex += 1; }

                            if (nextActiveItemIndex < 0)
                            {
                                if(_scroll_warp) nextActiveItemIndex = ItemsCount - 1;
                                else nextActiveItemIndex = 0;
                            }
                            if (nextActiveItemIndex >= ItemsCount)
                            {
                                if (_scroll_warp) nextActiveItemIndex = 0;
                                else nextActiveItemIndex = ItemsCount - 1;
                            }

                            if(nextActiveItemIndex!=previousActiveItemIndex) 
                                Global.Instance.GetInventory().SetActiveItemIndex(nextActiveItemIndex);
                        }
                    }
                }
                if (e is InputEventKey)
                {
                    InputEventKey keyEvent = (InputEventKey)e;
                    if(keyEvent.Scancode >= (int)KeyList.Key1 && keyEvent.Scancode < (int)KeyList.Key1 + 6)
                    {
                        Global.Instance.GetInventory().SetActiveItemIndex((int)keyEvent.Scancode-(int)KeyList.Key1);
                    }
                }
            }
        }

        public void OnActiveItemChange()
        {
            RenderItems();
        }
        public void OnInventoryUpdate()
        {
            RenderItems();
        }

        public void RenderItems()
        {
            foreach(Node child in GetChildren())
            {
                child.QueueFree();
                RemoveChild(child);
            }
            int clippedItemsCount = Math.Min(Global.Instance.GetInventory().GetSize(), ItemsCount);
            for (int i=0; i < clippedItemsCount; ++i)
            {
                UI.Item uiItem = UIItemPackedScene.Instance<UI.Item>();
                Game.Inventory.Item item = Global.Instance.GetInventory().GetItemByIndex(i);
                AddChild(uiItem);
                uiItem.Setup(i, item);

                Color modulateColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                if (Global.Instance.GetInventory().GetActiveItemIndex() == i)
                {
                    modulateColor.a = 1.0f;
                    uiItem.SetSelectionState(true);
                }
                else
                {
                    modulateColor.a = 0.8f;
                    uiItem.SetSelectionState(false);
                }
                uiItem.Modulate = modulateColor;
            }
        }
    }
}
