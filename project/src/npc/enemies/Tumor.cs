using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class Tumor : Enemy
    {
        AnimationPlayer _animationPlayer;
        MeshInstance _meshInstance;
        Material _material;

        AudioStreamPlayer3D _sound;

        float _playbackSpeedTarget = 1.0f;

        public override void _Ready()
        {
            _animationPlayer = GetNode(VisualNodePath).GetNode<AnimationPlayer>("AnimationPlayer");
            _animationPlayer.GetAnimation("idle").Loop = true;
            _animationPlayer.Play("idle");

            //TODO: same thing
            _animationPlayer.Seek(GD.Randf() * _animationPlayer.CurrentAnimationLength);

            _sound = GetNode<AudioStreamPlayer3D>("Sound");
            _meshInstance = this.GetAllChildren<MeshInstance>()[0];
            _material = (Material)_meshInstance.GetActiveMaterial(0).Duplicate();
            _meshInstance.SetSurfaceMaterial(0, _material);
            base._Ready();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            _animationPlayer.PlaybackSpeed += (_playbackSpeedTarget - _animationPlayer.PlaybackSpeed) / 20.0f;
        }

        public override void Die()
        {
            Global.Instance.GetGenerationManager().ActionHappened("tumor_died");
            ((SpatialMaterial)_material).EmissionEnabled = false;
            _playbackSpeedTarget = 0.0f;
            _sound.Stop();
            //QueueFree();
        }
    }
}
