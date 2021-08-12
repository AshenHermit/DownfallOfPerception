using Godot;
using System;


namespace Game
{
    public class Booth : Spatial
    {
        AudioStreamPlayer _sound;
        AudioStreamPlayer _stopSound;

        public override void _Ready()
        {
            base._Ready();

            _sound = GetNode<AudioStreamPlayer>("Sound");
            _stopSound = GetNode<AudioStreamPlayer>("StopSound");
            _sound.Play();

            Global.Instance.GetGenerationManager().OnStartAreaGenerated += OnGenerated;
            Global.Instance.GetPlayerCamera().StartShakingConstantly(0.03f);
            
        }

        public void OnGenerated()
        {
            _sound.Stop();
            _stopSound.Play();
            Global.Instance.GetPlayerCamera().StopShakingConstantly();
        }
    }

}