using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class TumorContainer : Npc
    {
        [Export]
        public Godot.Collections.Array<PackedScene> ScenesToSpawn;
        [Export]
        public NodePath SpawnPointPath;
        [Export]
        public int ItemAmount = -1;

        AnimationPlayer _animationPlayer;
        CollisionShape _collisionShape;
        Spatial _spawnPoint;

        float _timer = 0.0f;
        float _damageCounter = 0.0f;
        bool _spawned;
        bool _used = false;

        public override void _Ready()
        {
            _animationPlayer = GetNode(VisualNodePath).GetNode<AnimationPlayer>("AnimationPlayer");
            _collisionShape = GetNode<CollisionShape>(CollisionShapePath);
            _spawnPoint = GetNode<Spatial>(SpawnPointPath);

            _animationPlayer.PlayBackwards("open");
        }

        async public override void TakeDamage(float damage)
        {
            Global.Instance.GetVfxManager().MakeDamageEffectAtObject(this, damage);

            if(!_animationPlayer.IsPlaying()) _damageCounter += damage;
            if (!_used)
            {
                if (_damageCounter > 6.0f)
                {
                    Global.Instance.GetAudioManager().PlaySoundAtPosition(
                        "res://resources/sounds/containers/tumor_container.ogg", _spawnPoint.GlobalTransform.origin, this);
                    _used = true;
                    _animationPlayer.Play("open");
                    GetNode<CollisionShape>(CollisionShapePath).Disabled = true;
                    await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
                    GetNode<CollisionShape>(CollisionShapePath).Disabled = false;
                }
            }
        }

        void SpawnItem()
        {
            Spatial instance = ((Godot.Collections.Array)ScenesToSpawn).GetRandomElement<PackedScene>().Instance<Spatial>();
            Global.Instance.CurrentSceneInstance.AddChild(instance);
            instance.GlobalTransform = _spawnPoint.GlobalTransform;
            if (instance is PickableItem && ItemAmount!=-1)
            {
                ((PickableItem)instance).Amount = ItemAmount;
            }
            if (instance.HasMethod("apply_impulse"))
            {
                instance.CallDeferred("apply_impulse", Vector3.Zero, _spawnPoint.GlobalTransform.basis.z * 20.0f);
            }
        }

        public override void _Process(float delta)
        {
            if (_used)
            {
                if (!_spawned)
                {
                    if (_animationPlayer.IsPlaying() && _animationPlayer.CurrentAnimationPosition >= 0.1)
                    {
                        SpawnItem();
                        _spawned = true;
                    }
                }
                else
                {
                    if (_damageCounter > 0.0f)
                    {
                        _damageCounter -= delta*4.0f;
                        if (_damageCounter<0.0f)
                        {
                            _damageCounter = 0.0f;
                            _animationPlayer.PlayBackwards("open");
                        }
                    }
                    if (!_animationPlayer.IsPlaying() && _damageCounter<=0.0f)
                    {
                        GetNode<CollisionShape>(CollisionShapePath).Disabled = false;
                        _used = false;
                        _spawned = false;
                        Global.Instance.GetAudioManager().PlaySoundAtPosition(
                        "res://resources/sounds/containers/tumor_container.ogg", _spawnPoint.GlobalTransform.origin, this);
                    }
                }
            }
        }
    }
}
