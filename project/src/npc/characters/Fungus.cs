using Godot;
using System;

namespace Game
{
    public class Fungus : TalkingNpc
    {
        AnimationPlayer _animationPlayer;

        public override void _Ready()
        {
            _animationPlayer = GetNode(VisualNodePath).GetNode<AnimationPlayer>("AnimationPlayer");
            _animationPlayer.GetAnimation("idle").Loop = true;
            _animationPlayer.Play("idle");
            //TODO: move this in utility
            _animationPlayer.Seek(GD.Randf() * _animationPlayer.CurrentAnimationLength);

            AudioStreamPlayer3D soundPlayer = GetNode<AudioStreamPlayer3D>("Sound");
            soundPlayer.Play(soundPlayer.Stream.GetLength() * GD.Randf());
        }

        public override void TakeDamage(float damage)
        {
            
        }

        public override void Die()
        {
            base.Die();
            _animationPlayer.PlaybackSpeed = 0.0f;
            GetNode<AudioStreamPlayer3D>("Sound").Stop();
        }
    }
}
