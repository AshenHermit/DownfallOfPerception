using Godot;
using System;

namespace Game.UI
{
    public class InfoPopupDriver : Container
    {
        [Export]
        public PackedScene InfoPopupScene;

        public override void _Ready()
        {
            Global.Instance.OnInfoPopup += OnInfoPopup;
        }

        public override void _Process(float delta)
        {
            RectPosition = new Vector2(-RectSize.x/2.0f, RectPosition.y);
        }

        public void OnInfoPopup(string message)
        {
            InfoPopup infoPopup = InfoPopupScene.Instance<InfoPopup>();
            AddChild(infoPopup);
            infoPopup.Start(message);
        }
    }
}
