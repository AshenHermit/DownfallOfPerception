using Godot;
using Game.Utils;
using System;

namespace Game
{
    public class Enemy : Npc, IPoolingInstance
    {
        [Export]
        public float AttackFrame = 0.5f;
        [Export]
        public float Damage = 5.0f;

        [Export]
        public Godot.Collections.Array<Godot.Collections.Dictionary> ItemsToDrop = null;

        protected bool _removeWhenFarFromPlayer = true;

        public IPoolContainer PoolContainer { get; set; }

        public enum StateEnum
        {
            IDLE, WANDERING, CHASING_TARGET, ATTACKING
        }

        // constructor
        public Enemy() : base()
        {
            RegisterState((int)StateEnum.IDLE, IdleState);
            RegisterState((int)StateEnum.WANDERING, WanderingState);
            RegisterState((int)StateEnum.CHASING_TARGET, ChasingTargetState);
            RegisterState((int)StateEnum.ATTACKING, AttackingState);
        }

        public virtual void Pooled()
        {
            _Ready();
        }

        // states
        public virtual void IdleState(float delta)
        {
            
        }
        public virtual void WanderingState(float delta)
        {
            
        }
        public virtual void ChasingTargetState(float delta)
        {

        }
        public virtual void AttackingState(float delta)
        {

        }

        public override void _Process(float delta)
        {
            if (Global.Instance.GetNpcManager().IsSomebodyTalking())
            {
                SetState((int)StateEnum.IDLE);
            }
            base._Process(delta);
            if (_removeWhenFarFromPlayer && Global.Instance.GetPlayerCamera().GlobalTransform.origin.DistanceTo(GlobalTransform.origin)>=110.0f)
            {
                QueueFree();
                //this.FreeInPool();
            }
        }

        // methods

        public EnemyCorpse TurnIntoCorpse(Vector3 velocity)
        {
            EnemyCorpse body = new EnemyCorpse();
            Global.Instance.CurrentSceneInstance.AddChild(body);
            if(ItemsToDrop!=null) body.SetItemToDrop(ItemsToDrop.GetRandomElement());
            body.Mass = 10;
            body.GlobalTransform = GlobalTransform;
            GetNode<Spatial>(VisualNodePath).Reparent(body);
            GetNode<Spatial>(CollisionShapePath).Reparent(body);
            body.LinearVelocity = velocity;

            QueueFree();
            //this.FreeInPool();
            return body;
        }
    }
}
