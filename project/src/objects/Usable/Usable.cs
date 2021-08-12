using Godot;
using System;

namespace Game
{
    public class Usable : Spatial, IUsable
    {
        bool _usable = true;

        public override void _Ready()
        {
            base._Ready();
        }

        public virtual void Use(Node invoker)
        {
            
        }
        public virtual string GetUseText()
        {
            return Global.Translate("use_text.USE");
        }

        public bool IsUsable()
        {
            return _usable;
        }
        public void SetUsableState(bool usable)
        {
            _usable = usable;
        }
        public virtual Spatial GetUseInfoPoint()
        {
            return this;
        }

    }
}
