using Godot;
using System;


namespace Game
{
    public class MonologueOverlay : Control
    {
        [Export]
        public NodePath LabelPath;
        Label _label;

        float _alpha = 0.0f;

        public override void _Ready()
        {
            _label = GetNode<Label>(LabelPath);
            UpdateAlpha();
            Global.Instance.GetNpcManager().OnMonologueStarted += SomebodyStartedTalking;
        }
        public override void _Process(float delta)
        {
            if (Global.Instance.GetNpcManager().IsSomebodyTalking())
            {
                _alpha += (1.0f - _alpha) / 16.0f;
            }
            else
            {
                _alpha += (0.0f - _alpha) / 16.0f;
            }
            UpdateAlpha();
        }
        void UpdateAlpha()
        {
            Modulate = new Color(1.0f, 1.0f, 1.0f, _alpha);
        }

        public void SomebodyStartedTalking(string text)
        {
            _label.Text = text;
        }
    }
}
