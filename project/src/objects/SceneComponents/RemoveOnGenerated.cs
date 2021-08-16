using Godot;
using System;


namespace Game
{
    public class RemoveOnGenerated : Spatial
    {
        [Export]
        public bool Invert  = false;

        public override void _Ready()
        {
            if (Invert) Visible = false;

            if (Global.Instance.GetGenerationManager() != null)
            {
                Global.Instance.GetGenerationManager().OnStartAreaGenerated += OnGenerated;
            }
        }

        public override void _ExitTree()
        {
            Global.Instance.GetGenerationManager().OnStartAreaGenerated -= OnGenerated;
        }

        public void OnGenerated()
        {
            if (Invert)
            {
                Visible = true;
            }
            else
            {
                QueueFree();
            }
        }
    }
}
