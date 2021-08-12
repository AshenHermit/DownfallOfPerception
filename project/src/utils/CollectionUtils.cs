using Godot;
using System;

namespace Game.Utils
{
    static class CollectionUtils
    {
        public static Godot.Collections.Dictionary<string, object> Assign(this Godot.Collections.Dictionary<string, object> first, Godot.Collections.Dictionary<string, object> second)
        {
            Godot.Collections.Dictionary<string, object> dict = first.Duplicate();
            foreach (string key in second.Keys)
            {
                dict[key] = second[key];
            }
            return dict;
        }


        // Doesn't work in exported project, GetPropertyList is not returning c# public fields
        public static Godot.Collections.Dictionary<string, object> ToDict(this Godot.Object obj)
        {
            if (obj == null) return null;

            Godot.Collections.Dictionary<string, object> dict = new Godot.Collections.Dictionary<string, object>();

            foreach (Godot.Collections.Dictionary prop in obj.GetPropertyList())
            {
                string key = prop.Get<string>("name");
                dict[key] = obj.Get(key);
            }
            ((Godot.Collections.Dictionary)dict).TurnResourcesIntoPaths();

            return new Godot.Collections.Dictionary<string, object>(dict);
        }
        public static void AssignDict(this Godot.Object obj, Godot.Collections.Dictionary<string, object> dict)
        {
            dict.LoadResourcesWithPaths();
            foreach (string key in dict.Keys)
            {
                obj.Set(key, dict[key]);
            }
        }

        public static void LoadResourcesWithPaths(this Godot.Collections.Dictionary<string, object> dict)
        {
            foreach (string key in dict.Keys)
            {
                if (dict[key] is string)
                {
                    string value = (string)dict[key];
                    if (value.StartsWith("res://")) {
                        if (value.EndsWith(".tscn"))
                        {
                            dict[key] = GD.Load<PackedScene>(value);
                        }
                        else if (value.EndsWith(".png") || value.EndsWith(".jpg"))
                        {
                            dict[key] = GD.Load<Texture>(value);
                        }
                    }
                }
            }
        }
        public static void TurnResourcesIntoPaths(this Godot.Collections.Dictionary dict)
        {
            foreach (string key in dict.Keys)
            {
                if (dict[key] is PackedScene)
                {
                    dict[key] = ((PackedScene)dict[key]).ResourcePath;
                }
                else if (dict[key] is Texture)
                {
                    dict[key] = ((Texture)dict[key]).ResourcePath;
                }
            }
        }

        public static T Get<T>(this Godot.Collections.Dictionary dict, object key)
        {
            if (dict.Contains(key))
            {
                return (T)dict[key];
            }
            else
            {
                return default(T);
            }
        }
        public static int GetInt(this Godot.Collections.Dictionary dict, object key)
        {
            if (dict.Contains(key))
            {
                return Convert.ToInt32(dict[key]);
            }
            else
            {
                return 0;
            }
        }
        public static float GetFloat(this Godot.Collections.Dictionary dict, object key)
        {
            if (dict.Contains(key))
            {
                return Convert.ToSingle(dict[key]);
            }
            else
            {
                return 0.0f;
            }
        }
        public static int GetInt(this Godot.Collections.Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                return Convert.ToInt32(dict[key]);
            }
            else
            {
                return 0;
            }
        }
        public static float GetFloat(this Godot.Collections.Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                return Convert.ToSingle(dict[key]);
            }
            else
            {
                return 0.0f;
            }
        }
        public static T Get<T>(this Godot.Collections.Dictionary<string, object> dict, string key)
        {
            return ((Godot.Collections.Dictionary)dict).Get<T>(key);
        }

        public static T GetRandomElement<T>(this Godot.Collections.Array array)
        {
            if (array.Count == 0) return default(T);
            return (T)(array[(int)Mathf.Floor(GD.Randf() * array.Count)]);
        }
        public static Godot.Collections.Dictionary GetRandomElement(this Godot.Collections.Array<Godot.Collections.Dictionary> array)
        {
            return ((Godot.Collections.Array)array).GetRandomElement<Godot.Collections.Dictionary>();
        }
    }
}
