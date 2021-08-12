using Godot;
using Godot.Collections;
using System;

namespace Game
{
    class ScriptedWeapon : Weapon, IScriptedObject
    {
        [Export]
        public GDScript ScriptResource;
        [Export]
        public Dictionary Exports { get; set; }
        
        public Godot.Object ScriptContext { get; set; }

        public override void _Ready()
        {
            base._Ready();
            this.SetupScriptContext<ScriptedWeapon>(ScriptResource);
            ScriptContext.Set("item_properties", Item.Properties);
            this.CallMethodSafely("_ready");
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            this.CallMethodSafely("_process", delta);
        }

        public override void MakeShot(bool primary)
        {
            base.MakeShot(primary);
            this.CallMethodSafely("_make_shot", primary);
        }
        public override void Fire(bool primary = true)
        {
            base.Fire(primary);
            this.CallMethodSafely("_on_started_firing", primary);
        }
        public override void StopFiring()
        {
            base.StopFiring();
            this.CallMethodSafely("_on_stopped_firing");
        }
    }
}
