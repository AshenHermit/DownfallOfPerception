using Godot;
using System;


namespace Game {
    public class PlaceRegion : Area
    {
        [Export]
        public string PlaceableGroup;
        [Export]
        public NodePath PlacePoint;
        Spatial _placePoint;

        public override void _Ready()
        {
            _placePoint = GetNode<Spatial>(PlacePoint);
        }

        public bool UpdateObjectPlacement(Spatial obj)
        {
            obj.GlobalTransform = _placePoint.GlobalTransform;
            return true;
        }

        public void CompleteObjectPlacement(Spatial obj)
        {
            obj.Reparent(GetParent<Spatial>());
            CollisionMask = 0;
            CollisionLayer = 0;
            QueueFree();
        }
    }
}