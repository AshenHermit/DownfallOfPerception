using Godot;
using System;

namespace Game
{
    public static class GDE
    {
        public static void Print(params object[] what)
        {
            if (Global.Instance == null) GD.Print(what);
            if (Global.Instance.DEBUG) GD.Print(what);
        }
    }
}
