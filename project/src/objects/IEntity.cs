using Godot;
using System;

namespace Game
{
    public interface IEntity
    {
        float Health { get; set; }
        float MaxHealth { get; set; }
        bool Damageable { get; set; }

        void TakeDamage(float damage);
        void SetHealth(float hp);
        float GetHealth();
        void SetMaxHealth(float maxHealth);
        float GetMaxHealth();
    }

    public static class DamageableExtensions
    {

        public static bool IsAlive(this IEntity entity)
        {
            return entity.Health > 0.0f;
        }
        public static bool IsDamageable(this IEntity entity)
        {
            return entity.Damageable;
        }
    }
}
