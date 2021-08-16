using System;
using Godot;

namespace Game
{
    public interface IChargingBattery
    {
        /// <summary>
        /// Add instanced battery to the tree, setup battery position and set charging speed.
        /// </summary>
        void StartChargingBattery(ChargeableBattery battery, Transform batteryTransform);
        void StopChargingBattery(ChargeableBattery battery);
        /// <summary>
        /// When battery charged
        /// </summary>
        void OnBatteryCharged(ChargeableBattery battery);
    }
}
