using Godot;
using Godot.Collections;
using System;

namespace Game
{
    public class EnemySpawner : Spatial, IPoolContainer
    {
        [Export]
        public NodePath SpawnPointPath;
        Spatial _spawnPoint;

        [Export]
        public float SpawnRate = 50.0f;
        [Export]
        public float SpawnChance = 0.5f;
        [Export]
        public PackedScene EnemyScene;

        float workingDistance = 70.0f;

        public bool Enabled = true;

        float _timer;

        public System.Collections.Generic.List<IPoolingInstance> InstancesInPool { get; set; }
        public System.Collections.Generic.List<IPoolingInstance> ActiveInstances { get; set; }
        public System.Collections.Generic.List<IPoolingInstance> InactiveInstances { get; set; }

        public EnemySpawner()
        {
            InstancesInPool = new System.Collections.Generic.List<IPoolingInstance>();
            ActiveInstances = new System.Collections.Generic.List<IPoolingInstance>();
            InactiveInstances = new System.Collections.Generic.List<IPoolingInstance>();
        }

        public override void _Ready()
        {
            base._Ready();
            _spawnPoint = GetNode<Spatial>(SpawnPointPath);
            _timer = SpawnRate;
            //this.SetupPool<Enemy>(6, EnemyScene);
        }

        public override void _ExitTree()
        {
            this.FreePool();
            base._ExitTree();
        }

        public void Spawn()
        {
            Enemy instance = EnemyScene.Instance<Enemy>();
            //Enemy instance = this.InstanceFromPool<Enemy>(Global.Instance.CurrentSceneInstance);
            if (instance == null) return;
            Global.Instance.CurrentSceneInstance.AddChild(instance);
            //GDE.Print(instance, " ", instance.GetParent());
            instance.GlobalTransform = _spawnPoint.GlobalTransform;
        }

        public override void _Process(float delta)
        {
            if (!Enabled) return;
            if (Global.Instance.GetNpcManager().IsSomebodyTalking()) return;

            if (GlobalTransform.origin.DistanceTo(Global.Instance.GetPlayer().GlobalTransform.origin) < workingDistance)
            {
                _timer -= delta;
                if (_timer <= 0.0f)
                {
                    if (GD.Randf() < SpawnChance)
                    {
                        Spawn();
                    }
                    _timer = SpawnRate;
                }
            }
        }
    }
}
