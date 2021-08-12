using Godot;
using System;

namespace Game { 
    public class TextIn3D : Spatial
    {
        [Export]
        public NodePath MeshInstancePath;
        MeshInstance _meshInstance;

        [Export]
        public NodePath ViewportPath;
        Viewport _viewport;

        [Export]
        public NodePath LabelPath;
        Label _label;



        public override void _Ready()
        {
            _meshInstance = GetNode<MeshInstance>(MeshInstancePath);
            _meshInstance.SetSurfaceMaterial(0, (Material)_meshInstance.GetSurfaceMaterial(0).Duplicate());
            _viewport = GetNode<Viewport>(ViewportPath);
            _label = GetNode<Label>(LabelPath);
            ((SpatialMaterial)_meshInstance.GetSurfaceMaterial(0)).AlbedoTexture = _viewport.GetTexture();
        }

        public void SetText(string text)
        {
            _label.Text = text;
        }
    }
}
