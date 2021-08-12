using Godot;
using System;

namespace Game
{
    public class LoadingScreen : Control
    {
        [Export]
        public NodePath TextLabelPath;
        Label _textLabel;

        int _stringsCount = 3;
        string _string_prefix = "loading_screen_text.";
        string _message = "";

        float _timer = 0.0f;

        public override void _Ready()
        {
            Global.Instance.RegisterLoadingScreen(this);

            _textLabel = GetNode<Label>(TextLabelPath);
            _message = Global.TranslateRandomMessageVariation(_string_prefix, _stringsCount);
            if(Global.Instance.GetGenerationManager()!=null) Global.Instance.GetGenerationManager().OnStartAreaGenerated += EndLoading;
        }

        public void Show()
        {
            Visible = true;
        }

        public void EndLoading()
        {
            QueueFree();
            Global.Instance.GetGenerationManager().OnStartAreaGenerated -= EndLoading;
        }

        public override void _Process(float delta)
        {
            _timer += delta*2.0f;
            string postfix = "";
            for (int i = 0; i < 1 + Mathf.Floor(_timer) % 3; ++i)
            {
                postfix += ".";
            }
            _textLabel.Text = _message+postfix;
        }
    }

}