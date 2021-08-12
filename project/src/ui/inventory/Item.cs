using Godot;
using System;

namespace Game.UI {

    public class Item : TextureRect
    {
        [Export]
        public Texture EmptyIcon;

        [Export]
        public NodePath NameLabelNodePath;
        private Label _nameLabel;
        [Export]
        public NodePath AmountLabelNodePath;
        private Label _amountLabel;
        [Export]
        public NodePath SelectionNodePath;

        private int _itemIndex = -1;
        private Game.Inventory.Item _item = null;
        private UI.Inventory _uiInventory;
        private bool _draggable = false;

        public override void _Ready()
        {
            _nameLabel = GetNode<Label>(NameLabelNodePath);
            _amountLabel = GetNode<Label>(AmountLabelNodePath);
        }

        public void OnGuiInput(InputEvent e)
        {
            if(e is InputEventMouseButton)
            {
                if (_draggable) { 
                    InputEventMouseButton buttonEvent = (InputEventMouseButton)e;
                    if (buttonEvent.ButtonIndex == (int)ButtonList.Left)
                    {
                        if (buttonEvent.IsPressed())
                        {
                            if (HasItem() && _uiInventory != null && !_uiInventory.IsDragging())
                            {
                                _uiInventory.StartDraggingItem(this);
                            }
                            else if (_uiInventory.IsDragging() && _uiInventory != null)
                            {
                                _uiInventory.EndDraggingItem(this);
                            }
                        }
                    }
                }
            }
            if (e is InputEventMouseMotion)
            {
                if (HasItem())
                {
                    _uiInventory.SelectItem(_item);
                }
            }
        }

        public void Setup(int index, Game.Inventory.Item item)
        {
            SetItemIndex(index);
            if (item != null)
            {
                SetInventoryItem(item);
                ShowInfo();
            }
            else
            {
                HideInfo();
            }
        }
        /// <summary>
        /// Check if ui item contains valid inventory item
        /// </summary>
        /// <returns></returns>
        public bool HasItem()
        {
            return _item != null;
        }
        public void SetInventoryItem(Game.Inventory.Item item)
        {
            _item = item;
        }
        public Game.Inventory.Item GetInventoryItem() => _item;
        public void SetItemIndex(int itemIndex)
        {
            _itemIndex = itemIndex;
            _item = Global.Instance.GetInventory().GetItemByIndex(_itemIndex);
        }
        public int GetItemIndex() => _itemIndex;
        /// <summary>
        /// Sets icon of ui item
        /// </summary>
        /// <param name="icon">image texture instance, or null for empty icon</param>
        public void SetIcon(Texture icon)
        {
            if (icon != null)
            {
                Texture = icon;
            }
            else
            {
                Texture = EmptyIcon;
            }
        }
        public void SetUIInventory(UI.Inventory uiInventory)
        {
            _uiInventory = uiInventory;
        }
        public void SetDraggable(bool draggable)
        {
            _draggable = draggable;
        }
        public void SetItemName(string name)
        {
            _nameLabel.Text = name;
        }
        public void SetItemAmount(int amount)
        {
            _amountLabel.Text = amount.ToString();
        }

        public void ShowInfo()
        {
            if (HasItem())
            {
                SetIcon(_item.Icon);
                SetItemName(_item.Name);
                SetItemAmount(_item.Amount);
            }
        }
        public void HideInfo()
        {
            SetIcon(null);
            _nameLabel.Text = "";
            _amountLabel.Text = "";
        }

        public void SetSelectionState(bool state)
        {
            GetNode<Control>(SelectionNodePath).Visible = state;
        }
    }
}
