using Godot;
using System;
using System.Linq;

namespace Game
{
    public static class NodeUtils
    {
        static void RecursiveFillChildren<T>(Node node, Godot.Collections.Array<T> children) where T:Node
        {
            foreach(Node child in node.GetChildren())
            {
                if(child is T)
                {
                    children.Add((T)child);
                }
                RecursiveFillChildren<T>(child, children);
            }
        }
        public static Godot.Collections.Array<T> GetAllChildren<T>(this Node node) where T:Node
        {
            Godot.Collections.Array<T> children = new Godot.Collections.Array<T>();
            RecursiveFillChildren<T>(node, children);
            return children;
        }

        /// <summary>
        /// reparent Spatial saving its transform
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="newParent"></param>
        public static void Reparent(this Spatial obj, Spatial newParent)
        {
            Vector3 pos = new Vector3(obj.GlobalTransform.origin);
            Basis basis = obj.GlobalTransform.basis;
            obj.GetParent().RemoveChild(obj);
            newParent.AddChild(obj);
            Transform trans = obj.GlobalTransform;
            trans.origin = pos;
            trans.basis = basis;
            obj.GlobalTransform = trans;
        }
    }
}
