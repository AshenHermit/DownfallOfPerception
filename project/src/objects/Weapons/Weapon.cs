using Godot;
using System;

namespace Game
{
    public class Weapon : Spatial
    {
        [Export]
        public float FireSpeed;
        [Export]
        public bool OneShot;
        [Export]
        public NodePath MuzzlePointNodePath;
        public Spatial MuzzlePoint;

        public Inventory.Item Item;

        public bool CanBeRotated = true;

        private bool _isFiring = false;
        private float _timer = 0.0f;
        bool _firing_primary = false;

        string _ammoItemId = "";

        public bool IsFiring() => _isFiring;

        public void Setup(Inventory.Item item) {
            Item = item;
        }
        public override void _Ready()
        {
            base._Ready();
            _ammoItemId = GetAmmoItemIdFromItemInstance();
            if(MuzzlePointNodePath!=null) MuzzlePoint = GetNode<Spatial>(MuzzlePointNodePath);

            if (Item.Properties.ContainsKey("fire_speed")) FireSpeed = Item.GetPropertyValue<float>("fire_speed");
        }
        public override void _Process(float delta)
        {
            if (_timer > 0.0f)
            {
                _timer -= delta;
            }
            if (_isFiring && _timer<=0.0f)
            {
                MakeShotAndTakeAmmo();
                _timer = FireSpeed;
            }
        }

        public string GetAmmoItemIdFromItemInstance()
        {
            if (Item != null)
            {
                if (Item.Properties.ContainsKey("ammo_item_id"))
                {
                    return (string)Item.Properties["ammo_item_id"];
                }
            }
            return "";
        }
        public bool HasAmo()
        {
            if (_ammoItemId != "")
            {
                return Global.Instance.GetInventory().HasAmountOfItem(_ammoItemId, 1);
            }
            return true;
        }
        public void TakeAmmo()
        {
            if (_ammoItemId != "")
            {
                Global.Instance.GetInventory().TakeItem(_ammoItemId, 1);
            }
        }

        public virtual void MakeShot(bool primary)
        {

        }

        public void MakeShotAndTakeAmmo()
        {
            if (HasAmo())
            {
                MakeShot(_firing_primary);
                TakeAmmo();
            }
            else
            {
                StopFiring();
            }
        }

        public virtual void Fire(bool primary=true)
        {
            if (HasAmo()) {
                _isFiring = true;
                _firing_primary = primary;
                if (OneShot)
                {
                    MakeShotAndTakeAmmo();
                    _isFiring = false;
                }
                else
                {
                    
                }
            }
        }
        public virtual void StopFiring()
        {
            _isFiring = false;
        }
    }
}
