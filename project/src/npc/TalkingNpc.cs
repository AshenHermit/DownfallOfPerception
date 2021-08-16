using Godot;
using System;
using Game.Utils;

namespace Game { 
    public class TalkingNpc : Npc
    {
        [Export]
        public string npcId = "";
        [Export]
        public bool StopTalkingWhenPlayerIsFar = true;

        [Export]
        public NodePath SpawnPointPath;

        [Export]
        Godot.Collections.Dictionary<string, Godot.Collections.Array<Godot.Collections.Dictionary>> monologues = 
            new Godot.Collections.Dictionary<string, Godot.Collections.Array<Godot.Collections.Dictionary>>();

        int _currentMonologuePartIndex = 0;
        string _currentMonologueId = "";

        float _talkDistance = 35.0f;
        bool _isTalking = false;

        public bool IsTalking() => _isTalking;

        protected AudioManager.AudioUnit _currentAudioUnit;

        public override void _Ready()
        {
            base._Ready();
            SetUsableState(true);
        }

        public void AddMonologue(string monologueId, string message, AudioStream audioStream)
        {
            monologues[monologueId].Add(new Godot.Collections.Dictionary { {"audio_stream", audioStream } });
        }

        public override void Use(Node invoker)
        {
            base.Use(invoker);
            if (_isTalking)
            {
                Skip();
            }
            else
            {
                Talk();
            }
        }
        public override void _Process(float delta)
        {
            base._Process(delta);

            CheckMonologueState();
        }
        void CheckMonologueState()
        {
            if (_currentAudioUnit == null) return;

            if (_isTalking && _currentAudioUnit.IsEnded())
            {
                _currentAudioUnit = null;
                Global.Instance.GetNpcManager().SomebodyStoppedTalking();
                _isTalking = false;
                OnPartEnded();
                _currentMonologuePartIndex += 1;
                //await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                if (IsPlayerListening()) Talk();
            }
        }
        public virtual void OnPartEnded() {
            Godot.Collections.Array<Godot.Collections.Dictionary> monologeParts = monologues[_currentMonologueId];
            Godot.Collections.Dictionary monologuePart = monologeParts[_currentMonologuePartIndex];
            if (monologuePart.Contains("objective"))
            {
                Global.Instance.GetObjectivesManager().AchieveObjective(monologuePart.Get<string>("objective"));
            }
            if (monologuePart.Contains("action"))
            {
                Global.Instance.GetGenerationManager().ActionHappened(monologuePart.Get<string>("action"));
            }
            if (monologuePart.Contains("spawn"))
            {
                Spatial instance = GD.Load<PackedScene>(monologuePart.Get<string>("spawn")).Instance<Spatial>();
                Global.Instance.CurrentSceneInstance.AddChild(instance);
                instance.GlobalTransform = GetNode<Spatial>(SpawnPointPath).GlobalTransform;
            }
            if (monologuePart.Contains("stun_player"))
            {
                if (monologuePart.Get<bool>("stun_player"))
                {
                    Global.Instance.GetPlayer().Stun(this);
                }
            }
        }
        public virtual void OnPartStarted()
        {

        }
        public void Talk()
        {
            if (!IsPlayerListening()) return;
            if (!CanTalk()) return;

            Godot.Collections.Array<Godot.Collections.Dictionary> monologeParts = monologues[_currentMonologueId];
            Godot.Collections.Dictionary monologuePart = monologeParts[_currentMonologuePartIndex];

            _isTalking = true;
            _currentAudioUnit = Global.Instance.GetAudioManager().PlaySoundAtPosition(GetCurrentMonologuePartAudio(), GlobalTransform.origin, this, "voice");
            _currentAudioUnit.UnitSize = 60.0f;
            _currentAudioUnit.MaxDb = 2.0f;
            _currentAudioUnit.MaxDistance = 100.0f;

            OnPartStarted();

            string currentMonologueText = GetCurrentMonologuePartText();
            Global.Instance.GetNpcManager().SomebodyStartedTalking(currentMonologueText);
        }

        public void Skip()
        {
            if (!_isTalking) return;
            _currentAudioUnit.Stop();
        }

        public string GetCurrentMonologuePartText()
        {
            string text = "npc."+npcId+".monologue."+_currentMonologueId+"."+ _currentMonologuePartIndex.ToString()+".text";
            text = Global.Translate(text);
            return text;
        }

        public AudioStream GetCurrentMonologuePartAudio()
        {
            string path = "res://resources/sounds/monologues/" + npcId + "/" + _currentMonologueId + "_" + _currentMonologuePartIndex.ToString() + ".mp3";
            AudioStream stream = GD.Load<AudioStream>(path);
            return stream;
        }

        public void SetMonologue(string monologueId)
        {
            _currentMonologueId = monologueId;
            _currentMonologuePartIndex = 0;
        }
        public void SetMonologuePartIndex(int partIndex)
        {
            _currentMonologuePartIndex = partIndex;
        }

        public bool IsPlayerListening()
        {
            if (Global.Instance.GetNpcManager().IsSomebodyTalking()) return false;

            if (StopTalkingWhenPlayerIsFar)
            {
                return Global.Instance.GetPlayer().GlobalTransform.origin.DistanceTo(GlobalTransform.origin) <= _talkDistance;
            }
            else
            {
                return true;
            }
        }
        public bool CanTalk()
        {
            if (_currentMonologueId == "") return false;
            if (!monologues.ContainsKey(_currentMonologueId)) return false;
            Godot.Collections.Array<Godot.Collections.Dictionary> monologeParts = monologues[_currentMonologueId];
            if (_currentMonologuePartIndex >= monologeParts.Count) return false;
            return true;
        }

        public override string GetUseText()
        {
            if (CanTalk())
            {
                return Global.Translate("use_text.TALK_TO") + " " + Global.Translate("npc."+npcId+".talk_to_name");
            }
            else
            {
                return "";
            }
        }
    }
}


