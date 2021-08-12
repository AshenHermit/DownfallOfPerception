using Godot;
using System;

namespace Game
{
    public interface IScriptedObject
    {
        Godot.Object ScriptContext { get; set; }
        Godot.Collections.Dictionary Exports { get; set; }
    }

    public static class ScriptedObjectExtensions
    {
        /// <summary>
        /// instantiating object ScriptContext from scriptResource and setting "this" variable to this IScriptable Instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptedObject"></param>
        /// <param name="scriptResource"></param>
        public static void SetupScriptContext<T>(this IScriptedObject scriptedObject, Godot.GDScript scriptResource) where T:Godot.Object
        {
            scriptedObject.ScriptContext = (Godot.Object)scriptResource.New();
            scriptedObject.ScriptContext.Set("this", (T)scriptedObject);
            if (scriptedObject.Exports != null) { 
                foreach (string key in scriptedObject.Exports.Keys)
                {
                    scriptedObject.ScriptContext.Set(key, scriptedObject.Exports[key]);
                }
            }
        }
        public static void CallMethodSafely(this IScriptedObject scriptedObject, string method, params object[] args)
        {
            if (scriptedObject.ScriptContext != null)
            {
                if (scriptedObject.ScriptContext.HasMethod(method)) scriptedObject.ScriptContext.Call(method, args);
            }
        }
    }
}
