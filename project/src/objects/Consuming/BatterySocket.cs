using Godot;
using System;

namespace Game
{
    public class BatterySocket : ItemConsumer
    {
        [Export]
        public NodePath ModelPath;
        MeshInstance _mesh;

        [Export]
        public NodePath BatteryModelPath;
        MeshInstance _batteryMesh;

        SpatialMaterial material;

        public string Id = "terminal";
        bool _ready = false;

        public override void _Ready()
        {
            _canUseWhenWorking = false;
            _batteryMesh = GetNode<MeshInstance>(BatteryModelPath);
            _mesh = GetNode<MeshInstance>(ModelPath);
            material = (SpatialMaterial)_mesh.GetActiveMaterial(0).Duplicate();
            _mesh.SetSurfaceMaterial(0, material);

            base._Ready();
            ClearItems();
            _ready = true;
        }
        public override void StartedWorking()
        {
            base.StartedWorking();
            if (!_ready) return;
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/effects/zap.ogg", GlobalTransform.origin, this);
            material.EmissionEnabled = true;
            _batteryMesh.Visible = true;
            Global.Instance.GetGenerationManager()?.ActionHappened(Id+"_battery_socket_activated");
        }
        public override void StoppedWorking()
        {
            base.StoppedWorking();
            _batteryMesh.Visible = false;
            material.EmissionEnabled = false;
        }
    }
}