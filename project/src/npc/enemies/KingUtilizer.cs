using Godot;
using System;

namespace Game
{
    //TODO: this whole enemy is one big kludge
    public class KingUtilizer : FlyingEnemy
    {
        public bool PlayerControlling;

        public SpatialMaterial tumorMaterial;

        float controllingHealthTreshold = 40.0f;
        public float Speed = 1.6f;

        public override void _Ready()
        {
            base._Ready();
            _use_noise = false;
            SetUsableState(true);
            _attackTimer = 0.0f;
            _removeWhenFarFromPlayer = false;

            foreach(MeshInstance mesh in this.GetAllChildren<MeshInstance>())
            {
                if(mesh.Name == "Icosphere")
                {
                    tumorMaterial = (SpatialMaterial)mesh.GetActiveMaterial(0).Duplicate();
                    mesh.SetSurfaceMaterial(0, tumorMaterial);
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            Velocity = MoveAndSlide(Velocity, Vector3.Up, false);
            Velocity += Movement;
            Velocity /= 1.05f;
            Movement /= 2.0f;
        }
        public override void _Process(float delta)
        {
            if (Input.IsActionJustReleased("use") || Input.IsActionJustPressed("jump"))
            {
                StopPlayerControlling();
            }
            base._Process(delta);
        }

        public override void ChasingTargetState(float delta)
        {
            SetState((int)StateEnum.ATTACKING);
        }

        void StopPlayerControlling()
        {
            PlayerControlling = false;
            Global.Instance.GetPlayer().CollisionLayer = 1;
            Global.Instance.GetPlayer().CollisionMask = 1;
        }
        public override void Die()
        {
            StopPlayerControlling();
            _animationPlayer.PlaybackSpeed = 0.0f;
            EnemyCorpse body = TurnIntoCorpse(Velocity);
            Global.Instance.GetAudioManager().PlaySoundAtPosition(
                "res://resources/sounds/monsters/king_utilizer/king_utilizer_death.mp3",
                body.GlobalTransform.origin, body);
            Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition("creeping_enemy_death", body.GlobalTransform.origin, body);
            Health = 0.0f;
        }

        void FlyUnderControl(float delta, float speedFactor)
        {
            Vector3 dir = -Global.Instance.GetPlayer().GetCamera().GlobalTransform.basis.z;
            dir.y /= 4.0f;
            dir = dir.Normalized();

            Transform trans = GlobalTransform;
            Quat from = new Quat(GlobalTransform.basis).Normalized();
            Quat to = new Quat(GlobalTransform.LookingAt(GlobalTransform.origin + dir, Vector3.Up).basis).Normalized();

            trans.basis = new Basis(from.Slerp(to, 0.05f * (speedFactor * speedFactor)));

            GlobalTransform = trans;

            float factor = 1.5f * (Mathf.Pow(Global.Instance.GetPlayer().SpeedFactor, 2.0f));
            Movement += -GlobalTransform.basis.z * factor * speedFactor;

            trans = Global.Instance.GetPlayer().GlobalTransform;
            trans.origin = GlobalTransform.origin + GlobalTransform.basis.y * 2.0f;
            Global.Instance.GetPlayer().GlobalTransform = trans;
            Global.Instance.GetPlayer().Velocity = Vector3.Zero;
        }

        public override void AttackingState(float delta)
        {
            if (_attackTimer > 0.0f) _attackTimer -= delta;
            if (PlayerControlling)
            {
                FlyUnderControl(delta, 1.2f);
            }
            else
            {
                ChasePlayer(0.0f, Speed);

                if (_attackTimer <= 0.0f)
                {
                    if(GetDistanceToPlayer() < 8.0f)
                    {
                        Global.Instance.GetPlayer().TakeDamage(Damage*GD.Randf());
                        if (GD.Randf() < 0.8f)
                        {
                            Global.Instance.GetPlayer().Velocity = (GlobalTransform.basis.z * 20.0f + GlobalTransform.basis.y * 100.0f);
                        }
                        _attackTimer = 0.1f;
                    }
                }
            }
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);

            if (GetHealth() < controllingHealthTreshold)
            {
                tumorMaterial.EmissionEnergy = 50.0f;
                tumorMaterial.EmissionOperator = SpatialMaterial.EmissionOperatorEnum.Multiply;
                tumorMaterial.Emission = new Color("00ffc9");
            }
        }

        public override void Use(Node invoker)
        {
            if (GetHealth() < controllingHealthTreshold)
            {
                PlayerControlling = true;
                Global.Instance.GetPlayer().CollisionLayer = 0;
                Global.Instance.GetPlayer().CollisionMask = 0;
            }
        }
    }
}
