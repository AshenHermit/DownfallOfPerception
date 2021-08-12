using Godot;
using System;

namespace Game
{
    public class CreepingEnemy : Enemy
    {
        [Export]
        public NodePath RayCastPath;
        RayCast _rayCast;

        Vector3 upDirection = Vector3.Up;
        public Vector3 Velocity;
        public Vector3 Movement;

        public float _speed = 1.0f;

        float _randTimer = 1.0f;

        bool _jumping = false;

        float _attackTimer = 1.0f;
        float _rotationSpeed = 0.05f;
        bool _attacked = false;

        static float _attackDistance = 6.0f;

        public AnimationPlayer _animationPlayer;

        int stepCounter;
        

        public override void _Ready()
        {
            base._Ready();
            _rayCast = GetNode<RayCast>(RayCastPath);
            upDirection = GlobalTransform.basis.y;
            if (_animationPlayer == null)
            {
                _animationPlayer = GetNode(VisualNodePath).GetNode<AnimationPlayer>("AnimationPlayer");
                //_animationPlayer.AddAnimation("moving", GD.Load<Animation>("res://game_objects/Enemies/CreepingEnemy/scavengermoving.tres"));
                _animationPlayer.GetAnimation("moving").Loop = true;
            }

            //TODO: dead code
            //int track_id = _animationPlayer.GetAnimation("moving").AddTrack(Animation.TrackType.Method);
            //GD.Print(track_id);
            //_animationPlayer.GetAnimation("moving").TrackInsertKey(
            //    track_id, 0.0f, new Godot.Collections.Dictionary<string, object> { { "method", "play_sound_from_bank" }, { "args", new object[] { "creeping_enemy_step" } } });
            //_animationPlayer.GetAnimation("moving").TrackInsertKey(
            //    track_id, 0.4f, new Godot.Collections.Dictionary<string, object> { { "method", "play_sound_from_bank" }, { "args", new object[] { "creeping_enemy_step" } } });

            _animationPlayer.Play("moving");
        }

        public override void DefferedSetup()
        {
            upDirection = GlobalTransform.basis.y;
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            Velocity = MoveAndSlide(Velocity + Movement, upDirection, false);
            if (IsOnFloor())
            {
                Velocity = -upDirection;
                Movement /= 1.2f;
            }
            else
            {
                Velocity -= upDirection;
                Movement /= 40.0f;
            }
        }

        void PlayStepSound()
        {
            Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition("creeping_enemy_step", GlobalTransform.origin);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            if (this.IsAlive())
            {
                if (!_jumping) { 
                    if (_rayCast.IsColliding())
                    {
                        upDirection = _rayCast.GetCollisionNormal();
                    }
                    RotateAlongUpDirection(_rotationSpeed);
                }
                else
                {
                    upDirection = -upDirection;
                    RotateAlongUpDirection(0.5f);
                    _jumping = false;
                    _animationPlayer.PlaybackSpeed += (0.0f - _animationPlayer.PlaybackSpeed) / 10.0f;
                }
            }
        }

        void RotateAlongUpDirection(float rotateWeight = 0.05f)
        {
            Transform trans = GlobalTransform;
            trans.origin = Vector3.Zero;
            //TODO: move this in some utiliy to use in other places
            trans = trans.LookingAt(trans.origin + upDirection, Movement.Normalized());
            trans = trans.Rotated(trans.basis.x, -Mathf.Pi / 2.0f);
            GlobalTransform = new Transform(new Basis(new Quat(GlobalTransform.basis).Slerp(new Quat(trans.basis).Normalized(), rotateWeight)), GlobalTransform.origin);
        }

        void ChasePlayer(float speed)
        {
            Plane plane = new Plane(upDirection, upDirection.Dot(GlobalTransform.origin));
            Vector3 target = Global.Instance.GetPlayer().GetCamera().GlobalTransform.origin-Vector3.Up*4.0f;
            Vector3 point = plane.Project(target);
            Vector3 dir = (point - GlobalTransform.origin);
            if (dir.Length() >= 6.0f)
            {
                Movement += dir.Normalized() * speed;
            }
            else if (dir.Length() >= 1.0f)
            {
                Movement += dir.Normalized() * speed/40.0f;
            }
            if (dir.Length() < 1.0f && GD.Randf()<0.05f)
            {
                if (GlobalTransform.origin.DistanceTo(target) < 50.0f)
                {
                    if (upDirection.Dot((target - GlobalTransform.origin).Normalized())>0.8f)
                    {
                        Jump();
                    }
                }
            }
        }

        public void Jump()
        {
            _jumping = true;
        }
        float GetDistanceToPlayer()
        {
            return Global.Instance.GetPlayer().GetCamera().GlobalTransform.origin.DistanceTo(GlobalTransform.origin);
        }
        float GetDirectionToPlayerFactor()
        {
            return GlobalTransform.basis.z.Dot((Global.Instance.GetPlayer().GetCamera().GlobalTransform.origin-GlobalTransform.origin).Normalized());
        }

        public override void IdleState(float delta)
        {
            base.IdleState(delta);
            SetState((int)StateEnum.CHASING_TARGET);
        }

        public override void ChasingTargetState(float delta)
        {
            base.ChasingTargetState(delta);
            _rotationSpeed = 0.05f;
            ChasePlayer(_speed);
            _animationPlayer.PlaybackSpeed += (Movement.Length() / 8.0f - _animationPlayer.PlaybackSpeed) / 10.0f;

            if (GetDistanceToPlayer() < _attackDistance+2.0f)
            {
                _attackTimer -= delta;
                if (_attackTimer <= 0.0f)
                {
                    _attackTimer = 2.0f + GD.Randf() * 8.0f;
                    SetState((int)StateEnum.ATTACKING);
                }
            }
        }


        //TODO: why this is using same code as in FlyingEnemy, fix it
        // its also very dirty
        public override void AttackingState(float delta)
        {
            base.AttackingState(delta);
            _rotationSpeed = 0.2f;
            _animationPlayer.PlaybackSpeed = 1.0f;

            if (_animationPlayer.CurrentAnimation == "moving")
            {
                if (GetDistanceToPlayer() < _attackDistance)
                {
                    Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition("creeping_enemy_attack", GlobalTransform.origin);
                    _animationPlayer.Play("attack");
                    _attacked = false;
                }
            }

            if (_animationPlayer.CurrentAnimation == "attack")
            {
                if (!_attacked)
                {
                    ChasePlayer(2.0f);
                }
                else
                {
                    ChasePlayer(0.0f);
                }

                if (_animationPlayer.IsPlaying() 
                    && _animationPlayer.CurrentAnimationPosition > AttackFrame 
                    && _animationPlayer.CurrentAnimationPosition < AttackFrame + 0.2f 
                    && !_attacked)
                {
                    if (GetDistanceToPlayer() < _attackDistance && GetDirectionToPlayerFactor()>0.7f)
                    {
                        if (Global.Instance.GetPlayer().GetHealth() - Damage < 0.0f) Global.Instance.GetPlayer().Velocity += GlobalTransform.basis.z * 80.0f;
                        else Global.Instance.GetPlayer().Velocity += GlobalTransform.basis.z*6.0f;
                        Global.Instance.GetPlayer().TakeDamage(Damage);
                        _attacked = true;
                    }
                }
            }
            else
            {
                ChasePlayer(_speed);
            }
            if (!_animationPlayer.IsPlaying())
            {
                _animationPlayer.Play("moving");
            }
            _attackTimer -= delta;
            if (_attackTimer <= 0.0f)
            {
                SetState((int)StateEnum.CHASING_TARGET);
                _animationPlayer.Play("moving");
                _attackTimer = 2.0f + GD.Randf() * 6.0f;
            }
        }

        public override void Die()
        {
            _animationPlayer.PlaybackSpeed = 0.0f;
            EnemyCorpse body = TurnIntoCorpse(Velocity);
            Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition("creeping_enemy_death", body.GlobalTransform.origin, body);
        }
    }
}
