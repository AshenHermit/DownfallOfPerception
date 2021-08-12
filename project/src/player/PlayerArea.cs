using Godot;
using System;

namespace Game
{
    public class PlayerArea : Area, IPlayerComponent
    {
        [Export]
        public NodePath PlayerNodePath;

        // private fields
        private Player _player;

        // methods
        public void Setup(Player player)
        {
            _player = player;
        }
        public override void _Ready()
        {
            Setup(GetNode<Player>(PlayerNodePath));
        }


        /// <summary>
        /// get nearest IUsable body
        /// </summary>
        /// <returns></returns>
        public IUsable GetNearestUsableObject()
        {
            float maxDistance = 100.0f;
            IUsable nearestObject = null;
            Godot.Collections.Array array = GetOverlappingBodies();
            foreach (Node body in array)
            {
                if (body is Spatial && body is IUsable)
                {
                    Spatial spatialBody = (Spatial)body;
                    if (!_player.GetCamera().IsPositionBehind(spatialBody.GlobalTransform.origin))
                    {
                        float distance = spatialBody.GlobalTransform.origin.DistanceTo(_player.GlobalTransform.origin);
                        if (distance < maxDistance)
                        {
                            maxDistance = distance;
                            nearestObject = (IUsable)body;
                        }
                    }
                }
            }
            return nearestObject;
        }
        /// <summary>
        /// call Use method of nearest IUsable body
        /// </summary>
        public void UseNearestObject()
        {
            IUsable nearestUsable = GetNearestUsableObject();
            if (nearestUsable!=null) ((IUsable)nearestUsable).Use(_player);
        }
    }
}
