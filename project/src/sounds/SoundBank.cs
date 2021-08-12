using System;
using Godot;
using Game.Utils;

namespace Game
{
    public class SoundBank : Resource
    {
        [Export]
        Godot.Collections.Dictionary Sounds;

        public AudioStream GetRandomSoundVariation(string id)
        {
            if (!Sounds.Contains(id)) return null;

            if(Sounds[id] is Godot.Collections.Array)
            {
                return ((Godot.Collections.Array)Sounds[id]).GetRandomElement<AudioStream>();
            }
            else if (Sounds[id] is AudioStream)
            {
                return ((AudioStream)Sounds[id]);
            }
            return null;
        }
    }
}
