using Godot;
using System;

namespace Game
{
    public interface IUsable
    {
        bool IsUsable();
        void SetUsableState(bool usable);
        void Use(Node invoker);
        string GetUseText();
        Spatial GetUseInfoPoint();
    }
}
