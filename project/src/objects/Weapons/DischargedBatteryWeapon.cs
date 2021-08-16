using Godot;
using System;

namespace Game
{
    public class DischargedBatteryWeapon : Weapon
    {
        [Export]
        public NodePath ModelPath;
        [Export]
        public PackedScene ChargeableBatteryScene;
        Spatial _model;
        [Export]
        public NodePath ModelContainerPath;
        NoiseMovement _modelContainerPath;

        public bool _swinging = false;
        public float _swingStrength = 0.0f;

        static float _modelDefaultZPos = -3.184f;
        static float _modelSwingZPos = 1.906f;
        static float _readySwingTreshold = 0.9f;

        bool _thrown = false;

        public override void _Ready()
        {
            base._Ready();

            _modelContainerPath = GetNode<NoiseMovement>(ModelContainerPath);
            _model = GetNode<Spatial>(ModelPath);
            _modelContainerPath.Speed = 2.0f;
        }


        public override void _Process(float delta)
        {
            base._Process(delta);

            if (IsFiring())
            {
                _swinging = true;
            }
            else
            {
                _swinging = false;
            }
            UpdateModelPosition(delta);
        }

        public void UpdateModelPosition(float delta)
        {
            Transform trans = _model.Transform;
            if (_swinging)
            {
                trans.origin += (new Vector3(0.0f, 0.0f, _modelSwingZPos) - trans.origin) / 50.0f;
                _swingStrength += (1.0f - _swingStrength) / 50.0f;
            }
            else
            {
                trans.origin += (new Vector3(0.0f, 0.0f, _modelDefaultZPos) - trans.origin) / 2.0f;
                _swingStrength += (0.0f - _swingStrength) / 2.0f;
            }
            if (_swingStrength > _readySwingTreshold) _modelContainerPath.Strength = _swingStrength * 0.4f;
            else _modelContainerPath.Strength = 0.0f;
            _model.Transform = trans;
        }

        public override void MakeShot(bool primary)
        {
            base.MakeShot(primary);
        }
        public override void Fire(bool primary = true)
        {
            base.Fire(primary);
            _thrown = false;
        }

        public void ThrustIn()
        {
            if (_thrown) return;

            Transform trans = _model.Transform;
            trans.origin = new Vector3(0.0f, 0.0f, _modelDefaultZPos - _swingStrength * 2.0f);
            _model.Transform = trans;
            Vector3 impulse = -Global.Instance.GetPlayerCamera().GlobalTransform.basis.z * _swingStrength * 20.0f;
            Global.Instance.GetPlayerCamera().Shake(_swingStrength*3.0f);
            impulse.y /= 4.0f;
            Global.Instance.GetPlayer().Velocity += impulse;

            if (_swingStrength > _readySwingTreshold)
            {
                Godot.Collections.Dictionary collision = Global.Instance.GetPlayerCamera().CastRay(7.0f, 1);
                if (collision.Count > 0)
                {
                    if(collision["collider"] is IChargingBattery)
                    {
                        IChargingBattery chargingEntity = ((IChargingBattery)collision["collider"]);

                        _thrown = true;
                        Transform batteryTrans = _model.GlobalTransform;
                        batteryTrans.origin -= _model.GlobalTransform.basis.z * 1.0f;
                        chargingEntity.StartChargingBattery(ChargeableBatteryScene.Instance<ChargeableBattery>(), batteryTrans);
                        Global.Instance.GetInventory().TakeItem(Item.ID, 1);
                    }
                }
            }

            _swingStrength = 0.0f;
        }

        public override void StopFiring()
        {
            if(IsFiring()) ThrustIn();
            base.StopFiring();
        }
    }
}
