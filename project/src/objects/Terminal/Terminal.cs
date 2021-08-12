using Godot;
using System;

namespace Game
{
    public class Terminal : Usable
    {
        [Export]
        public NodePath DisplayPath;
        TextIn3D _diplay;

        Godot.Collections.Array<string> _pageTexts = new Godot.Collections.Array<string>();
        int _currentPage = 0;

        bool _shure = false;
        bool _chekingIfShure = false;
        bool _activated = false;

        public override void _Ready()
        {
            for(int i=0; i<3; ++i)
                _pageTexts.Add(Global.Translate("terminal.page."+i.ToString()));

            _diplay = GetNode<TextIn3D>(DisplayPath);

            UpdateDisplay();
        }

        public override void Use(Node invoker)
        {
            if (CanActivate())
            {
                Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/effects/button_low.ogg", GlobalTransform.origin, this);
                if (!_activated && !_shure)
                {
                    _chekingIfShure = true;
                    UpdateDisplay();
                    return;
                }
            }
            else
            {
                Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/effects/button_low.ogg", GlobalTransform.origin, this);
            }
        }

        public override string GetUseText()
        {
            if (CanActivate())
            {
                return Global.Translate("use_text.ACTIVATE");
            }
            return "";
        }

        void UpdateDisplay()
        {
            if (_chekingIfShure)
            {
                _diplay.SetText(Global.Translate("terminal.checking_if_sure"));
                return;
            }
            if (_activated)
            {
                _diplay.SetText(Global.Translate("terminal.activated_text"));
                return;
            }
            _diplay.SetText(_pageTexts[_currentPage]);
        }

        public void PressRightButton()
        {
            //TODO: this terminal doesnt look like an actual terminal
            if (_chekingIfShure)
            {
                ActivateDestroy();
                _activated = true;
                _chekingIfShure = false;
                UpdateDisplay();
                Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/effects/button_low.ogg", GlobalTransform.origin, this);
                return;
            }

            SetPage(_currentPage + 1);
            UpdateDisplay();
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/effects/button.ogg", GlobalTransform.origin, this);
        }
        public void PressLeftButton()
        {
            if (_chekingIfShure)
            {
                _chekingIfShure = false;
                UpdateDisplay();
                Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/effects/button.ogg", GlobalTransform.origin, this);
                return;
            }

            SetPage(_currentPage - 1);
            UpdateDisplay();
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/effects/button.ogg", GlobalTransform.origin, this);
        }
        public void SetPage(int pageIndex)
        {
            _currentPage = Mathf.Clamp(pageIndex, 0, _pageTexts.Count-1);
        }

        public bool CanActivate()
        {
            return _currentPage == 2 && !_activated;
        }

        public void ActivateDestroy()
        {
            if (!CanActivate()) return;
            Global.Instance.GetGenerationManager().ActionHappened("layer_system_destroy_activated");
        }

        public override Spatial GetUseInfoPoint()
        {
            return _diplay;
        }
    }
}

