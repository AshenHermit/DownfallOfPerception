using System;
using Godot;

namespace Game
{
    public class ChargeableBattery : Spatial
    {
        [Export]
        public string DischargedBatteryItemScenePath;
        PackedScene _dischargedBatteryItemScene;
        [Export]
        public string ChargedBatteryItemScenePath;
        PackedScene _chargedBatteryItemScene;

        PackedScene _currentPickableItemScene;
        PickableItem _pickableItem;

        public float ChargingSpeed = 0.0f;
        public float Energy = 0.0f;
        IChargingBattery _chargingEntity;

        public override void _Ready()
        {
            _dischargedBatteryItemScene = GD.Load<PackedScene>(DischargedBatteryItemScenePath);
            _chargedBatteryItemScene = GD.Load<PackedScene>(ChargedBatteryItemScenePath);

            base._Ready();
            CheckChargeState();
        }

        public override void _Process(float delta)
        {
            if(_currentPickableItemScene != null && !IsInstanceValid(_pickableItem))
            {
                _chargingEntity?.StopChargingBattery(this);
                QueueFree();
            }

            if (ChargingSpeed > 0.0f)
            {
                Charge(ChargingSpeed*delta);
            }
        }

        public void SetChargingEntity(IChargingBattery chargingEntity)
        {
            _chargingEntity = chargingEntity;
        }

        void RemovePickableItem()
        {
            if (_pickableItem == null) return;
            if (!IsInstanceValid(_pickableItem)) return;
            _pickableItem.QueueFree();
        }

        void InstancePickableItem(PackedScene itemScene)
        {
            RemovePickableItem();
            _pickableItem = itemScene.Instance<PickableItem>();
            _pickableItem.Set("mode", RigidBody.ModeEnum.Kinematic);
            _pickableItem.Amount = 1;
            AddChild(_pickableItem);
            _currentPickableItemScene = itemScene;
        }
        void CheckChargeState()
        {
            // discharged
            if (Energy < 1.0f)
            {
                if (_currentPickableItemScene != _dischargedBatteryItemScene)
                {
                    InstancePickableItem(_dischargedBatteryItemScene);
                }
            }
            // charged
            else
            {
                if (_currentPickableItemScene != _chargedBatteryItemScene)
                {
                    InstancePickableItem(_chargedBatteryItemScene);
                    _chargingEntity?.OnBatteryCharged(this);
                }
            }
        }

        public void Charge(float factor)
        {
            Energy += factor;
            CheckChargeState();
        }
    }
}
