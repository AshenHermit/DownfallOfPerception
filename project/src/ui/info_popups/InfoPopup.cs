using Godot;
using System;

namespace Game.UI
{
    public class InfoPopup : Control
    {
        [Export]
        public NodePath LabelPath;
        Label _label;
        [Export]
        public float Timeout = 2.0f;

        private float _timer = 1.0f;

        public override void _Ready()
        {
            _label = GetNode<Label>(LabelPath);
            _timer = Timeout;
        }

        public override void _Process(float delta)
        {
            _timer -= delta;

            Modulate = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp(_timer, 0.0f, 1.0f));

            if (_timer <= 0.0f)
            {
                GetParent().RemoveChild(this);
                QueueFree();
            }
        }

        public void Start(string message)
        {
            _label.Text = message;
        }
    }
}
