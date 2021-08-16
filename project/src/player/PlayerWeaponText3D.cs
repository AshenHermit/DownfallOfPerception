using Godot;
using System;

namespace Game
{
    public class PlayerWeaponText3D : Spatial
    {
        [Export]
        public NodePath WeaponPath;
        Weapon weapon;

        public override void _Ready()
        {
            base._Ready();
            weapon = GetNode<Weapon>(WeaponPath);
            if (!CanUseText3D()) return;
            Global.Instance.GetPlayer().GetText3D().Visible = true;
        }

        public TextIn3D RequestText3D()
        {
            if (!CanUseText3D()) return null;
            return Global.Instance.GetPlayer().GetText3D();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            if (!CanUseText3D()) return;
            Global.Instance.GetPlayer().GetText3D().GlobalTransform = GlobalTransform;
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            if (!CanUseText3D()) return;
            Global.Instance.GetPlayer().GetText3D().Visible = true;
        }
        public override void _ExitTree()
        {
            base._ExitTree();
            if (!CanUseText3D()) return;
            Global.Instance.GetPlayer().GetText3D().Visible = false;
        }

        public bool CanUseText3D()
        {
            return Global.Instance.GetPlayer().GetCurrentWeapon() == weapon;
        }
    }
}
