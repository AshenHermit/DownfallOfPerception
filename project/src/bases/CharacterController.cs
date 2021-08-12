using Godot;
using System;

namespace Game
{
    public class CharacterController : Godot.KinematicBody
    {
        [Export]
        public float MaxSpeed = 0.2f;
        [Export]
        public float Mass = 10.0f;

        public Vector3 Velocity;
        Vector3 _movement;
        bool _isGrounded;
        bool _preventIsGrounded;
        public float PreventVelocityY;

        public float SpeedFactor = 1.0f;

        public delegate void OnGroundedHandler(Godot.Collections.Dictionary collision);
        public delegate void OnUngroundedHandler();
        public event OnGroundedHandler OnGroundedEvent;
        public event OnUngroundedHandler OnUngroundedEvent;
        public bool Disabled = false;

        public bool CanFly = false;

        public CharacterController()
        {
            OnGroundedEvent += OnGrounded;
            OnUngroundedEvent += OnUngrounded;
        }

        public override void _Ready()
        {

        }

        public override void _Process(float delta)
        {

        }
        public override void _PhysicsProcess(float delta)
        {
            if (!Disabled)
            {
                ProcessVelocity(delta);
                Fall();
                UpdateGroundedState();
            }
        }

        public void Move(Vector3 direction)
        {
            _movement += direction * 6.0f;
        }
        public void Jump(float strength)
        {
            Velocity.y = strength;
        }

        public bool IsGrounded() { return _isGrounded; }

        void OnGrounded(Godot.Collections.Dictionary collision)
        {
            _isGrounded = true;
        }
        void OnUngrounded()
        {
            _isGrounded = false;
        }

        void Fall()
        {
            if (IsOnFloor())
            {
                //Velocity.y = -Mass;
            }
            else
            {
                Velocity.y -= Mass;
            }
        }

        void UpdateGroundedState()
        {
            Godot.Collections.Dictionary collision = GetWorld().DirectSpaceState.IntersectRay(
                GlobalTransform.origin + new Vector3(0.0f, 1.0f, 0.0f),
                GlobalTransform.origin - new Vector3(0.0f, 0.5f, 0.0f),
                new Godot.Collections.Array { this });

            if (collision.Count > 0)
            {
                if (!_preventIsGrounded)
                {
                    OnGroundedEvent.Invoke(collision);
                }
            }
            else
            {
                if (_preventIsGrounded)
                    OnUngroundedEvent.Invoke();
            }
            _preventIsGrounded = _isGrounded;
            PreventVelocityY = Velocity.y;
        }

        void ProcessVelocity(float delta)
        {
            if (!Disabled)
            {
                Velocity = MoveAndSlide(Velocity * 1.0f, Vector3.Up, true, 4, 0.785f);

                //TODO: looks stupid
                if (IsGrounded() || CanFly)
                {
                    Velocity += _movement * MaxSpeed*1.2f* SpeedFactor;
                    Velocity.x /= 1.4f;
                    Velocity.z /= 1.4f;
                }
                else
                {
                    Velocity += _movement * MaxSpeed * 1.2f * 0.05f * SpeedFactor;
                    Velocity.x /= 1.01f;
                    Velocity.z /= 1.01f;
                }
                _movement = Vector3.Zero;
            }
        }
    }
}