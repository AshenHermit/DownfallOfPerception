using Godot;
using System;

namespace Game
{
    public class NoiseMovement : Spatial
    {
        public float Speed = 0.03f;
        public float Strength = 1.0f;

        OpenSimplexNoise _noise = new OpenSimplexNoise();
        Vector3 _origin;
        float _timer=0.0f;

        public override void _Ready()
        {
            _origin = Transform.origin;
            _noise.Seed = (int)OS.GetUnixTime();
        }

        public override void _Process(float delta)
        {
            Transform trans = Transform;
            trans.origin = _origin + new Vector3(_noise.GetNoise2d(-100.0f, _timer*100.0f), _noise.GetNoise2d(0.0f, _timer*100.0f), _noise.GetNoise2d(100.0f, _timer*100.0f))*Strength;
            Transform = trans;
            _timer += delta * Speed;
        }
    }
}
