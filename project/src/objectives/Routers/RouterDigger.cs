using Godot;
using System;

namespace Game
{
    public class RouterDigger : Usable
    {
        [Export]
        public NodePath ModelPath;
        Spatial _arrow;
        [Export]
        public NodePath NoiseNodePath;
        public NoiseMovement NoiseNode;

        [Export]
        public NodePath TextNodePath;

        [Export]
        public NodePath WireInputPath;

        bool _working = false;
        public bool IsWorking() => _working;
        bool _calibrated = false;
        public bool IsCalibrated() => _calibrated;

        FastNoiseLite _callibrationNoise = new FastNoiseLite();
        float _calibrationPosision = 0.0f;
        float _softCalibrationPosision = 0.0f;
        float _calibrationNoiseStrength = 1.0f;
        float timer = 0.0f;

        float _calibrationProgress;

        TextIn3D _textNode;

        bool _isCalibrating = false;

        public override void _Ready()
        {
            base._Ready();
            _arrow = GetNode<Spatial>(ModelPath).GetNode("ArrowContainer").GetChild<Spatial>(0);
            NoiseNode = GetNode<NoiseMovement>(NoiseNodePath);
            NoiseNode.Speed = 0.0f;
            NoiseNode.Strength = 0.1f;

            _textNode = GetNode<TextIn3D>(TextNodePath);

            _callibrationNoise.Seed = (int)GD.RandRange(0, 100000);
            SetCalibrationDifficulty(0.2f);
            if (Global.Instance.DEBUG) SetCalibrationDifficulty(0.01f);
            UpdateText();

        }

        public void SetCalibrationDifficulty(float factor)
        {
            _callibrationNoise.Period = 16.0f * (1.0f-factor);
            _calibrationNoiseStrength = factor*1.0f;
        }

        void UpdateText()
        {
            if (!_working)
            {
                _textNode.SetText("");
                return;
            }

            if (!_calibrated)
            {
                string progress = "[";
                for(int i=0; i<8; ++i)
                {
                    if((float)i < _calibrationProgress * 7.0f)
                    {
                        progress += "=";
                    }
                    else
                    {
                        progress += "  ";
                    }
                }
                progress += "]";
                _textNode.SetText(Global.Translate("router_digger.progress_info")+"\n"+progress);
            }
            else
            {
                _textNode.SetText(Global.Translate("router_digger.calibrated"));
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (_working)
            {
                NoiseNode.Speed += (3.0f - NoiseNode.Speed)/10.0f;

                timer += delta;

                if (!_calibrated)
                {
                    _calibrationPosision += _callibrationNoise.GetNoise2d(0.0f, timer)*_calibrationNoiseStrength;
                    if (Mathf.Cos(_calibrationPosision) > 0.7f)
                    {
                        _calibrationProgress += 0.2f*delta;
                    }
                    _calibrationProgress -= delta*0.1f;
                    _calibrationProgress = Mathf.Max(0.0f, _calibrationProgress);
                }
                else
                {
                    _calibrationPosision += _callibrationNoise.GetNoise2d(0.0f, timer) * 0.01f;
                }

                UpdateText();
                UpdateArrow();
            }
        }

        public void UpdateArrow()
        {
            _softCalibrationPosision += (_calibrationPosision - _softCalibrationPosision) / 6.0f;
            Vector3 rot = _arrow.Rotation;
            rot.y = _softCalibrationPosision;
            _arrow.Rotation = rot;
        }

        public void TryToTurnOn()
        {
            if (Global.Instance.DEBUG)
            {
                TurnOn();
                MakeCalibrated();
                return;
            }

            if (Global.Instance.CurrentSceneName != "Withering") return;

            bool routerFound = Global.Instance.GetGenerationManager()
                .GetCurrentGenerationProfile<WitheringLayer>()
                .HasRouterAtPosition(GlobalTransform.origin);
            if (routerFound || true)
            {
                TurnOn();
            }
            else
            {
                _textNode.SetText("router_digger.router_not_found");
            }
        }

        void TurnOn() {
            _working = true;
            SetUsableState(false);
            GetNode<AudioStreamPlayer3D>("Sound").Play();
        }



        public override void Use(Node invoker)
        {
            TryToTurnOn();
        }

        public override string GetUseText()
        {
            return Global.Translate("use_text.TURN_ON");
        }

        public void OnWireConnected(Wire wire)
        {
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/source_machine/source_machine_turn_on.ogg",
                GlobalTransform.origin, this);
        }

        public void MakeCalibrated()
        {
            _calibrated = true;
            Global.Instance.GetGenerationManager().ActionHappened("router_digger_calibrated");
            Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/effects/zap.ogg",
                GlobalTransform.origin, this);
            GetNode<Spatial>(WireInputPath).Visible = true;
            //TODO: awfull
            GetNode<Spatial>(WireInputPath).Set("wire_id", "router_digger");
        }

        public bool Calibrate(float factor)
        {
            _calibrationPosision -= factor*0.04f;
            if (_calibrationProgress >= 1.0f)
            {
                MakeCalibrated();
            }
            return _calibrated;
        }
    }
}

