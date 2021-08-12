using Godot;
using System;

namespace Game
{
    public class Gate : Usable, IVerticalUsable
    {
        [Export]
        public float SlideDistance;
        Vector3 _targetPosition;
        bool _opening = false;

        public Spatial UseInfoPoint { get; set; }

        public override void _Ready()
        {
            base._Ready();
            CallDeferred("DefferedSetup");
            this.SetupVerticalUsable(this);
        }
        public void DefferedSetup()
        {
            _targetPosition = GlobalTransform.origin + Vector3.Up * SlideDistance;
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
            }
            else
            {
                this.UpdateVerticalUsable();
            }
        }

        public override void Use(Node invoker)
        {
            if (Global.Instance.CurrentSceneName == "Memory Storage")
            {
                Global.Instance.GetGenerationManager().ActionHappened("try_open_gate");
            }
        }

        public void Open()
        {
            _opening = true;
            SetUsableState(false);
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
