using Godot;
using System;

namespace Game.UI
{
    public class HealthBar : Control
    {
        [Export]
        public NodePath BackgroundPanelPath;
        Control _backgroundPanel;
        [Export]
        public NodePath HealthPanelPath;
        Panel _healthPanel;
        [Export]
        public NodePath SlowHealthPanelPath;
        Panel _slowHealthPanel;

        Player _player;

        float _barValue;

        public override void _Ready()
        {
            _backgroundPanel = GetNode<Control>(BackgroundPanelPath);
            _healthPanel = GetNode<Panel>(HealthPanelPath);
            _slowHealthPanel = GetNode<Panel>(SlowHealthPanelPath);

            _player = Global.Instance.GetPlayer();
            _player.OnHealthChangeEvent += OnPlayerHealthChange;

            OnPlayerHealthChange(0.0f);
        }

        public void OnPlayerHealthChange(float healthChange)
        {
            SetBarsValue(_player.MaxHealth, _player.GetHealth());
        }

        public override void _Process(float delta)
        {
            UpdateBarLength(_healthPanel, 0.5f);
            UpdateBarLengthLinear(_slowHealthPanel, 0.25f);
        }

        void SetBarsValue(float maxValue, float value)
        {
            _barValue = value/maxValue;
        }

        void UpdateBarLength(Control bar, float speed = 0.5f)
        {
            Vector2 size = bar.RectSize;
            size.x += ((_backgroundPanel.RectSize.x * _barValue) - size.x) * speed;
            size.y = _backgroundPanel.RectSize.y;
            bar.RectSize = size;
        }
        void UpdateBarLengthLinear(Control bar, float speed = 0.5f)
        {
            Vector2 size = bar.RectSize;
            size.y = _backgroundPanel.RectSize.y;
            if (_backgroundPanel.RectSize.x * _barValue < size.x)
            {
                size.x -= speed;
            }
            else
            {
                size.x = _backgroundPanel.RectSize.x * _barValue;
            }
            bar.RectSize = size;
        }
    }
}
