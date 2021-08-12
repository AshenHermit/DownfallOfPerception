using Godot;
using System;

namespace Game.UI
{
    public class NpcState : Control
    {
        [Export]
        public NodePath MainContainerNodePath;
        private VBoxContainer _mainContainer;

        [Export]
        public NodePath HealthPanelNodePath;
        private PanelContainer _healthPanel;

        public Npc ConnectedNpc;

        public void ConnectToEnemy(Npc npc)
        {
            ConnectedNpc = npc;
        }

        public override void _Ready()
        {
            _mainContainer = GetNode<VBoxContainer>(MainContainerNodePath);
            _healthPanel = GetNode<PanelContainer>(HealthPanelNodePath);
        }

        public override void _Process(float delta)
        {
            if (ConnectedNpc != null)
            {
                RectPosition = Global.Instance.GetPlayer().GetCamera().UnprojectPosition(ConnectedNpc.GlobalTransform.origin);
            }
        }
    }
}

