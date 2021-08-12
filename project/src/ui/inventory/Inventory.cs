using Godot;
using System;

namespace Game.UI
{
    public class Inventory : Container
    {
        [Export]
        public PackedScene UIItemPackedScene;
        [Export]
        public NodePath ItemsContainerNodePath;
        private Container _itemsContainer;
        [Export]
        public NodePath NameLabelNodePath;
        private Label _nameLabel;
        [Export]
        public NodePath DescriptionLabelNodePath;
        private RichTextLabel _descriptionLabel;

        private int _draggingItemIndex = -1;
        private Game.Inventory.Item selectedItem=null;

        public override void _Ready()
        {
            _itemsContainer = GetNode<Container>(ItemsContainerNodePath);
            _nameLabel = GetNode<Label>(NameLabelNodePath);
            _descriptionLabel = GetNode<RichTextLabel>(DescriptionLabelNodePath);
            _descriptionLabel.BbcodeEnabled = true; 

            SelectItem(null);
            RenderItems();

            // TODO: wtf is wrong with this, need to standardize adding event listeners.
            // with field and operator or with method? method can be covered in interface, field is not 
            // (EDIT: actually can, with property), maybe just "event getOnChangeEvent() => ... ?"
            Global.Instance.GetInventory().OnItemsArrayChanged += OnInventoryUpdate;
            Global.Instance.GetUIManager().AddOverlayChangeListener(OnOverlayChanged);
        }
        public void OnInventoryUpdate()
        {
            RenderItems();
        }

        /// <summary>
        /// Removes all elements in items container and fills it again with empty item cells.
        /// </summary>
        public void RenderItems()
        {
            foreach (Node child in _itemsContainer.GetChildren())
            {
                child.QueueFree();
                _itemsContainer.RemoveChild(child);
            }

            int selectedItemIndex = -1;
            if (selectedItem != null)
            {
                selectedItemIndex = Global.Instance.GetInventory().GetItemIndex(selectedItem.ID);
            }
            for (int i = 0; i < Global.Instance.GetInventory().GetSize(); ++i)
            {
                UI.Item uiItem = UIItemPackedScene.Instance<UI.Item>();
                Game.Inventory.Item item = Global.Instance.GetInventory().GetItemByIndex(i);
                _itemsContainer.AddChild(uiItem);
                uiItem.SetUIInventory(this);
                uiItem.Setup(i, item);
                uiItem.SetDraggable(true);
                if (i == selectedItemIndex)
                    uiItem.SetSelectionState(true);
                else
                    uiItem.SetSelectionState(false);
                if (i == _draggingItemIndex)
                {
                    uiItem.HideInfo();
                }
            }
        }

        public void OnOverlayChanged(OverlayType overlay)
        {
            if(overlay != OverlayType.GAME_OVERLAY)
            {
                StopDraggingItem();
                RenderItems();
            }
        }
        public void OnGameOverlayGUIEvent(InputEvent e)
        {
            if(e is InputEventMouseButton)
            {
                InputEventMouseButton mbuttonEvent = (InputEventMouseButton)e;
                if (mbuttonEvent.ButtonIndex == (int)ButtonList.Left)
                {
                    if (IsDragging())
                    {
                        Game.Inventory.Item item = Global.Instance.GetInventory().GetItemByIndex(_draggingItemIndex);
                        Global.Instance.GetPlayer().DropItem(item, GetViewport().GetMousePosition());
                        StopDraggingItem();
                    }
                }
            }
        }

        public void SelectItem(Game.Inventory.Item item)
        {
            if (selectedItem == item) return;
            if (item != null)
            {
                selectedItem = item;
                _nameLabel.Text = Game.Inventory.Item.GetLocalizedName(item.ID) + " x" + item.Amount.ToString();
                _descriptionLabel.BbcodeText = Game.Inventory.Item.GetLocalizedDescription(item.ID);
            }
            else
            {
                selectedItem = null;
                _nameLabel.Text = "";
                _descriptionLabel.BbcodeText = "";
            }
            CallDeferred("RenderItems");
        }
        public void StartDraggingItem(UI.Item item)
        {
            item.HideInfo();
            Global.Instance.GetUIManager().SetIconUnderCursor(item.GetInventoryItem().Icon);
            _draggingItemIndex = item.GetItemIndex();
        }
        public void StartDraggingItemWithIndex(int itemIndex)
        {
            StartDraggingItem(_itemsContainer.GetChild<UI.Item>(itemIndex));
        }
        public void EndDraggingItem(UI.Item item)
        {
            if (item != null)
            {
                if (_draggingItemIndex == item.GetItemIndex())
                {
                    item.ShowInfo();
                    StopDraggingItem();
                }
                else
                {
                    Global.Instance.GetInventory().SwapItems(_draggingItemIndex, item.GetItemIndex());
                    item.ShowInfo();
                    if (item.HasItem())
                    {
                        CallDeferred("StartDraggingItemWithIndex", _draggingItemIndex);
                    }
                    else
                    {
                        StopDraggingItem();
                    }
                }
            }
            else
            {
                _itemsContainer.GetChild<Item>(_draggingItemIndex).ShowInfo();
                StopDraggingItem();
            }
            
        }
        public void StopDraggingItem() 
        {
            _draggingItemIndex = -1;
            Global.Instance.GetUIManager().ClearIconUnderCursor();
            RenderItems();
        }
        public bool IsDragging()
        {
            return _draggingItemIndex != -1;
        }
        public Game.Inventory.Item GetDraggingItem()
        {
            return Global.Instance.GetInventory().GetItemByIndex(_draggingItemIndex);
        }
    }
}
