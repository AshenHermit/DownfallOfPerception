using Godot;
using Godot.Collections;
using System;

namespace Game
{
    public class ScriptedEnemy : Enemy, IScriptedObject
    {
        [Export]
        public GDScript ScriptResource;
        [Export]
        public Dictionary Exports { get; set; }

        public Godot.Object ScriptContext { get; set; }

        public override void _Ready()
        {
            base._Ready();
            this.SetupScriptContext<ScriptedEnemy>(ScriptResource);
            this.CallMethodSafely("_ready");
        }
        public override void _Process(float delta)
        {
            this.CallMethodSafely("_process", delta);
            base._Process(delta);
        }
        public override void _PhysicsProcess(float delta)
        {
            this.CallMethodSafely("_physics_process", delta);
            base._PhysicsProcess(delta);
        }
        public override void Die()
        {
            this.CallMethodSafely("_on_died");
            base.Die();
        }

        public override void IdleState(float delta)
        {
            this.CallMethodSafely("_idle_state", delta);
        }
        public override void WanderingState(float delta)
        {
            this.CallMethodSafely("_wandering_state", delta);
        }
        public override void ChasingTargetState(float delta)
        {
            this.CallMethodSafely("_chasing_target_state", delta);
        }
        public override void AttackingState(float delta)
        {
            this.CallMethodSafely("_attacking_state", delta);
        }

    }
}
