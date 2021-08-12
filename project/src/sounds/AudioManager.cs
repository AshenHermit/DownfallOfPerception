using Godot;
using System;

namespace Game
{
    public class AudioManager : Godot.Object
    {
        public class AudioUnit : AudioStreamPlayer3D
        {
            bool _isAboutToFree;
            public bool IsEnded() => _isAboutToFree;

            public AudioUnit(AudioStream audioStream, Vector3 position, Spatial parent = null, string bus = "world")
            {
                if (parent==null) Global.Instance.CurrentSceneInstance.AddChild(this);
                else parent.AddChild(this);
                Transform trans = GlobalTransform;
                trans.origin = position;
                GlobalTransform = trans;
                Stream = audioStream;
                Bus = bus;
                UnitSize = 25.0f;
                MaxDistance = 100.0f;
                //AttenuationFilterCutoffHz = 20500;
                Play();
            }
            public override void _Process(float delta)
            {
                if (!Playing && !_isAboutToFree)
                {
                    _isAboutToFree = true;
                    return;
                }
                if (_isAboutToFree)
                {
                    QueueFree();
                }
            }
        }
        //TODO: rough copy of AudioUnit
        public class NonSpatialAudioUnit : AudioStreamPlayer
        {
            bool _isAboutToFree;
            public bool IsEnded() => _isAboutToFree;

            public NonSpatialAudioUnit(AudioStream audioStream, string bus = "master")
            {
                Global.Instance.CurrentSceneInstance.AddChild(this);
                Stream = audioStream;
                Bus = bus;
                Play();
            }
            public override void _Process(float delta)
            {
                if (!Playing && !_isAboutToFree)
                {
                    _isAboutToFree = true;
                    return;
                }
                if (_isAboutToFree)
                {
                    QueueFree();
                }
            }
        }

        Godot.Collections.Array<AudioUnit> audioUnits = new Godot.Collections.Array<AudioUnit>();

        public int MasterBusIndex;
        public int WorldBusIndex;
        public int VoiceBusIndex;
        float _worldBusVolume = -100.0f;
        float _worldBusVolumeTarget = -100.0f;

        SoundBank _soundBank;

        //TODO: this and some methods for music can be moved in another class
        AudioStreamPlayer _musicPlayer;
        float _musicFadeInTime = 0.0f;
        float _musicFadeOutTime = 0.0f;
        float _musicVolume = 0.0f;
        bool _musicIsFadingOut = false;


        //TODO: maybe it is not safe
        public AudioManager()
        {
            
        }

        public void _Ready()
        {
            _soundBank = GD.Load<SoundBank>("res://game_objects/SoundBank.tres");
            _musicPlayer = new AudioStreamPlayer();
            Global.Instance.AddChild(_musicPlayer);

            MasterBusIndex = AudioServer.GetBusIndex("Master");
            WorldBusIndex = AudioServer.GetBusIndex("world");
            VoiceBusIndex = AudioServer.GetBusIndex("voice");
            AudioServer.SetBusVolumeDb(WorldBusIndex, _worldBusVolume);
        }

        public AudioUnit PlaySoundAtPosition(AudioStream audioStream, Vector3 position, Spatial parent = null, string bus="world")
        {
            if(audioStream==null) return null;

            AudioUnit audioUnit = new AudioUnit(audioStream, position, parent, bus);
            //audioUnits.Add(audioUnit);
            return audioUnit;
        }
        public AudioUnit PlaySoundAtPosition(string audioStreamPath, Vector3 position, Spatial parent = null, string bus = "world")
        {
            return PlaySoundAtPosition(GD.Load<AudioStream>(audioStreamPath), position, parent, bus);
        }
        public AudioUnit PlaySoundFromBankAtPosition(string id, Vector3 position, Spatial parent = null, string bus = "world")
        {
            return PlaySoundAtPosition(_soundBank.GetRandomSoundVariation(id), position, parent, bus);
        }


        public NonSpatialAudioUnit PlayNonSpatialSound(AudioStream audioStream, string bus = "master")
        {
            if (audioStream == null) return null;

            NonSpatialAudioUnit audioUnit = new NonSpatialAudioUnit(audioStream, bus);
            return audioUnit;
        }
        public NonSpatialAudioUnit PlayNonSpatialSound(string audioStreamPath, string bus = "master")
        {
            return PlayNonSpatialSound(GD.Load<AudioStream>(audioStreamPath), bus);
        }
        public NonSpatialAudioUnit PlayNonSpatialSoundFromBank(string id, string bus = "master")
        {
            return PlayNonSpatialSound(_soundBank.GetRandomSoundVariation(id), bus);
        }

        public void Process(float delta)
        {
            RegulateBuses();
            UpdateMusicPlayer(delta);
        }

        void RegulateBuses()
        {
            if (Global.Instance.GetNpcManager() != null)
            {
                if (Global.Instance.GetNpcManager().IsSomebodyTalking())
                {
                    _worldBusVolumeTarget = -30.0f;
                }
                else
                {
                    _worldBusVolumeTarget = -5.0f;
                }
            }
            else
            {
                _worldBusVolumeTarget = -5.0f;
            }
            _worldBusVolume += (_worldBusVolumeTarget - _worldBusVolume) / 16.0f;
            AudioServer.SetBusVolumeDb(WorldBusIndex, _worldBusVolume);
        }
        public void SetVolume(float volume)
        {
            AudioServer.SetBusVolumeDb(MasterBusIndex, volume);
        }


        // music
        void UpdateMusicPlayer(float delta)
        {
            // fade in / fade out
            if (!_musicIsFadingOut)
            {
                _musicVolume = Mathf.Min(_musicVolume + delta * 1.0f / _musicFadeInTime, 1.0f);
            }
            else
            {
                _musicVolume = Mathf.Max(0.0f, _musicVolume - delta * 1.0f / _musicFadeOutTime);
            }

            _musicPlayer.VolumeDb = -80.0f + _musicVolume*80.0f;
        }


        public void StartMusicFadeIn(float fadeInTime = 0.0f)
        {
            _musicIsFadingOut = false;
            _musicVolume = 0.0f;
            _musicPlayer.VolumeDb = -80;
            _musicFadeInTime = fadeInTime;
        }
        public void StartMusicFadeOut(float fadeOutTime = 0.0f)
        {
            _musicIsFadingOut = true;
            _musicFadeOutTime = fadeOutTime;
        }

        public void PlayMusic(AudioStream stream, float fadeInTime=0.0f)
        {
            if (_musicPlayer.Stream == stream) return;
            _musicPlayer.Stream = stream;
            _musicPlayer.Play();
            StartMusicFadeIn(fadeInTime);
        }
        public void PlayMusic(string streamPath, float fadeInTime = 0.0f)
        {
            PlayMusic(GD.Load<AudioStream>(streamPath), fadeInTime);
        }

        public void StopMusic(float fadeOutTime = 0.0f)
        {
            StartMusicFadeOut(fadeOutTime);
        }
    }
}
