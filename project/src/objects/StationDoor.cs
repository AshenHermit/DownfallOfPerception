using System;
using Godot;

namespace Game
{
    public class StationDoor : Spatial
    {
        bool _opening = false;
        KinematicBody _body;
        float _xRotation = 0.0f;

        [Export]
        public bool OpenOnLayerGenerated = false;
        [Export]
        public bool OpenOnStart = false;
        [Export]
        public bool PlaySound = true;

        public override void _Ready()
        {
            _body = GetChild<KinematicBody>(0);
            if (OpenOnStart) Open();
            if (OpenOnLayerGenerated) Global.Instance.GetGenerationManager().OnStartAreaGenerated += Open;
        }
        public override void _ExitTree()
        {
            if (OpenOnLayerGenerated) {
                if (Global.Instance.GetGenerationManager() != null) {
                    Global.Instance.GetGenerationManager().OnStartAreaGenerated -= Open;
                }
            }
        }

        public override void _Process(float delta)
        {
            if (_opening)
            {
                _xRotation += (Mathf.Pi / 2.0f - _xRotation) / 10.0f;
            }
            else
            {
                _xRotation += (0.0f - _xRotation) / 10.0f;
            }
            _body.Rotation = new Vector3(_xRotation, 0.0f, 0.0f);
        }

        public void Open()
        {
            if (_opening) return;
            if (PlaySound) Global.Instance.GetAudioManager().PlaySoundAtPosition(
                "res://resources/sounds/effects/metal_door.ogg", GlobalTransform.origin, this);
            _opening = true;
            if (!OpenOnLayerGenerated)
            {
                _body.CollisionLayer = 0;
                _body.CollisionMask = 0;
            }
        }
        public void Close()
        {
            if (!_opening) return;
            if(PlaySound) Global.Instance.GetAudioManager().PlaySoundAtPosition(
                "res://resources/sounds/effects/metal_door.ogg", GlobalTransform.origin, this);
            _opening = false;
            _body.CollisionLayer = 1;
            _body.CollisionMask = 1;
        }
    }
}
