using Godot;
using System;

namespace Game
{
    public class CustomAnimationPlayer : AnimationPlayer
    {
        public void PlaySoundFromBank(string id)
        {
            Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition(id, GetNode<Spatial>(RootNode).GlobalTransform.origin);
        }
    }
}
