using Godot;
using System;

namespace Game
{
    public class Fungus : TalkingNpc, IChargingBattery
    {
        AnimationPlayer _animationPlayer;
        float _animationPlaybackSpeedTarget = 1.0f;
        Skeleton _skeleton;

        AudioStreamPlayer3D _soundPlayer;

        public override void _Ready()
        {
            _soundPlayer = GetNode<AudioStreamPlayer3D>("Sound");
            _animationPlayer = GetNode(VisualNodePath).GetNode<AnimationPlayer>("AnimationPlayer");
            _skeleton = GetNode(VisualNodePath).GetNode<Skeleton>("Armature/Skeleton");
            _animationPlayer.GetAnimation("idle").Loop = true;
            _animationPlaybackSpeedTarget = 1.4f;
            _soundPlayer.Play(_soundPlayer.Stream.GetLength() * GD.Randf());
            _animationPlayer.Play("idle");

            base._Ready();

            //TODO: move this in utility
            _animationPlayer.Seek(GD.Randf() * _animationPlayer.CurrentAnimationLength);
            if (GetHealth() <= 0.0f) Die();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            _animationPlayer.PlaybackSpeed += (_animationPlaybackSpeedTarget - _animationPlayer.PlaybackSpeed)/10.0f;
        }

        public override void TakeDamage(float damage)
        {
            
        }

        public override void Die()
        {
            base.Die();
            _animationPlaybackSpeedTarget = 0.0f;
            _soundPlayer.Stop();
            CollisionLayer = 0;
            CollisionMask = 0;
        }

        // battery
        public void StartChargingBattery(ChargeableBattery battery, Transform batteryTransform)
        {
            battery.SetChargingEntity(this);
            battery.ChargingSpeed = 0.25f * (0.05f + GD.Randf());
            BoneAttachment attachment = new BoneAttachment();
            float minDist = 9999.0f;
            for (int i = 0; i < _skeleton.GetBoneCount(); ++i)
            {
                Vector3 pos = _skeleton.ToGlobal(_skeleton.GetBoneGlobalPose(i).origin);
                float dist = pos.DistanceTo(batteryTransform.origin);
                if (dist < minDist)
                {
                    minDist = dist;
                    attachment.BoneName = _skeleton.GetBoneName(i);
                }
            }
            _skeleton.AddChild(attachment);
            attachment.AddChild(battery);
            battery.GlobalTransform = batteryTransform;
            battery.Scale = Vector3.One;
            _animationPlaybackSpeedTarget = 3.0f;
            _soundPlayer.PitchScale = 0.6f;
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/monsters/fungus/battery_thrust_in.ogg", _soundPlayer.GlobalTransform.origin, this);
        }
        public void OnBatteryCharged(ChargeableBattery battery)
        {
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/monsters/fungus/electro_death.ogg", _soundPlayer.GlobalTransform.origin, this);
            Die();
        }

        public void StopChargingBattery(ChargeableBattery battery)
        {
            if (!IsAlive()) return;
            _animationPlaybackSpeedTarget = 1.0f;
            _soundPlayer.PitchScale = 1.0f;
        }
    }
}
