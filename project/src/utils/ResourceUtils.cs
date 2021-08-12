using Godot;
using System;

namespace Game.Utils
{
    /// <summary>
    /// Provides api for accessing subresources of standardized resources, exported with resource exporter of this game
    /// Res-exporter exports:
    ///     mesh models with materials (MeshInstance) (.tscn)
    ///     optimized collision shapes (CollisionShape) (.tscn)
    ///     physics bodies             (Static or Rigid Bodies combined with mesh and collision shape) (.tscn)
    ///     icons                      (ImageTexture) (.png)
    /// </summary>
    public class ResourceUtils
    {
        /// <summary>
        /// Get variant of type T at index in bundled packed scene "variants" array
        /// </summary>
        /// <typeparam name="T">type of returning instance</typeparam>
        /// <param name="scene">packed scene to get variants from</param>
        /// <param name="variantIdx">index of needed variant in bundled packed scene</param>
        /// <returns>instance of class T</returns>
        public static T GetPackedSceneVariant<T>(PackedScene scene, int variantIdx)
        {
            Godot.Collections.Array variants = (Godot.Collections.Array)scene._Bundled["variants"];
            if (variantIdx < 0)
            {
                variantIdx = variants.Count + variantIdx;
            }
            return (T)variants[variantIdx];
        }

        // mesh
        /// <summary>
        /// Get mesh of body exported with res-exporter.
        /// </summary>
        /// <param name="modelScene">model scene resource</param>
        /// <returns>mesh resource</returns>
        public static Mesh GetMeshOfBodyModel(PackedScene modelScene)
        {
            return GetMeshOfMeshModel(GetPackedSceneVariant<PackedScene>(modelScene, -2));
        }
        /// <summary>
        /// Get mesh of mesh model exported with res-exporter.
        /// </summary>
        /// <param name="modelScene">model scene resource</param>
        /// <returns>mesh resource</returns>
        public static Mesh GetMeshOfMeshModel(PackedScene modelScene)
        {
            return GetPackedSceneVariant<Mesh>(modelScene, 0);
        }

        // material
        /// <summary>
        /// Get material of body exported with res-exporter.
        /// </summary>
        /// <param name="modelScene">model scene resource</param>
        /// <returns>material resource</returns>
        public static Material GetMaterialOfBodyModel(PackedScene modelScene)
        {
            return GetMaterialOfMeshModel(GetPackedSceneVariant<PackedScene>(modelScene, -2));
        }
        /// <summary>
        /// Get material of mesh model exported with res-exporter.
        /// </summary>
        /// <param name="modelScene">model scene resource</param>
        /// <returns>material resource</returns>
        public static Material GetMaterialOfMeshModel(PackedScene modelScene)
        {
            return GetPackedSceneVariant<Material>(modelScene, 0);
        }

        // icons
        /// <summary>
        /// Get icon image mesh resource.
        /// It adds "_icon.png" at the end of mesh filename, then loads and returns icon image resource.
        /// </summary>
        /// <param name="mesh">mesh resource</param>
        /// <returns>image texture resoruce</returns>
        public static Texture GetIconOfMesh(Mesh mesh)
        {
            if (mesh == null) return null;

            string modelPath = mesh.ResourcePath;
            if (modelPath.EndsWith(".obj"))
            {
                return GetIconOfScene(modelPath);
            }
            return null;
        }
        public static Texture GetIconOfScene(string scenePath)
        {
            string iconPath = scenePath.Substring(0, scenePath.FindLast("."));
            iconPath += "_icon.png";
            if (Godot.ResourceLoader.Exists(iconPath))
            {
                return GD.Load<Texture>(iconPath);
            }
            return null;
        }
        public static Texture GetIconOfSpatial(Spatial spatial)
        {
            if (spatial == null) return null;

            string modelPath = spatial.Filename;
            if (modelPath.EndsWith(".obj") || modelPath.EndsWith(".tscn") || modelPath.EndsWith(".glb"))
            {
                return GetIconOfScene(modelPath);
            }
            return null;
        }
    }
}
