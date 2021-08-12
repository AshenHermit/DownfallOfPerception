using Godot;
using System;

namespace Game
{
    public class Npc : KinematicBody, IEntity, IUsable
    {
        // fields
        [Export]
        public float MaxHealth { get; set; }
        public float Health { get; set; }
        public bool Damageable { get; set; }

        [Export]
        public NodePath VisualNodePath;
        [Export]
        public NodePath CollisionShapePath;

        bool _usable = false;
        bool _died = false;

        public delegate void StateHandler(float delta);
        private int _currentState = 0;
        System.Collections.Generic.List<StateHandler> _stateHandlers = new System.Collections.Generic.List<StateHandler>();

        /// <summary>
        /// Called when npc has spawned. Needs to add some logic after spawning, for example check CreepingEnemy.
        /// </summary>
        /// <param name="globalTransform"></param>
        public virtual void DefferedSetup()
        {
            
        }

        // methods
        public Npc()
        {
            //Damageable = true;
            //SetMaxHealth(MaxHealth);
            //SetHealth(MaxHealth);
        }
        public override void _Ready()
        {
            Damageable = true;
            _died = false;
            SetMaxHealth(MaxHealth);
            SetHealth(MaxHealth);
            CallDeferred("DefferedSetup");
        }
        public override void _Process(float delta)
        {
            if (Health > 0.0f)
            {
                if (_stateHandlers.Count == 0) return;
                if (_stateHandlers[_currentState] != null)
                {
                    _stateHandlers[_currentState](delta);
                }
            }
        }

        public override void _ExitTree()
        {
            _stateHandlers.Clear();
        }

        /// <summary>
        /// Sets current state of npc state machine
        /// </summary>
        /// <param name="state">you can cast enum to int</param>
        public void SetState(int state)
        {
            _currentState = state;
        }
        /// <summary>
        /// Get current state of npc state machine
        /// </summary>
        public int GetCurrentState()
        {
            return _currentState;
        }
        /// <summary>
        /// Registers method as state process function, so it will be calling like _process, but only when connected state is current.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stateHandler"></param>
        public void RegisterState(int state, StateHandler stateHandler)
        {
            for(int i=0; i<Math.Max(0, state+1-_stateHandlers.Count); ++i)
            {
                _stateHandlers.Add(null);
            }
            _stateHandlers[state] = stateHandler;
        }

        /// <summary>
        /// Sets health to zero.
        /// </summary>
        public virtual void Die()
        {
            Health = 0;
        }
        /// <summary>
        /// calls Die method if health <= 0
        /// </summary>
        void DieIfNotAlive()
        {
            if (Health <= 0)
            {
                if (!_died)
                {
                    Die();
                }
                _died = true;
            }
        }

        // IEntity implementation
        public virtual void TakeDamage(float damage)
        {
            if (!Damageable) return;
            if (damage > 0.0f)
            {
                Health -= damage;
                DieIfNotAlive();
                Global.Instance.GetVfxManager().MakeDamageEffectAtObject(this, damage);
                Global.Instance.GetNpcManager().NpcTookDamage(this, damage);
            }
        }
        public void SetHealth(float hp)
        {
            Health = hp;
            DieIfNotAlive();
        }
        public float GetHealth()
        {
            return Health;
        }
        public void SetMaxHealth(float maxHealth)
        {
            MaxHealth = maxHealth;
        }
        public float GetMaxHealth()
        {
            return MaxHealth;
        }
        public bool IsAlive()
        {
            return GetHealth() > 0.0f;
        }

        // IUsable implementation
        public virtual void Use(Node invoker)
        {
            
        }
        public bool IsUsable()
        {
            return _usable;
        }
        public void SetUsableState(bool usable)
        {
            _usable = usable;
        }

        public virtual string GetUseText()
        {
            return "";
        }

        public virtual Spatial GetUseInfoPoint()
        {
            return this;
        }
    }
}
