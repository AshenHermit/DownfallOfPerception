using Godot;
using System;

namespace Game
{
    public interface IPoolContainer
    {
        System.Collections.Generic.List<IPoolingInstance> InstancesInPool { get; set; }
        System.Collections.Generic.List<IPoolingInstance> ActiveInstances { get; set; }
        System.Collections.Generic.List<IPoolingInstance> InactiveInstances { get; set; }
    }

    public static class PoolContainerExtensions
    {
        /// <typeparam name="T">must be IPoolingInstance</typeparam>
        public static void SetupPool<T>(this IPoolContainer container, int maxPoolingInstancesCount, PackedScene instanceScene) where T : Node
        {
            //if (!(typeof(T).IsInstanceOfType(typeof(IPoolingInstance)))) return;

            for(int i=0; i < maxPoolingInstancesCount; ++i)
            {
                IPoolingInstance instance = (IPoolingInstance)instanceScene.Instance<T>();
                instance.PoolContainer = container;
                container.InstancesInPool.Add(instance);
                container.InactiveInstances.Add(instance);
            }
            GDE.Print(container.InstancesInPool);
        }

        /// <typeparam name="T">must be IPoolingInstance</typeparam>
        public static T InstanceFromPool<T>(this IPoolContainer container, Node parent) where T : Node
        {
            if (container.InactiveInstances.Count == 0) return null;
            IPoolingInstance instance = container.InactiveInstances[0];
            container.InactiveInstances.RemoveAt(0);
            container.ActiveInstances.Add(instance);
            parent.AddChild((T)(instance));
            instance.Pooled();
            return (T)(instance);
        }

        public static void FreeInstance(this IPoolContainer container, IPoolingInstance instance)
        {
            if (!((Node)instance).IsInsideTree()) return;

            ((Node)instance).GetParent().RemoveChild((Node)instance);
            container.ActiveInstances.Remove(instance);
            container.InactiveInstances.Add(instance);
        }

        public static void FreePool(this IPoolContainer container)
        {
            foreach(IPoolingInstance instance in container.InstancesInPool)
            {
                instance.FreeInPool();
                ((Node)instance).Free();
            }
            container.InstancesInPool.Clear();
            container.InactiveInstances.Clear();
            container.ActiveInstances.Clear();
        }
    }

    public interface IPoolingInstance
    {
        IPoolContainer PoolContainer { get; set; }
        void Pooled();
    }

    public static class PoolingInstanceExtensions
    {
        public static void FreeInPool(this IPoolingInstance instance)
        {
            instance.PoolContainer.FreeInstance(instance);
        }
    }
}
