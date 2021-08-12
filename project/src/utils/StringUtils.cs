using System;
using Godot;

namespace Game.Utils
{
    public static class StringUtils
    {
        public static string GetFileNameFromPath(this string path)
        {
            if (path == "") return "";
            int slashPos = path.FindLast("/") + 1;
            return path.Substring(slashPos, path.FindLast(".") - slashPos);
        }
    }
}
