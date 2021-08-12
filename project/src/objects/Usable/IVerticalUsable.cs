using Godot;
using System;

namespace Game
{
    public interface IVerticalUsable
    {
        Spatial UseInfoPoint { get; set; }
    }

    public static class VerticalUsableExtensions
    {
        public static void SetupVerticalUsable(this IVerticalUsable usable, Spatial parent)
        {
            usable.UseInfoPoint = new Spatial();
            parent.AddChild(usable.UseInfoPoint);
        }
        public static void UpdateVerticalUsable(this IVerticalUsable usable)
        {
            Transform trans = usable.UseInfoPoint.GlobalTransform;
            trans.origin.y = Global.Instance.GetPlayerCamera().GlobalTransform.origin.y;
            usable.UseInfoPoint.GlobalTransform = trans;
        }
    }
}
