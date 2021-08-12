using Godot;
using System;

namespace Game
{
    public class MemoryUtils
    {
        public static bool IsObjectDisposed(Godot.Object obj)
        {
            return Global.WeakRef(obj).GetRef() == null;
        }
    }
}
