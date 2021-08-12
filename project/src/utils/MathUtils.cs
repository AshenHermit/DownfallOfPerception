using Godot;
using System;

namespace Game.Utils
{
    public static class MathUtils
    {
        public static Vector3 Randv()
        {
            return (new Vector3(GD.Randf(), GD.Randf(), GD.Randf())*2.0f-Vector3.One).Normalized();
        }

        public static bool RayIsIntersecting(this PhysicsDirectSpaceState directSpaceState, Vector3 from, Vector3 to, Godot.Collections.Array execlude=null, uint collisionMask = 2147483647)
        {
            Godot.Collections.Dictionary collision = directSpaceState.IntersectRay(from, to, execlude, collisionMask);
            return collision.Count > 0;
        }

        public static void SetRotationAlongNormal(this Spatial obj, Vector3 origin, Vector3 normal, Vector3 forward)
        {
            Transform trans = obj.GlobalTransform;
            trans.origin = Vector3.Zero;
            trans = trans.LookingAt(trans.origin + normal, forward);
            trans = trans.Rotated(trans.basis.x, -Mathf.Pi / 2.0f);
            trans.origin = origin;
            obj.GlobalTransform = trans;
        }
    }
}
