using Godot;
using System;
using Game.Utils;


namespace Game
{
    public class AI : TalkingNpc, IVerticalUsable
    {
        [Export]
        public PackedScene WireOutputScene;
        [Export]
        public NodePath DefenceAreaPath;
        Area _defenceArea;

        AnimationPlayer _animationPlayer;
        AnimationPlayer _panelAnimationPlayer;
        AnimationPlayer _handAnimationPlayer;

        float _animationPlaybackSpeedTarget = 0.0f;

        Spatial _handPoint;

        WeakRef bodyInArea;
        bool _takingBody = false;
        bool _bodyGrabbed = false;
        bool _mustTalkWhenPlayerArrived = false;
        bool _wiresUnfolded = false;
        bool _working = false;

        int _wireOutputCounter = 3;
        public void SetWiresOutputsCount(int wiresCount)
        {
            _wireOutputCounter = wiresCount;
        }

        public Spatial UseInfoPoint { get; set; }

        public void TalkWhenPlayerArrived()
        {
            _mustTalkWhenPlayerArrived = true;
        }
        public void DontTalkWhenPlayerArrived()
        {
            _mustTalkWhenPlayerArrived = false;
        }

        public override void _Ready()
        {
            base._Ready();
            SetMonologue("need_router_calibration");
            _animationPlayer = GetNode(VisualNodePath).GetNode<AnimationPlayer>("AnimationPlayer");
            _panelAnimationPlayer = GetNode("AIPanel").GetNode<AnimationPlayer>("AnimationPlayer");
            _handAnimationPlayer = GetNode("AIHand").GetNode<AnimationPlayer>("AnimationPlayer");
            _handPoint = GetNode("AIHand").GetNode("HandArmature").GetNode("base3").GetNode("arm_12").GetNode("arm_22").GetNode<Spatial>("hand");
            _defenceArea = GetNode<Area>(DefenceAreaPath);
            _defenceArea.Monitoring = false;
            SetupAnimations();
            SetupWireOutputs();
            Damageable = false;

            this.SetupVerticalUsable(this);

            //TODO:kludge:
            if(Global.Instance.CurrentSceneName != "Withering")
            {
                Charge();
            }
        }
        public override void _Process(float delta)
        {
            base._Process(delta);
            UpdateTalkingAnimation();
            _animationPlayer.PlaybackSpeed += (_animationPlaybackSpeedTarget - _animationPlayer.PlaybackSpeed) / 20.0f;
            UpdateBodyInHand(delta);
            this.UpdateVerticalUsable();
        }

        void SetupAnimations()
        {
            _animationPlayer.GetAnimation("idle").Loop = true;
            _animationPlayer.GetAnimation("sick").Loop = true;
            _animationPlayer.Play("idle");
        }

        WireOutput CreateWireOutput(Spatial parent)
        {
            if (_wireOutputCounter > 0)
            {
                _wireOutputCounter -= 1;
                WireOutput output = WireOutputScene.Instance<WireOutput>();
                parent.AddChild(output);
                output.Transform = Transform.Identity;
                output.SetWireId("router_digger");
                output.RequestWire().OnConnected += OnWireConnected;
               return output;
            }
            return null;
        }

        void SetupWireOutputs()
        {
            Node panel = GetNode("AIPanel").GetNode("PanelArmature").GetNode("base2").GetNode("arm_1").GetNode("arm_2").GetNode("panel");
            WireOutput output = CreateWireOutput(panel.GetNode<Spatial>("wire_input_1"));
            output = CreateWireOutput(panel.GetNode<Spatial>("wire_input_2"));
            output = CreateWireOutput(panel.GetNode<Spatial>("wire_input_3"));
        }

        void OnWireConnected(Spatial wireInput)
        {
            if ((string)wireInput.Get("wire_id") != "router_digger") return;

            RouterDigger digger = wireInput.GetParent().GetParent<RouterDigger>();
            if (digger.IsCalibrated())
            {
                bodyInArea = WeakRef(bodyInArea);
                Global.Instance.GetGenerationManager().ActionHappened("router_wire_connected");
            }
        }

        public void OnAreaBodyEntered(Node body)
        {
            if (body == Global.Instance.GetPlayer())
            {
                if (_mustTalkWhenPlayerArrived)
                {
                    Talk();
                    _mustTalkWhenPlayerArrived = false;
                }
            }

            if (_takingBody) return;

            string bodyName = body.Filename.GetFileNameFromPath();
            if (bodyName == "MemoryCardItem")
            {
                bodyInArea = WeakRef(body);
                Global.Instance.GetGenerationManager().ActionHappened("ai_got_" + bodyName);
                TakeBodyInArea();
            }
        }

        public void TakeBodyInArea()
        {
            if (_takingBody) return;
            if (bodyInArea.GetRef() == null) return;
            
            if(bodyInArea.GetRef() is PickableItem)
            {
                ((PickableItem)bodyInArea.GetRef()).SetUsableState(false);
            }
            ((Godot.Object)bodyInArea.GetRef()).Set("collision_layer", 0);
            ((Godot.Object)bodyInArea.GetRef()).Set("collision_mask", 0);

            _takingBody = true;
            _handAnimationPlayer.Play("hand_grab");
        }
        void UpdateBodyInHand(float delta)
        {
            if (!_takingBody) return;
            if (bodyInArea.GetRef() == null) return;

            if(!_bodyGrabbed 
                && _handAnimationPlayer.IsPlaying() 
                && _handAnimationPlayer.CurrentAnimation == "hand_grab" 
                && _handAnimationPlayer.CurrentAnimationPosition>=0.25f)
            {
                ((Godot.Object)bodyInArea.GetRef()).Set("mode", RigidBody.ModeEnum.Kinematic);
                _bodyGrabbed = true;
            }
            if (_bodyGrabbed && !_handAnimationPlayer.IsPlaying())
            {
                _bodyGrabbed = false;
            }
            if (_bodyGrabbed)
            {
                Spatial body = ((Spatial)bodyInArea.GetRef());
                //body.GlobalTransform.InterpolateWith(_handPoint.GlobalTransform, 0.5f);
                Transform trans = body.GlobalTransform;
                trans.origin += (_handPoint.GlobalTransform.origin - trans.origin) * 6.0f * delta;
                trans.basis = _handPoint.GlobalTransform.basis;
                body.GlobalTransform = trans;
            }
        }

        public void Discharge(bool canBreath = true)
        {
            _animationPlaybackSpeedTarget = 0.0f;
            if (!canBreath) GetNode<AudioStreamPlayer3D>("Sound").Stop();
            _working = false;
        }
        public void Charge()
        {
            MakeIdle();
            _working = true;
        }
        public void MakeSick()
        {
            _animationPlayer.Play("sick");
        }
        public void Unfold()
        {
            if (_wiresUnfolded) return;
            _wiresUnfolded = true;
            _panelAnimationPlayer.Play("unfold");
        }

        public void _OnDefenceAreaBodyEntered(Node body)
        {
            if (!IsTalking()) return;
            if (!(body is Enemy)) return;
            Enemy enemy = (Enemy)body;
            enemy.MultiplyItemsChance(1.0f / 5.0f);
            enemy.Die();
        }

        public override void OnPartStarted()
        {
            base.OnPartStarted();
            _defenceArea.Monitoring = true;
        }
        public override void OnPartEnded()
        {
            base.OnPartEnded();
            _defenceArea.Monitoring = false;
        }

        public override Spatial GetUseInfoPoint()
        { 
            return this.UseInfoPoint;
        }

        void MakeIdle()
        {
            if (!_working) return;
            _animationPlaybackSpeedTarget = 0.5f;
        }

        void UpdateTalkingAnimation()
        {
            if (!_working) return;
            if (!IsTalking())
            {
                MakeIdle();
            }
            else
            {
                float volume = (Global.Instance.GetAudioManager().GetVoiceBusPeakVolume() + 20.0f)/3.0f;
                _animationPlaybackSpeedTarget = Mathf.Clamp(
                    volume, 0.0f, 1.0f);
            }

        }
    }
}
