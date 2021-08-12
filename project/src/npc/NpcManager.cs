using System;
using Godot;


namespace Game
{
    public class NpcManager : Node
    {
        bool _someBodyIsTalking = false;

        public delegate void MonologueHandler(string text);
        public event MonologueHandler OnMonologueStarted;
        public event Action OnMonologueEnded;

        public override void _Ready()
        {
            Global.Instance.RegisterNpcManager(this);
        }

        public void NpcTookDamage(Npc npc, float damage)
        {
            
        }

        public bool IsSomebodyTalking()
        {
            return _someBodyIsTalking;
        }
        public void SomebodyStartedTalking(string text)
        {
            _someBodyIsTalking = true;
            OnMonologueStarted?.Invoke(text);
        }
        public void SomebodyStoppedTalking()
        {
            _someBodyIsTalking = false;
            OnMonologueEnded?.Invoke();
        }
    }
}
