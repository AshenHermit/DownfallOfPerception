using Godot;
using System;


namespace Game
{
    public class Cutscene : Spatial
    {
        [Export]
        public AudioStream AudioTrack;

        AnimationPlayer _animationPlayer; 

        public override void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            if(AudioTrack!=null) Global.Instance.GetAudioManager().PlayNonSpatialSound(AudioTrack);
        }

        public void Start()
        {
            Transform = Transform.Identity;

            _animationPlayer.Play("main");
        }

        public float GetAnimationPosition()
        {
            return _animationPlayer.CurrentAnimationPosition;
        }

        public bool IsPlaying()
        {
            return _animationPlayer.IsPlaying();
        }

        public Spatial GetCameraContainer()
        {
            return GetNode<Spatial>("Armature").GetNode<Spatial>("CameraContainer");
        }
    }
}
