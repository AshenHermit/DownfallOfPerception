using Godot;
using System;

namespace Game {
    public class VfxManager : Node
    {
        [Export]
        public PackedScene DamageParticles;

        public override void _Ready()
        {
            Global.Instance.RegisterVfxManager(this);
        }

        public void MakeDamageEffectAtObject(Spatial targetObject, float damage)
        {
            Particles particles = DamageParticles.Instance<Particles>();
            GetParent().AddChild(particles);
            particles.GlobalTransform = particles.GlobalTransform.Translated(targetObject.GlobalTransform.origin);
            particles.OneShot = true;
            particles.Emitting = true;
        }
    }
}
