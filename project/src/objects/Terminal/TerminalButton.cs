using Godot;
using System;

namespace Game
{
    public class TerminalButton : Usable
    {
        [Export]
        public NodePath TerminalPath;
        Terminal _terminal;

        public enum TerminalButtonType{ LEFT, RIGHT }
        [Export]
        public TerminalButtonType _type;

        public override void _Ready()
        {
            _terminal = GetNode<Terminal>(TerminalPath);
        }

        public override void Use(Node invoker)
        {
            if (_terminal == null) return;
            if (invoker != Global.Instance.GetPlayer()) return;

            if(_type == TerminalButtonType.LEFT)
            {
                _terminal.PressLeftButton();
            }
            else if(_type == TerminalButtonType.RIGHT)
            {
                _terminal.PressRightButton();
            }
        }

        public override string GetUseText()
        {
            return Global.Translate("use_text.PRESS");
        }
    }
}

