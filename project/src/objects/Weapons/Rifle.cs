using Godot;
using Game.Utils;
using System;

namespace Game
{
    public class Rifle : Weapon
    {
        [Export]
        public NodePath VisualNodePath;
        AnimationPlayer _animationPlayer;

        [Export]
        public NodePath AreaPath;
        Area _area;

        AudioManager.AudioUnit _lastAudio;

        float Damage = 35.0f;

        public override void _Ready()
        {
            base._Ready();
            _animationPlayer = GetNode(VisualNodePath).GetNode<AnimationPlayer>("AnimationPlayer");
            _area = GetNode<Area>(AreaPath);
            _animationPlayer.Play("init");
            Damage = Item.Properties.GetFloat("damage");
        }

        public override void Fire(bool primary = true)
        {
            base.Fire(primary);
        }

        public override void MakeShot(bool primary)
        {
            Global.Instance.GetPlayerCamera().Shake(1.0f);

            if (_lastAudio != null && IsInstanceValid(_lastAudio))
            {
                _lastAudio.Stop();
            }
            _lastAudio = Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/weapons/rifle/rifle_shot.mp3",
                MuzzlePoint.GlobalTransform.origin, this);

            base.MakeShot(primary);
            _animationPlayer.Stop(true);
            _animationPlayer.Play("fire", 0.001f);
            foreach (Node body in _area.GetOverlappingBodies())
            {
                if (body != Global.Instance.GetPlayer())
                {
                    if(body is IEntity)
                    {
                        IEntity entity = (IEntity)body;
                        if (entity.IsDamageable())
                        {
                            entity.TakeDamage(Damage);
                        }
                    }
                }
                if(body is RigidBody)
                {
                    ((RigidBody)body).ApplyImpulse(Vector3.Zero, -GlobalTransform.basis.z*100.0f);
                }
            }
        }

        public override void StopFiring()
        {
            base.StopFiring();
        }
    }
}
