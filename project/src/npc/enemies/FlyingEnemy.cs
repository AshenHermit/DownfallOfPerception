using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class FlyingEnemy : Enemy
    {
        public Vector3 Velocity;
        public Vector3 Movement;
        public Vector3 NoiseMovement;
        float _distanceFromPlayer = 10.0f;

        protected AnimationPlayer _animationPlayer;

        float _randTimer = 1.0f;

        protected float _attackTimer = 1.0f;
        bool _attacked = false;
        protected bool _use_noise = true;

        public override void _Ready()
        {
            base._Ready();
            _distanceFromPlayer = (float)GD.RandRange(6.0f, 12.0f);

            if (_animationPlayer == null)
            {
                _animationPlayer = GetNode(VisualNodePath).GetNode<AnimationPlayer>("AnimationPlayer");
                _animationPlayer.GetAnimation("idle").Loop = true;
            }
            _animationPlayer.Play("idle");
        }

        public override void _PhysicsProcess(float delta)
        {
            Velocity = MoveAndSlide(Velocity, Vector3.Up, false);
            Velocity += Movement;
            Velocity /= 1.2f;
            Movement /= 2.0f;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            if (this.IsAlive())
            {
                if (_use_noise)
                {
                    _randTimer -= delta;
                    if (_randTimer<=0.0f)
                    {
                        NoiseMovement = MathUtils.Randv()*60.0f;
                        _randTimer = 0.2f+GD.Randf();
                    }

                    Movement += (NoiseMovement - Movement) / 100.0f;
                }
            }
        }

        protected void ChasePlayer(float minDistance, float speedFactor=1.0f)
        {
            Vector3 dif = Global.Instance.GetPlayer().GetCamera().GlobalTransform.origin - GlobalTransform.origin;
            Vector3 ndif = dif.Normalized();

            Transform trans = GlobalTransform;
            Quat from = new Quat(GlobalTransform.basis).Normalized();
            Quat to = new Quat(GlobalTransform.LookingAt(GlobalTransform.origin + ndif
                + Movement * Mathf.Min(Mathf.Max(dif.Length() - minDistance, 0.5f), 14.0f) / 14.0f * 6.0f, Vector3.Up).basis).Normalized();

            trans.basis = new Basis(from.Slerp(to, 0.05f*(speedFactor*speedFactor)));

            GlobalTransform = trans;

            Movement += ndif * 0.6f * speedFactor;
            Movement -= ndif * (Mathf.Max(minDistance - dif.Length(), 0.0f)) * 0.4f;
        }

        protected float GetDistanceToPlayer()
        {
            return Global.Instance.GetPlayer().GetCamera().GlobalTransform.origin.DistanceTo(GlobalTransform.origin);
        }

        public override void IdleState(float delta)
        {
            base.IdleState(delta);
            SetState((int)StateEnum.CHASING_TARGET);
        }

        public override void ChasingTargetState(float delta)
        {
            base.ChasingTargetState(delta);
            ChasePlayer(_distanceFromPlayer);

            if (GetDistanceToPlayer() < _distanceFromPlayer+2.0f)
            {
                _attackTimer -= delta;
                if (_attackTimer<=0.0f)
                {
                    _attackTimer = 3.0f+GD.Randf()*9.0f;
                    SetState((int)StateEnum.ATTACKING);
                }
            }
        }
        public override void AttackingState(float delta)
        {
            base.AttackingState(delta);
            ChasePlayer(6.0f, 2.0f);

            if (_animationPlayer.CurrentAnimation == "idle")
            {
                if (GetDistanceToPlayer() < 6.0f)
                {
                    _animationPlayer.Play("attack");
                    _attacked = false;
                }
            }

            if (_animationPlayer.CurrentAnimation == "attack")
            {
                if (_animationPlayer.IsPlaying() && _animationPlayer.CurrentAnimationPosition > AttackFrame && _animationPlayer.CurrentAnimationPosition < AttackFrame+0.2f && !_attacked)
                {
                    if (GetDistanceToPlayer() < 7.0f)
                    {
                        Global.Instance.GetPlayer().TakeDamage(Damage);
                        _attacked = true;
                    }
                }
            }
            if (!_animationPlayer.IsPlaying())
            {
                _animationPlayer.Play("idle");
            }
            _attackTimer -= delta;
            if (_attackTimer <= 0.0f)
            {
                SetState((int)StateEnum.CHASING_TARGET);
                _animationPlayer.Play("idle");
                _attackTimer = 2.0f + GD.Randf() * 10.0f;
            }
        }
        public override void Die()
        {
            base.Die();
            _animationPlayer.PlaybackSpeed = 0.0f;
            EnemyCorpse body = TurnIntoCorpse(Velocity);
            Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition("creeping_enemy_death", body.GlobalTransform.origin, body);
        }
    }
}
