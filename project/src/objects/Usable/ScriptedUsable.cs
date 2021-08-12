using Godot;
using Godot.Collections;
using System;

namespace Game
{
    class ScriptedUsable : Usable, IScriptedObject
    {
        [Export]
        public GDScript ScriptResource;
        [Export]
        public Dictionary Exports { get; set; }

        public Godot.Object ScriptContext { get; set; }

        public override void _Ready()
        {
            this.SetupScriptContext<ScriptedUsable>(ScriptResource);
            ScriptContext.Set("use_text", base.GetUseText());
            this.CallMethodSafely("_ready");
        }
        public override void _Process(float delta)
        {
            this.CallMethodSafely("_process", delta);
        }

        public override void Use(Node invoker)
        {
            this.CallMethodSafely("_on_use", invoker);
        }

        public override string GetUseText()
        {
            return (string)ScriptContext.Get("use_text");
        }
    }
}
