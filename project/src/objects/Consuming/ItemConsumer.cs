using System;
using Godot;


namespace Game
{
    public class ItemConsumer : Usable
    {
        [Export]
        public Godot.Collections.Dictionary<string, int> NeedItemsCount = new Godot.Collections.Dictionary<string, int>();
        public Godot.Collections.Dictionary<string, int> ItemsCount;

        bool _working = false;

        public string InfoTitle="";

        public bool Disabled = false;

        public ItemConsumer()
        {

        }

        public virtual void Disable()
        {
            Disabled = true;
            SetUsableState(false);
        }

        public override void _Ready()
        {
            base._Ready();
            ItemsCount = NeedItemsCount.Duplicate();
            UpdateWorkingState();
        }

        public override void Use(Node invoker)
        {
            if (Disabled) return;

            if (!IsWorking())
            {
                string needItemId = GetNeededItemId();
                if (Global.Instance.GetInventory().HasAmountOfItem(needItemId, 1))
                {
                    Global.Instance.GetInventory().TakeItem(needItemId, 1);
                    if (!ItemsCount.ContainsKey(needItemId)) ItemsCount[needItemId] = 0;
                    ItemsCount[needItemId] += 1;
                    GDE.Print(ItemsCount);
                }
                UpdateWorkingState();
                ItemsChanged();
            }
            else
            {
                WorkingUse();
            }
        }

        public bool IsWorking() => _working;
        public void UpdateWorkingState()
        {
            bool nextWorkingState = HasNeededItems();
            if(nextWorkingState && !_working)
            {
                StartedWorking();
            }
            else if (!nextWorkingState && _working)
            {
                StoppedWorking();
            }
            _working = nextWorkingState;
        }
        public void ClearItems()
        {
            ItemsCount = new Godot.Collections.Dictionary<string, int>();
            UpdateWorkingState();
            ItemsChanged();
        }

        public virtual void StartedWorking()
        {

        }
        public virtual void StoppedWorking()
        {

        }
        public virtual void ItemsChanged()
        {

        }
        public virtual void WorkingUse()
        {

        }


        public string GetInfo()
        {
            if (Disabled) return "";

            if (_working)
            {
                return "";
            }
            string text = "";
            if (InfoTitle != "") text += InfoTitle + "\n\n";
            text += Global.Translate("item_consumer.lack_of_items") + "\n";
            foreach (string needItemId in NeedItemsCount.Keys)
            {
                if (ItemIsNeeded(needItemId))
                {
                    text += Inventory.Item.GetLocalizedName(needItemId) + " x" + GetNeededCountOfItem(needItemId).ToString();
                    text += "\n";
                }
            }
            return text;
        }

        public bool HasNeededItems()
        {
            return GetNeededItemId() == "";
        }

        public bool ItemIsNeeded(string itemId)
        {
            if (!ItemsCount.ContainsKey(itemId)) return true;
            if (ItemsCount[itemId] < NeedItemsCount[itemId]) return true;
            return false;
        }

        public string GetNeededItemId()
        {
            foreach (string needItemId in NeedItemsCount.Keys)
            {
                if (ItemIsNeeded(needItemId)) return needItemId;
            }
            return "";
        }

        public int GetNeededCountOfItem(string itemId)
        {
            if (!ItemsCount.ContainsKey(itemId)) return NeedItemsCount[itemId];
            return Math.Max(0, NeedItemsCount[itemId] - ItemsCount[itemId]);
        }

        public override string GetUseText()
        {
            if (!IsWorking())
            {
                return Global.Translate("use_text.GIVE") + " " + Inventory.Item.GetLocalizedName(GetNeededItemId());
            }
            else
            {
                return Global.Translate("use_text.USE");
            }
        }
    }
}
