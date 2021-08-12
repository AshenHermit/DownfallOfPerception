using Godot;
using System;

namespace Game
{
    public class Detachable : RigidBody
    {
        [Export]
        public float SlideDownSpeed = 0.0f;
        [Export]
        public float SlideDownDistance = 0.0f;
        [Export]
        public PackedScene SceneToReplace;
        [Export]
        public AudioStream DetachSoundStream;



        bool _sliding = false;
        bool _slided = false;
        bool _detached = false;
        Vector3 origin;

        public override void _Ready()
        {
            CallDeferred("SetupOrigin");
        }

        public void SetupOrigin()
        {
            origin = GlobalTransform.origin;
        }

        public override void _Process(float delta)
        {
            if (_sliding)
            {
                Transform trans = GlobalTransform;
                trans.origin -= trans.basis.y * SlideDownSpeed * delta;
                GlobalTransform = trans;

                if (trans.origin.DistanceTo(origin) >= SlideDownDistance)
                {
                    _slided = true;
                    _sliding = false;
                }
            }
        }

        public void Detach()
        {
            if (!_slided && SlideDownDistance > 0.0f)
            {
                _sliding = true;
                return;
            }

            if (!_detached)
            {
                Mode = ModeEnum.Rigid;
                CollisionLayer = 2;
                CollisionMask = 2;
                _detached = true;

                if (DetachSoundStream != null)
                {
                    Global.Instance.GetAudioManager().PlaySoundAtPosition(DetachSoundStream, GlobalTransform.origin, GetParent<Spatial>());
                }

                if (SceneToReplace != null)
                {
                    Spatial instance = SceneToReplace.Instance<Spatial>();
                    GetParent().AddChild(instance);
                    instance.GlobalTransform = GlobalTransform;
                    QueueFree();
                }
                return;
            }
        }
    }
}