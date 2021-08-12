using Godot;
using System;


namespace Game
{
    public class Radio : TalkingNpc
    {
        public override void _Ready()
        {
            base._Ready();
            SetMonologue("stay_still");
            Talk();
        }
    }
}
