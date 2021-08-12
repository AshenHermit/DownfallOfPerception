using Godot;
using System;


namespace Game
{
    public class MobileTurret : Usable, IContainingItem
    {
        [Export]
        public NodePath AreaPath;
        Area area;

        Inventory.Item _turretItem;

        Inventory.Item weaponItem;
        Weapon weapon;

        Spatial _xRotator;
        Spatial _yRotator;
        Vector3 _modelScale;

        Vector3 targetPosition;
        Vector3 softTargetPosition;

        bool _firing;

        enum UseAction
        {
            NONE,
            DETACH,
            GIVE_WEAPON,
            TAKE_WEAPON,
        }
        UseAction _useAction;

        public override void _Ready()
        {
            base._Ready();
            _modelScale = GetNode<Spatial>("mobile_turret").Scale;
            _yRotator = GetNode("mobile_turret").GetNode("Armature").GetNode("base").GetNode<Spatial>("y_rotator");
            _xRotator = _yRotator.GetNode<Spatial>("x_rotator");
            area = GetNode<Area>(AreaPath);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            UpdateRotators();
            FireToNearestEnemy();
        }

        void UpdateRotators()
        {
            softTargetPosition += (targetPosition - softTargetPosition) / 6.0f;

            if (_firing)
            {
                _yRotator.LookAt(softTargetPosition, GlobalTransform.basis.y);
                _yRotator.Rotation = new Vector3(0.0f, _yRotator.Rotation.y, 0.0f);
                _xRotator.LookAt(softTargetPosition, GlobalTransform.basis.y);
                _xRotator.Rotation = new Vector3(_xRotator.Rotation.x, 0.0f, 0.0f);
            }
        }

        public Enemy GetNearestEnemy()
        {
            float minDist = 9999.0f;
            Enemy nearestEnemy = null;
            foreach (Spatial body in area.GetOverlappingBodies())
            {
                if(body is Enemy)
                {
                    float dist = body.GlobalTransform.origin.DistanceTo(GlobalTransform.origin);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestEnemy = (Enemy)body;
                    }
                }
            }
            return nearestEnemy;
        }

        public void FireToNearestEnemy()
        {
            Enemy nearestEnemy = GetNearestEnemy();

            if (nearestEnemy != null)
            {
                targetPosition = nearestEnemy.GlobalTransform.origin;
                if (weapon != null)
                {
                    if (!weapon.IsFiring()) weapon.Fire(true);
                    _firing = true;
                }
            }
            else
            {
                if (weapon != null)
                {
                    if (weapon.IsFiring()) weapon.StopFiring();
                    _firing = false;
                }
            }
        }

        public Inventory.Item GetPlayerWeaponActive()
        {
            Inventory.Item activeItem = Global.Instance.GetInventory().GetActiveItem();
            if (activeItem == null) return null;
            if (!activeItem.GetPropertyValue<bool>("weapon")) return null;
            return activeItem;
        }

        public void DropWeapon()
        {
            if (weaponItem == null) return;
            PickableItem instance = weaponItem.PickableItem.Instance<PickableItem>();
            Global.Instance.CurrentSceneInstance.AddChild(instance);
            instance.GlobalTransform = _xRotator.GlobalTransform;
            instance.Scale = Vector3.One;
            instance.Amount = 1;

            weapon.GetParent().RemoveChild(weapon);
            weapon.QueueFree();
            weapon = null;
            weaponItem = null;
        }

        public void SetWeapon(Inventory.Item item)
        {
            weaponItem = item;
            weapon = weaponItem.GetPropertyValue<PackedScene>("weapon_scene").Instance<Weapon>();
            weapon.Setup(item);
            _xRotator.AddChild(weapon);
            weapon.Scale = Vector3.One/_modelScale;

            weapon.Setup(item);
        }

        public override void Use(Node invoker)
        {
            Inventory.Item activeItem = GetPlayerWeaponActive();

            if(_useAction == UseAction.DETACH)
            {
                Spatial pickableItem = _turretItem.PickableItem.Instance<Spatial>();
                Global.Instance.CurrentSceneInstance.AddChild(pickableItem);
                pickableItem.GlobalTransform = GlobalTransform.Translated(Vector3.Up * 0.5f);
                pickableItem.Scale = Vector3.One;
                QueueFree();
            }
            else if(_useAction == UseAction.GIVE_WEAPON)
            {
                if (activeItem == null) return;
                SetWeapon(activeItem);
                Global.Instance.GetInventory().TakeItem(activeItem.ID, 1);
            }
            else if (_useAction == UseAction.TAKE_WEAPON)
            {
                DropWeapon();
            }
        }

        void UpdateUseAction()
        {
            _useAction = UseAction.NONE;
            if (weaponItem != null)
            {
                _useAction = UseAction.TAKE_WEAPON;
            }
            else
            {
                Inventory.Item activeItem = Global.Instance.GetInventory().GetActiveItem();
                if (activeItem == null)
                {
                    _useAction = UseAction.DETACH;
                }
                else
                {
                    if(activeItem.ID == _turretItem.ID)
                    {
                        _useAction = UseAction.DETACH;
                    }
                    if (activeItem.GetPropertyValue<bool>("weapon"))
                    {
                        _useAction = UseAction.GIVE_WEAPON;
                    }
                }
            }
        }

        public override string GetUseText()
        {
            UpdateUseAction();

            Inventory.Item activeItem = GetPlayerWeaponActive();

            switch (_useAction)
            {
                case UseAction.NONE:
                    return "";
                case UseAction.DETACH:
                    return Global.Translate("use_text.DETACH");
                case UseAction.GIVE_WEAPON:
                    return Global.Translate("use_text.GIVE") + " " + activeItem.Name;
                case UseAction.TAKE_WEAPON:
                    return Global.Translate("use_text.TAKE") + " " + weaponItem.Name;
                default:
                    return "";
            }
            return "";
        }

    public void SetItem(Inventory.Item item)
        {
            _turretItem = item;
        }
}
}
