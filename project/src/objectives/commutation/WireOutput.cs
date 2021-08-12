using Godot;
using System;

namespace Game
{
    public class WireOutput : Spatial
    {
        [Export]
        public NodePath WirePath;
        Wire _wire;

        [Export]
        public string WireId;

        public override void _Ready()
        {
            base._Ready();
            _wire = GetNode<Wire>(WirePath);
            _wire.Id = WireId;
        }

        public void SetWireId(string wireId)
        {
            WireId = wireId;
            _wire.Id = WireId;
        }

        public Wire RequestWire()
        {
            return _wire;
        }
    }
}
