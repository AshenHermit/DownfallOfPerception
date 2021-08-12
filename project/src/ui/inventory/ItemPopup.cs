using Godot;
using System;

namespace Game.UI
{
    public class ItemPopup : Control
    {
        [Export]
        public Godot.Collections.Array<NodePath> LabelsNodePaths = new Godot.Collections.Array<NodePath>();
        [Export]
        public float Timeout = 1.0f;

        private float _timer = 1.0f;
        private float _xPosition;
        private float _yOffset;

        public void SetLabelText(int labelIndex, string text)
        {
            GetNode<Label>(LabelsNodePaths[labelIndex]).Text = text;
        }
        public override void _Ready()
        {
            _timer = Timeout;
            _xPosition = -MarginRight-10;
        }
        public override void _Process(float delta)
        {
            _timer -= delta;
            //_yOffset -= (1.0f-Modulate.a)/20.0f;

            Modulate = new Color(1.0f, 1.0f, 1.0f, Mathf.Min(1.0f, _timer));
            _xPosition -= (1.0f - Modulate.a) * 10.0f;
            _xPosition += (0.0f - _xPosition)/4.0f;

            if (_timer <= 0.0f)
            {
                GetParent().RemoveChild(this);
                QueueFree();
            }
            RectPosition = new Vector2(_xPosition, RectPosition.y + _yOffset);
        }

        public void Start(string message, Game.Inventory.Item item, int amount)
        {
            SetLabelText(0, message);
            SetLabelText(1, item.Name);
            SetLabelText(2, "x"+amount.ToString());
        }
    }

}
