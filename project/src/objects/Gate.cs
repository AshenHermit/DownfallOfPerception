using Godot;
using System;

namespace Game
{
    public class Gate : Usable, IVerticalUsable
    {
        [Export]
        public float SlideDistance;
        Vector3 _startPosition;
        Vector3 _targetPosition;
        bool _opening = false;
        bool _opened = false;

        public Spatial UseInfoPoint { get; set; }

        float closeTimer = 0.0f;

        public override void _Ready()
        {
            base._Ready();
            CallDeferred("DefferedSetup");
            this.SetupVerticalUsable(this);
        }
        public void DefferedSetup()
        {
            _startPosition = GlobalTransform.origin;
            _targetPosition = _startPosition + Vector3.Up * SlideDistance;
        }
        public override void _Process(float delta)
        {
            base._Process(delta);
            if (_opening)
            {
                if (GlobalTransform.origin.y < _targetPosition.y)
                {
                    RotateObjectLocal(Vector3.Up, delta * Mathf.Pi);
                    Translate(delta * Vector3.Up * 10.0f);
                }
                else
                {
                    if (!_opened)
                    {
                        _opened = true;
                        GetNode<AudioStreamPlayer3D>("Sound").Stop();
                        closeTimer = 20.0f + GD.Randf() * 30.0f;
                    }
                    else
                    {
                        if (closeTimer > 0.0f)
                        {
                            closeTimer -= delta;
                            if (closeTimer < 0.0f)
                            {
                                Close();
                            }
                        }
                    }
                }
            }
            else
            {
                if (GlobalTransform.origin.y > _startPosition.y)
                {
                    RotateObjectLocal(Vector3.Down, -delta * Mathf.Pi);
                    Translate(delta * Vector3.Down * 10.0f);
                }
                else {
                    if (_opened)
                    {
                        _opened = false;
                        GetNode<AudioStreamPlayer3D>("Sound").Stop();
                    }
                }
            }
            this.UpdateVerticalUsable();
        }

        public override void Use(Node invoker)
        {
            if (_opening) return;
            if (Global.Instance.CurrentSceneName == "Memory Storage")
            {
                Global.Instance.GetGenerationManager().ActionHappened("try_open_gate");
            }
        }

        public void Open()
        {
            if (!_opened)
            {
                _opening = true;
                SetUsableState(false);
                GetNode<AudioStreamPlayer3D>("Sound").Play();
            }
        }
        public void Close()
        {
            if (_opened)
            {
                _opening = false;
                SetUsableState(true);
                GetNode<AudioStreamPlayer3D>("Sound").Play();
            }
        }

        public override string GetUseText()
        {
            return Global.Translate("use_text.OPEN");
        }

        public override Spatial GetUseInfoPoint()
        {
            return this.UseInfoPoint;
        }
    }
}
