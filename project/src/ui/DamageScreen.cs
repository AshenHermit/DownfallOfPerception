using Godot;
using System;

namespace Game
{
    class DamageScreen : Control
    {
        float _timer = 0.0f;

        public override void _Ready()
        {
            Visible = true;
            Global.Instance.GetPlayer().OnHealthChangeEvent += ShowHealthChangeScreen;
            ShowHealthChangeScreen(0.0f);
        }

        public override void _Process(float delta)
        {
            if (_timer > 0.0f)
            {
                if (Global.Instance.GetPlayer().GetHealth()>0.0f) _timer -= delta;
                else
                {
                    _timer = 2.0f;
                }
                if (_timer <= 0.0f)
                {
                    _timer = 0.0f;
                }
                UpdateColorModulation();
            }
        }

        void UpdateColorModulation()
        {
            Modulate = new Color(255.0f / 255.0f, 0.0f / 255.0f, 36.0f / 255.0f,
                    _timer / 2.0f + (0.8f - Global.Instance.GetPlayer().GetHealth() / Global.Instance.GetPlayer().GetMaxHealth()) * 0.8f);
        }

        public void ShowHealthChangeScreen(float healthChange)
        {
            if (healthChange < 0.0f)
            {
                _timer = Mathf.Min(1.0f, Math.Abs(healthChange));
            }
            else
            {
                UpdateColorModulation();
            }
        }
    }
}
