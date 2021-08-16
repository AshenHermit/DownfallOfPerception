using Godot;
using System;

namespace Game
{
    public class ConsumingMachine : ItemConsumer
    {
        [Export]
        public NodePath Text3DPath;
        TextIn3D _text3d;
        [Export]
        public NodePath ModelPath;
        Spatial _model;
        AnimationPlayer _animationPlayer;

        float _animSpeedTarget = 0.0f;

        public override void _Ready()
        {
            _text3d = GetNode<TextIn3D>(Text3DPath);
            _model = GetNode<Spatial>(ModelPath);
            _animationPlayer = _model.GetNode<AnimationPlayer>("AnimationPlayer");
            SetupAnimations();
            base._Ready();
        }

        public override void Disable()
        {
            base.Disable();
            _working = false;
            _animSpeedTarget = 0.0f;
            StoppedWorking();
        }

        void SetupAnimations()
        {
            if (_animationPlayer == null) return;
            _animationPlayer.GetAnimation("working").Loop = true;
            _animationPlayer.Play("working");
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            if (_animationPlayer == null) return;
            _animationPlayer.PlaybackSpeed += (_animSpeedTarget - _animationPlayer.PlaybackSpeed)/20.0f;
        }

        public override void StartedWorking()
        {
            _animSpeedTarget = 1.0f;
            UpdateInfo();
        }
        public override void StoppedWorking()
        {
            _animSpeedTarget = 0.0f;
        }
        public override void ItemsChanged()
        {
            UpdateInfo();
        }

        public void UpdateInfo()
        {
            _text3d.SetText(GetInfo());
        }
    }
}
