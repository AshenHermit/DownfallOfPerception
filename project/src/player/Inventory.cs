using Godot;
using System;
using System.Collections.Generic;
using Game.Utils;


namespace Game
{
    public class Inventory : Node
    {
        /// <summary>
        /// Item struct, has packed scene to spawn when throwing, icon image, item id and amount
        /// </summary>
        public class Item : Godot.Resource
        {
            [Export]
            public PackedScene PickableItem;
            [Export]
            public Texture Icon;
            [Export]
            public string ID;
            [Export]
            public int Amount;

            [Export]
            public string Name;

            [Export]
            public Godot.Collections.Dictionary<string, object> Properties = new Godot.Collections.Dictionary<string, object>();

            /// <summary>
            /// checks if properties contains given key, then returns property value as given type
            /// </summary>
            /// <param name="key">key to property in Properties dictionary</param>
            /// <returns></returns>
            public T GetPropertyValue<T>(string key)
            {
                if (Properties.ContainsKey(key))
                {
                    if(!(Properties[key] is T))
                    {
                        return default(T);
                    }
                    return (T)Properties[key];
                }
                return default(T);
            }
            /// <summary>
            /// Adds "item." before and ".name" after itemId and translates this key with Global.Translate method
            /// </summary>
            /// <param name="itemId">id of item which name needs to be localized</param>
            /// <returns></returns>
            public static string GetLocalizedName(string itemId)
            {
                return Global.Translate("item." + itemId + ".name");
            }
            public static string GetLocalizedDescription(string itemId)
            {
                return Global.Translate("item." + itemId + ".description");
            }
            public Item(string id, int amount=1, Texture icon=null, PackedScene pickableItem=null, Godot.Collections.Dictionary<string, object> userData =null)
            {
                ID = id;
                Amount = amount;
                if (icon != null) Icon = icon;
                if (pickableItem != null) PickableItem = pickableItem;
                if (userData != null) Properties = userData;
                Name = GetLocalizedName(id);
            }
            public Item(Godot.Collections.Dictionary<string, object> dict)
            {
                LoadFromDict(dict);
                Name = GetLocalizedName(ID);
            }

            /// <summary>
            /// Converts item instance to dictionary with all its properties.  
            /// All resources in Properties turned into paths.
            /// </summary>
            /// <returns></returns>
            public Godot.Collections.Dictionary<string, object> ExportToDict()
            {
                Godot.Collections.Dictionary<string, object> dict = new Godot.Collections.Dictionary<string, object>();
                dict["Amount"] = Amount;
                dict["ID"] = ID;
                dict["Properties"] = (Godot.Collections.Dictionary)Properties.Duplicate();
                dict.Get<Godot.Collections.Dictionary>("Properties").TurnResourcesIntoPaths();
                dict["Icon"] = Icon;
                dict["PickableItem"] = PickableItem;
                ((Godot.Collections.Dictionary)dict).TurnResourcesIntoPaths();


                //TODO: DEAD CODE
                //Godot.Collections.Dictionary<string, object> dict = this.ToDict();
                //GD.Print(dict);
                //dict["Properties"] = dict.Get<Godot.Collections.Dictionary<string, object>>("Properties").Duplicate();
                //dict.Get<Godot.Collections.Dictionary<string, object>>("Properties").TurnResourcesIntoPaths();
                //dict.Remove("script");
                //dict.Remove("Reference");
                //dict.Remove("Resource");
                //dict.Remove("resource_path");
                //dict.Remove("resource_name");
                //dict.Remove("Script Variables");
                return dict;
            }
            public void LoadFromDict(Godot.Collections.Dictionary<string, object> dict)
            {
                this.AssignDict(dict);
                Properties.LoadResourcesWithPaths();
            }
        }

        // events
        [Signal]
        /// <summary>
        /// Delegate for item manipulation signals
        /// </summary>
        /// <param name="item">inventory item instance</param>
        /// <param name="amountChange">change in amount of item. >0 increase, <0 decrease.</param>
        public delegate void ItemManipulationHandler(Item item, int amountChange);
        public event ItemManipulationHandler OnItemAddEvent;
        public event ItemManipulationHandler OnItemRemoveEvent;
        public event Action OnItemsArrayChanged;
        public event Action OnActiveItemChange;

        // private fields
        private List<Item> _items = new List<Item>();
        public List<Item> GetItems() { return _items; }
        // private Godot.Collections.Array<Item> _itemsA = new Godot.Collections.Array<Item>();
        private int _activeItemIndex = 0;

        // methods
        public override void _Ready()
        {
            Global.Instance.RegisterInventory(this);
            Resize(24);
        }

        public override void _ExitTree()
        {
            
        }

        public Godot.Collections.Array ExportItems()
        {
            Godot.Collections.Array list = new Godot.Collections.Array();
            foreach (Item item in _items)
            {
                if (item != null)
                {
                    list.Add(item.ExportToDict());
                }
                else
                {
                    list.Add(null);
                }
            }

            return list;
        }
        public void ImportItems(Godot.Collections.Array list)
        {
            for(int i=0; i<list.Count; ++i)
            {
                if (list[i] != null)
                {
                    Godot.Collections.Dictionary<string, object> itemDict = new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)list[i]);
                    Item item = new Item(itemDict);
                    _items.Add(item);
                }
                else
                {
                    _items.Add(null);
                }

            }
        }

        /// <summary>
        /// Change max count of items in inventory. resizes items list and if needs, invokes item remove event
        /// </summary>
        /// <param name="newItemsCount">new max count of items in inventory</param>
        public void Resize(int newItemsCount)
        {
            bool increase = newItemsCount > _items.Count;
            int difference = Math.Abs(newItemsCount - _items.Count);
            if (increase)
            {
                for (int i = 0; i < difference; ++i)
                {
                    _items.Add(null);
                }
            }
            else
            {
                for (int i = 0; i < difference; ++i)
                {
                    if (_items[_items.Count - 1] != null)
                        OnItemRemoveEvent.Invoke(_items[_items.Count - 1], 0);
                    _items.RemoveAt(_items.Count-1);
                }
            }
            if(OnItemsArrayChanged!=null) OnItemsArrayChanged.Invoke();
        }
        public int GetSize() => _items.Count;

        public void SetActiveItemIndex(int activeItemIndex)
        {
            _activeItemIndex = activeItemIndex;
            if(OnActiveItemChange!=null) OnActiveItemChange.Invoke();
        }
        public int GetActiveItemIndex() => _activeItemIndex;
        public Item GetActiveItem() => _items[_activeItemIndex];

        /// <summary>
        /// Gets index of first item with given id
        /// </summary>
        /// <param name="itemId">id of item to find its index in items list</param>
        /// <returns>index of item in items list</returns>
        public int GetItemIndex(string itemId)
        {
            return _items.FindIndex((x)=>{
                if (x != null) return x.ID == itemId;
                return false;
            });
        }
        /// <summary>
        /// Gets index of first empty place in items list.
        /// </summary>
        /// <returns>index of first empty place in items list.</returns>
        public int GetEmptyPlaceIndex()
        {
            return _items.FindIndex(x => x == null);
        }

        // add item
        public void AddItem(string itemId, int amount=1, Texture icon =null, PackedScene pickableItem=null, Godot.Collections.Dictionary<string, object> properties =null) 
        {
            int idx = GetItemIndex(itemId);
            Item item = null;
            if (idx != -1)
            {
                item = _items[idx];
                item.Amount += amount;
            }
            else
            {
                if (GetActiveItem() == null)
                {
                    idx = _activeItemIndex;
                }
                else
                {
                    idx = GetEmptyPlaceIndex();
                }
                item = new Item(itemId, amount, icon, pickableItem, properties);
                _items[idx] = item;
            }
            
            if(idx == _activeItemIndex)
            {
                if (OnActiveItemChange!= null) OnActiveItemChange.Invoke();
            }

            if (OnItemAddEvent!=null) OnItemAddEvent.Invoke(item, amount);
            if (OnItemsArrayChanged != null) OnItemsArrayChanged.Invoke();
        }
        public void AddItem(IPickableItem pickableItem)
        {
            AddItem(
                pickableItem.GetItemId(),
                pickableItem.GetAmount(),
                pickableItem.GetIcon(),
                pickableItem.GetItemScene(),
                pickableItem.GetProperties());
        }

        // take item
        public bool HasAmountOfItem(string itemId, int amount)
        {
            int idx = GetItemIndex(itemId);
            if (idx != -1)
            {
                Item item = _items[idx];
                if (item.Amount - amount >= 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Takes amount of item from items list
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns>if item has amount which needs to be taken, returns true, otherwise returns false.</returns>
        public bool TakeItem(string itemId, int amount)
        {
            if (HasAmountOfItem(itemId, amount))
            {
                int idx = GetItemIndex(itemId);
                Item item = GetItemByIndex(idx);
                item.Amount -= amount;
                if (item.Amount == 0)
                {
                    _items[idx] = null;
                    if (idx == _activeItemIndex)
                    {
                        if (OnActiveItemChange != null) OnActiveItemChange.Invoke();
                    }
                }
                if (OnItemRemoveEvent != null) OnItemRemoveEvent.Invoke(item, -amount);
                if (OnItemsArrayChanged != null) OnItemsArrayChanged.Invoke();
                return true;
            }
            return false;
        }
        public bool TakeWholeAmountOfItem(string itemId)
        {
            return TakeItem(itemId, GetItem(itemId).Amount);
        }

        // swap items
        public void SwapItems(int itemAIndex, int itemBIndex)
        {
            Item itemA = _items[itemAIndex];
            _items[itemAIndex] = _items[itemBIndex];
            _items[itemBIndex] = itemA;
            if (OnItemsArrayChanged != null) OnItemsArrayChanged.Invoke();
            if(itemAIndex == _activeItemIndex || itemBIndex == _activeItemIndex)
            {
                if (OnActiveItemChange != null) OnActiveItemChange.Invoke();
            }
        }

        // get item
        public Item GetItemByIndex(int index)
        {
            return _items[index];
        }
        public Item GetItem(string itemId)
        {
            int idx = GetItemIndex(itemId);
            if(idx!=-1) return _items[idx];
            return null;
        }

    }
}
