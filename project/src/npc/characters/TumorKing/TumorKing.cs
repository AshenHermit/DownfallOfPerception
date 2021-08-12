using Godot;
using System;

namespace Game
{
    public class TumorKing : TalkingNpc
    {
        AnimationPlayer _animationPlayer;
        float _playbackSpeedTarget = 1.0f;
        SpatialMaterial _material;

        public override void _Ready()
        {
            base._Ready();
            //SetMonologue("last_order");
            _animationPlayer = GetNode(VisualNodePath).GetNode<AnimationPlayer>("AnimationPlayer");
            _animationPlayer.GetAnimation("idle").Loop = true;
            _animationPlayer.Play("idle");
            SetupMaterial();

            //TODO:KLUDGE:
            GetNode<AudioStreamPlayer3D>("Sound").Play();
        }

        void SetupMaterial()
        {
            //TODO: make interface for entities that need to disable their material emission
            MeshInstance mesh = GetNode(VisualNodePath).GetAllChildren<MeshInstance>()[0];
            if (mesh != null)
            {
                _material = (SpatialMaterial)mesh.GetActiveMaterial(0).Duplicate();
                mesh.SetSurfaceMaterial(0, _material);
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            _animationPlayer.PlaybackSpeed += (_playbackSpeedTarget - _animationPlayer.PlaybackSpeed) / 20.0f;

            if (IsAlive())
            {
                Transform trans = GlobalTransform;
                Vector3 euler = trans.LookingAt(Global.Instance.GetPlayer().GlobalTransform.origin, Vector3.Up).basis.GetEuler();
                euler.x = 0.0f;
                euler.z = 0.0f;
                trans.basis = new Basis(new Quat(trans.basis).Slerp(new Quat(euler), delta));
                GlobalTransform = trans;
            }
        }

        public override void Die()
        {
            _playbackSpeedTarget = 0.0f;
            _material.EmissionEnabled = false;
            GetNode<AudioStreamPlayer3D>("Sound").Stop();
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/monologues/tumor_king/death.mp3", GlobalTransform.origin, this);
        }
    }
}
