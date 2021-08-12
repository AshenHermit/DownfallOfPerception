using Godot;
using System;


namespace Game
{
    public class DestroyControl : Usable
    {
        [Export]
        public NodePath InfoPath;
        TextIn3D _info;

        AnimationPlayer _animationPlayer;

        bool _readyToDestroy = false;
        float _maxProgress = 256.0f*256.0f;
        float _progress;
        bool _activated = false;

        public override void _Ready()
        {
            base._Ready();
            _info = GetNode<TextIn3D>(InfoPath);
            _animationPlayer = GetChild(0).GetNode<AnimationPlayer>("AnimationPlayer");
            UpdateInfo();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (!_readyToDestroy)
            {
                _progress += (_maxProgress - _progress) / 2000.0f;
                UpdateInfo();
            }
            else
            {
                if (_animationPlayer.IsPlaying() && _animationPlayer.CurrentAnimation == "destroy_control_activate")
                {
                    
                }
            }
        }

        public override void Use(Node invoker)
        {
            base.Use(invoker);
            if (_readyToDestroy)
            {
                _animationPlayer.Play("destroy_control_activate");
                SetUsableState(false);
                Global.Instance.GetGenerationManager().ActionHappened("destroy_activated");
                _activated = true;
            }
        }

        public void MakeReadyToDestroy()
        {
            _readyToDestroy = true;
            _progress = _maxProgress;
            UpdateInfo();
        }

        void UpdateInfo()
        {
            string text = "";
            if (_readyToDestroy)
            {
                text = Global.Translate("ai_destroy_control.info.ready");
            }
            else
            {
                text = Global.Translate("ai_destroy_control.info.in_progress");
            }
            text += "\n";
            text += ((int)_progress).ToString() + " / " + ((int)_maxProgress).ToString();
            _info.SetText(text);
        }

        public override string GetUseText()
        {
            if (_readyToDestroy)
            {
                return Global.Translate("use_text.ACTIVATE");
            }
            else
            {
                return "";
            }
        }
    }
}
