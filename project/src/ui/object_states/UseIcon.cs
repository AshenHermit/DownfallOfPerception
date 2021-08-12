using Godot;
using System;

namespace Game.UI
{
    public class UseIcon : Control
    {
        [Export]
        public NodePath LabelPath;
        Label _label;

        Spatial _connectedObject;
        Spatial _attachPoint;

        public override void _Ready()
        {
            _label = GetNode<Label>(LabelPath);
            Global.Instance.GetPlayer().OnObjectToUseChanged += OnObjectToUseChanged;
        }

        public override void _Process(float delta)
        {
            if (_connectedObject!=null && _attachPoint!=null && IsInstanceValid(_connectedObject) && IsInstanceValid(_attachPoint))
            {
                if (((IUsable)_connectedObject).IsUsable())
                {
                    if (!Global.Instance.GetPlayer().GetCamera().IsPositionBehind(_attachPoint.GlobalTransform.origin))
                    {
                        RectGlobalPosition = Global.Instance.GetPlayer().GetCamera().UnprojectPosition(_attachPoint.GlobalTransform.origin);
                        Show();
                        return;
                    }
                }
            }
            Hide();
        }
         
        public void OnObjectToUseChanged(Spatial objectToUse)
        {
            _connectedObject = objectToUse;
            _attachPoint = _connectedObject;
            if (objectToUse is IUsable)
            {
                _label.Text = ((IUsable)objectToUse).GetUseText();
                _attachPoint = ((IUsable)objectToUse).GetUseInfoPoint();
            }
            _label.MarginLeft = -_label.RectSize.x / 2.0f;
        }
    }
}
