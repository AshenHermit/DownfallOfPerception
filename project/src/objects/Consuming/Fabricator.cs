using Godot;
using System;


namespace Game
{
    public class Fabricator : ConsumingMachine
    {
        [Export]
        public string ResultItemId;
        [Export]
        public PackedScene ResultItemScene;
        [Export]
        public int ResultAmount;
        [Export]
        public NodePath ResultSpawnPointPath;

        [Export]
        public AudioStream DoneSoundStream;
        AudioStreamPlayer3D _workingSoundPlayer;


        float _timer = 0.0f;

        public override void _Ready()
        {
            base._Ready();
            InfoTitle = Inventory.Item.GetLocalizedName(ResultItemId);
            ClearItems();

            _workingSoundPlayer = GetNode<AudioStreamPlayer3D>("Sound");
            _workingSoundPlayer.Play(_workingSoundPlayer.Stream.GetLength()*GD.Randf());
            _workingSoundPlayer.UnitDb = -80;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (IsWorking())
            {
                _workingSoundPlayer.UnitDb += (1.0f - _workingSoundPlayer.UnitDb)/10.0f;

                if (_timer > 0.0f)
                {
                    _timer -= delta;
                }
                else
                {
                    SpawnResultItem();
                    Global.Instance.GetAudioManager().PlaySoundAtPosition(DoneSoundStream, GlobalTransform.origin, this);
                }
            }
            else
            {
                _workingSoundPlayer.UnitDb += (-80.0f - _workingSoundPlayer.UnitDb) / 10.0f;
            }
        }

        public void SpawnResultItem()
        {
            if (ResultItemScene != null)
            {
                Spatial instance = ResultItemScene.Instance<Spatial>();
                if(instance is PickableItem)
                {
                    ((PickableItem)instance).Amount = ResultAmount;
                }
                GetParent().AddChild(instance);
                instance.GlobalTransform = GetNode<Spatial>(ResultSpawnPointPath).GlobalTransform;
            }
            else
            {
                GDE.Print("fabricator: nothing to spawn");
            }
            ClearItems();
        }

        public override void StartedWorking()
        {
            base.StartedWorking();
            _timer = 3.0f;
        }

        public override string GetUseText()
        {
            if (!IsWorking())
            {
                return Global.Translate("use_text.GIVE") + " " + Inventory.Item.GetLocalizedName(GetNeededItemId()) + " x" + ResultAmount.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
