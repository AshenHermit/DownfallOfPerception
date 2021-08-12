using Godot;
using System;

namespace Game
{
    public class FakeTeleport : Usable
    {
        public override void _Ready()
        {
            base._Ready();

            if (GetChild(0).HasNode("AnimationPlayer"))
            {
                GetChild(0).GetNode<AnimationPlayer>("AnimationPlayer").GetAnimation("working").Loop = true;
                GetChild(0).GetNode<AnimationPlayer>("AnimationPlayer").Play("working");
            }
        }
        public override void Use(Node invoker)
        {
            Set("mode", RigidBody.ModeEnum.Rigid);
            GetChild(0).GetNode<AnimationPlayer>("AnimationPlayer").Stop(false);
            Global.Instance.GetGenerationManager().ActionHappened("player_used_fake_teleport");
            SetUsableState(false);
        }
        public override string GetUseText()
        {
            return Global.Translate("use_text.GO_HOME");
        }
    }
}
